import { useEffect, useRef, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import {
  CefrLevel,
  ConversationSummary,
  DialogueTurnItem,
  LanguageCorrectionItem,
  TeacherVoiceName,
} from '../types/live';

const BACKEND_BASE_URL = import.meta.env.VITE_BACKEND_URL || 'http://localhost:5000';

function arrayBufferToBase64(buffer: ArrayBuffer): string {
  const bytes = new Uint8Array(buffer);
  let binary = '';
  for (let i = 0; i < bytes.byteLength; i++) {
    binary += String.fromCharCode(bytes[i]);
  }
  return btoa(binary);
}

export function useThreeHubLiveSession(speechSpeed: number) {
  const [conversations, setConversations] = useState<ConversationSummary[]>([]);
  const [activeConversationId, setActiveConversationId] = useState<string>('');
  const [activeConversationTitle, setActiveConversationTitle] = useState<string>('Buổi luyện nói mới');
  const [isConnected, setIsConnected] = useState(false);
  const [isConnecting, setIsConnecting] = useState(false);
  const [isRecording, setIsRecording] = useState(false);
  const [interactionStatus, setInteractionStatus] = useState<'IDLE' | 'IN_PROGRESS'>('IDLE');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [hubStatuses, setHubStatuses] = useState({
    tutor: 'Disconnected',
    transcribe: 'Disconnected',
    translate: 'Disconnected',
  });

  const [turns, setTurns] = useState<DialogueTurnItem[]>([]);
  const [corrections, setCorrections] = useState<LanguageCorrectionItem[]>([]);

  const speechSpeedRef = useRef<number>(speechSpeed);
  useEffect(() => {
    speechSpeedRef.current = speechSpeed;
  }, [speechSpeed]);

  // 3 Separate SignalR Hub Connections (FE -> BE)
  const tutorHubRef = useRef<signalR.HubConnection | null>(null);
  const transcribeHubRef = useRef<signalR.HubConnection | null>(null);
  const translateHubRef = useRef<signalR.HubConnection | null>(null);

  // Web Audio API Playback & Capture refs
  const playbackCtxRef = useRef<AudioContext | null>(null);
  const captureCtxRef = useRef<AudioContext | null>(null);
  const mediaStreamRef = useRef<MediaStream | null>(null);
  const nextPlayTimeRef = useRef<number>(0);

  const fetchConversations = useCallback(async () => {
    try {
      const res = await fetch(`${BACKEND_BASE_URL}/api/conversations`);
      if (!res.ok) return;
      const data: ConversationSummary[] = await res.json();
      setConversations(data);
      if (!activeConversationId && data.length > 0) {
        setActiveConversationId(data[0].id);
        setActiveConversationTitle(data[0].title);
      }
    } catch {
      // Backend might not be running yet
    }
  }, [activeConversationId]);

  const selectConversation = useCallback(async (conversationId: string) => {
    setActiveConversationId(conversationId);
    try {
      const res = await fetch(`${BACKEND_BASE_URL}/api/conversations/${conversationId}`);
      if (!res.ok) return null;
      const data = await res.json();
      setActiveConversationTitle(data.title || 'Buổi luyện nói');

      const loadedTurns: DialogueTurnItem[] = (data.messages || [])
        .map((m: any, idx: number) => ({
          turnId: m.turnId,
          speaker: m.speaker,
          sequenceOrder: typeof m.sequenceOrder === 'number' ? m.sequenceOrder : idx + 1,
          transcriptLine1: m.transcriptLine1 || '',
          isTranscriptFinal: Boolean(m.isFinal),
          translationLine2: m.translationLine2 || '',
          timestamp: new Date(m.createdAt).toLocaleTimeString('vi-VN', {
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
          }),
        }))
        .sort((a: DialogueTurnItem, b: DialogueTurnItem) => a.sequenceOrder - b.sequenceOrder);

      const loadedCorrections: LanguageCorrectionItem[] = (data.corrections || []).map((c: any) => ({
        id: c.id,
        turnId: c.turnId,
        originalText: c.originalText,
        correctedText: c.correctedText,
        errorCategory: c.errorCategory,
        explanationVi: c.explanationVi,
        cefrTip: c.cefrTip,
        timestamp: new Date(c.createdAt).toLocaleTimeString('vi-VN', {
          hour: '2-digit',
          minute: '2-digit',
          second: '2-digit',
        }),
      }));

      setTurns(loadedTurns);
      setCorrections(loadedCorrections);
      return data;
    } catch {
      return null;
    }
  }, []);

  const createNewConversation = useCallback(
    async (
      cefrLevel: CefrLevel,
      extendedThinking: boolean,
      voiceName: TeacherVoiceName,
      speed: number
    ) => {
      try {
        const title = `Hội thoại ${cefrLevel} (${new Date().toLocaleTimeString('vi-VN', {
          hour: '2-digit',
          minute: '2-digit',
        })})`;
        const res = await fetch(`${BACKEND_BASE_URL}/api/conversations`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            title,
            cefrLevel,
            extendedThinking,
            voiceName,
            speechSpeed: speed,
          }),
        });
        if (res.ok) {
          const created = await res.json();
          await fetchConversations();
          setActiveConversationId(created.id);
          setActiveConversationTitle(created.title || title);
          setTurns([]);
          setCorrections([]);
          return created.id as string;
        }
      } catch {
        // Fallback local ID
      }
      const fallbackId = `conv_${Math.random().toString(36).slice(2, 10)}`;
      setActiveConversationId(fallbackId);
      setActiveConversationTitle(`Hội thoại ${cefrLevel} mới`);
      setTurns([]);
      setCorrections([]);
      return fallbackId;
    },
    [fetchConversations]
  );

  const deleteConversation = useCallback(
    async (conversationId: string) => {
      try {
        await fetch(`${BACKEND_BASE_URL}/api/conversations/${conversationId}`, {
          method: 'DELETE',
        });
        await fetchConversations();
        if (activeConversationId === conversationId) {
          setTurns([]);
          setCorrections([]);
          setActiveConversationId('');
          setActiveConversationTitle('Buổi luyện nói mới');
        }
      } catch {
        // Ignore
      }
    },
    [activeConversationId, fetchConversations]
  );

  // Upsert turn and sort strictly by SequenceOrder so User turn is always before AI reply
  const upsertTurn = useCallback(
    (
      turnId: string,
      speaker: 'user' | 'ai',
      sequenceOrder: number,
      patch: Partial<Pick<DialogueTurnItem, 'transcriptLine1' | 'isTranscriptFinal' | 'translationLine2'>>
    ) => {
      setTurns((prev) => {
        const idx = prev.findIndex((t) => t.turnId === turnId && t.speaker === speaker);
        let nextList: DialogueTurnItem[];
        if (idx === -1) {
          nextList = [
            ...prev,
            {
              turnId,
              speaker,
              sequenceOrder: sequenceOrder || prev.length + 1,
              transcriptLine1: patch.transcriptLine1 ?? '',
              isTranscriptFinal: patch.isTranscriptFinal ?? false,
              translationLine2: patch.translationLine2 ?? '',
              timestamp: new Date().toLocaleTimeString('vi-VN', {
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit',
              }),
            },
          ];
        } else {
          nextList = [...prev];
          nextList[idx] = {
            ...nextList[idx],
            sequenceOrder: sequenceOrder || nextList[idx].sequenceOrder,
            ...(patch.transcriptLine1 !== undefined ? { transcriptLine1: patch.transcriptLine1 } : {}),
            ...(patch.isTranscriptFinal !== undefined ? { isTranscriptFinal: patch.isTranscriptFinal } : {}),
            ...(patch.translationLine2 !== undefined ? { translationLine2: patch.translationLine2 } : {}),
          };
        }
        return [...nextList].sort((a, b) => a.sequenceOrder - b.sequenceOrder);
      });
    },
    []
  );

  const enqueuePcm24kAudio = useCallback((base64Pcm24k: string) => {
    if (!playbackCtxRef.current) {
      playbackCtxRef.current = new AudioContext({ sampleRate: 24000 });
    }
    const ctx = playbackCtxRef.current;
    const binary = atob(base64Pcm24k);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
      bytes[i] = binary.charCodeAt(i);
    }
    const int16 = new Int16Array(bytes.buffer);
    const float32 = new Float32Array(int16.length);
    for (let i = 0; i < int16.length; i++) {
      float32[i] = int16[i] / 32768.0;
    }

    const buffer = ctx.createBuffer(1, float32.length, 24000);
    buffer.getChannelData(0).set(float32);

    const rate = Math.max(0.5, Math.min(2.0, speechSpeedRef.current || 1.0));
    const source = ctx.createBufferSource();
    source.buffer = buffer;
    source.playbackRate.value = rate;
    source.connect(ctx.destination);

    const now = ctx.currentTime;
    const startAt = Math.max(now, nextPlayTimeRef.current);
    source.start(startAt);
    nextPlayTimeRef.current = startAt + buffer.duration / rate;
  }, []);

  const disconnectAll = useCallback(async () => {
    mediaStreamRef.current?.getTracks().forEach((t) => t.stop());
    mediaStreamRef.current = null;
    if (captureCtxRef.current) {
      await captureCtxRef.current.close().catch(() => {});
      captureCtxRef.current = null;
    }
    setIsRecording(false);

    if (activeConversationId) {
      await tutorHubRef.current?.invoke('StopSession', activeConversationId).catch(() => {});
    }
    await Promise.all([
      tutorHubRef.current?.stop().catch(() => {}),
      transcribeHubRef.current?.stop().catch(() => {}),
      translateHubRef.current?.stop().catch(() => {}),
    ]);
    tutorHubRef.current = null;
    transcribeHubRef.current = null;
    translateHubRef.current = null;
    setIsConnected(false);
    setHubStatuses({
      tutor: 'Disconnected',
      transcribe: 'Disconnected',
      translate: 'Disconnected',
    });
  }, [activeConversationId]);

  const connectAllThreeHubs = useCallback(
    async (
      apiKey: string,
      cefrLevel: CefrLevel,
      extendedThinking: boolean,
      voiceName: TeacherVoiceName,
      speed: number,
      targetConversationId?: string
    ) => {
      if (isConnecting) return;
      try {
        setIsConnecting(true);
        setErrorMessage(null);

        // Clean up any existing connections first
        await disconnectAll();

        let convId = targetConversationId || activeConversationId;
        if (!convId) {
          convId = await createNewConversation(cefrLevel, extendedThinking, voiceName, speed);
        } else {
          await selectConversation(convId);
        }

        const tutorConn = new signalR.HubConnectionBuilder()
          .withUrl(`${BACKEND_BASE_URL}/hubs/tutor`)
          .withAutomaticReconnect()
          .build();

        const transcribeConn = new signalR.HubConnectionBuilder()
          .withUrl(`${BACKEND_BASE_URL}/hubs/transcribe`)
          .withAutomaticReconnect()
          .build();

        const translateConn = new signalR.HubConnectionBuilder()
          .withUrl(`${BACKEND_BASE_URL}/hubs/translate`)
          .withAutomaticReconnect()
          .build();

        tutorConn.on(
          'ReceiveTutorAudio',
          (dto: { turnId: string; base64Pcm24k: string; interactionStatus: 'IDLE' | 'IN_PROGRESS' }) => {
            setInteractionStatus(dto.interactionStatus);
            enqueuePcm24kAudio(dto.base64Pcm24k);
          }
        );

        tutorConn.on('ReceiveLanguageCorrection', (correction: LanguageCorrectionItem) => {
          setCorrections((prev) => [
            {
              ...correction,
              timestamp: new Date().toLocaleTimeString('vi-VN', {
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit',
              }),
            },
            ...prev,
          ]);
        });

        tutorConn.on('ReceiveInteractionStatus', (payload: { status: 'IDLE' | 'IN_PROGRESS' }) => {
          setInteractionStatus(payload.status);
        });

        tutorConn.on('TutorInterrupted', () => {
          if (playbackCtxRef.current) {
            playbackCtxRef.current.close();
            playbackCtxRef.current = new AudioContext({ sampleRate: 24000 });
            nextPlayTimeRef.current = 0;
          }
        });

        transcribeConn.on(
          'ReceiveTranscriptLine1',
          (dto: {
            speaker: 'user' | 'ai';
            turnId: string;
            sequenceOrder: number;
            fullTranscriptText: string;
            isFinal: boolean;
          }) => {
            upsertTurn(dto.turnId, dto.speaker, dto.sequenceOrder, {
              transcriptLine1: dto.fullTranscriptText,
              isTranscriptFinal: dto.isFinal,
            });
            if (dto.isFinal) {
              fetchConversations();
            }
          }
        );

        translateConn.on(
          'ReceiveTranslationLine2',
          (dto: {
            speaker: 'user' | 'ai';
            turnId: string;
            sequenceOrder: number;
            fullVietnameseText: string;
            isFinal: boolean;
          }) => {
            upsertTurn(dto.turnId, dto.speaker, dto.sequenceOrder, {
              translationLine2: dto.fullVietnameseText,
            });
          }
        );

        await Promise.all([tutorConn.start(), transcribeConn.start(), translateConn.start()]);

        // Initialize Tutor session first, then Sub-Hubs sequentially to prevent Google WebSocket burst throttling
        await tutorConn.invoke('InitializeTutorSession', {
          conversationId: convId,
          apiKey,
          cefrLevel,
          extendedThinking,
          thinkingLevel: 'medium',
          voiceName,
          speechSpeed: speed,
        });
        setHubStatuses((prev) => ({ ...prev, tutor: 'Connected' }));

        await transcribeConn.invoke('InitializeTranscribeSession', {
          conversationId: convId,
          apiKey,
        });
        setHubStatuses((prev) => ({ ...prev, transcribe: 'Connected' }));

        await translateConn.invoke('InitializeTranslateSession', {
          conversationId: convId,
          apiKey,
        });
        setHubStatuses({
          tutor: 'Connected',
          transcribe: 'Connected',
          translate: 'Connected',
        });

        tutorHubRef.current = tutorConn;
        transcribeHubRef.current = transcribeConn;
        translateHubRef.current = translateConn;
        setIsConnected(true);
        await fetchConversations();
      } catch (err: unknown) {
        const msg =
          err instanceof Error
            ? err.message
            : 'Không thể kết nối tới Backend hoặc Google AI Studio. Vui lòng kiểm tra API Key và SQL Server.';
        setErrorMessage(msg);
      } finally {
        setIsConnecting(false);
      }
    },
    [
      activeConversationId,
      createNewConversation,
      disconnectAll,
      enqueuePcm24kAudio,
      fetchConversations,
      isConnecting,
      selectConversation,
      upsertTurn,
    ]
  );

  const startMicrophone = useCallback(async () => {
    if (!isConnected || !activeConversationId) return;

    const stream = await navigator.mediaDevices.getUserMedia({
      audio: {
        sampleRate: 16000,
        channelCount: 1,
        echoCancellation: true,
        noiseSuppression: true,
      },
    });
    mediaStreamRef.current = stream;

    const micCtx = new AudioContext({ sampleRate: 16000 });
    captureCtxRef.current = micCtx;
    const source = micCtx.createMediaStreamSource(stream);

    const handleAudioChunk = (pcmBuffer: ArrayBuffer) => {
      const base64Pcm16k = arrayBufferToBase64(pcmBuffer);
      const payload = {
        conversationId: activeConversationId,
        speaker: 'user',
        turnId: null,
        base64Pcm16k,
      };
      // Stream 16kHz PCM audio from FE to all 3 SignalR hubs independently!
      tutorHubRef.current?.invoke('StreamUserAudioToTutor', payload).catch(() => {});
      transcribeHubRef.current?.invoke('StreamAudioForTranscription', payload).catch(() => {});
      translateHubRef.current?.invoke('StreamAudioForTranslation', payload).catch(() => {});
    };

    try {
      await micCtx.audioWorklet.addModule('/pcm16k-capture-worklet.js');
      const workletNode = new AudioWorkletNode(micCtx, 'pcm16k-capture-processor');
      workletNode.port.onmessage = (event: MessageEvent<{ pcmBuffer: ArrayBuffer }>) => {
        handleAudioChunk(event.data.pcmBuffer);
      };
      source.connect(workletNode);
      workletNode.connect(micCtx.destination);
    } catch {
      const processor = micCtx.createScriptProcessor(2048, 1, 1);
      processor.onaudioprocess = (e) => {
        const floatSamples = e.inputBuffer.getChannelData(0);
        const pcm16 = new Int16Array(floatSamples.length);
        for (let i = 0; i < floatSamples.length; i++) {
          const s = Math.max(-1, Math.min(1, floatSamples[i]));
          pcm16[i] = s < 0 ? s * 0x8000 : s * 0x7fff;
        }
        handleAudioChunk(pcm16.buffer);
      };
      source.connect(processor);
      processor.connect(micCtx.destination);
    }

    setIsRecording(true);
  }, [activeConversationId, isConnected]);

  const stopMicrophone = useCallback(async () => {
    mediaStreamRef.current?.getTracks().forEach((t) => t.stop());
    mediaStreamRef.current = null;
    if (captureCtxRef.current) {
      await captureCtxRef.current.close().catch(() => {});
      captureCtxRef.current = null;
    }
    setIsRecording(false);
    if (activeConversationId) {
      await Promise.all([
        tutorHubRef.current?.invoke('NotifyUserSpeechEnd', activeConversationId).catch(() => {}),
        transcribeHubRef.current?.invoke('EndAudioTurn', activeConversationId).catch(() => {}),
        translateHubRef.current?.invoke('EndTranslateAudioTurn', activeConversationId).catch(() => {}),
      ]);
    }
  }, [activeConversationId]);

  useEffect(() => {
    fetchConversations();
    return () => {
      tutorHubRef.current?.stop();
      transcribeHubRef.current?.stop();
      translateHubRef.current?.stop();
    };
  }, [fetchConversations]);

  const requestManualGrammarCheck = useCallback(
    async (turnId: string, transcriptText: string, cefrLevel: CefrLevel) => {
      if (!activeConversationId || !transcriptText.trim()) return;
      await tutorHubRef.current
        ?.invoke('AnalyzeTurnGrammar', activeConversationId, turnId, transcriptText, cefrLevel)
        .catch(() => {});
    },
    [activeConversationId]
  );

  return {
    conversations,
    activeConversationId,
    activeConversationTitle,
    isConnected,
    isConnecting,
    isRecording,
    interactionStatus,
    errorMessage,
    hubStatuses,
    turns,
    corrections,
    fetchConversations,
    selectConversation,
    createNewConversation,
    deleteConversation,
    connectAllThreeHubs,
    startMicrophone,
    stopMicrophone,
    disconnectAll,
    requestManualGrammarCheck,
  };
}
