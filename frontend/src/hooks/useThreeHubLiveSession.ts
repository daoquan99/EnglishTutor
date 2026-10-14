import { useEffect, useRef, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import { CefrLevel, DialogueTurnItem, LanguageCorrectionItem } from '../types/live';

const BACKEND_BASE_URL = import.meta.env.VITE_BACKEND_URL || 'http://localhost:5000';

function arrayBufferToBase64(buffer: ArrayBuffer): string {
  const bytes = new Uint8Array(buffer);
  let binary = '';
  for (let i = 0; i < bytes.byteLength; i++) {
    binary += String.fromCharCode(bytes[i]);
  }
  return btoa(binary);
}

export function useThreeHubLiveSession() {
  const [sessionId] = useState(() => `sess_${Math.random().toString(36).slice(2, 10)}`);
  const [isConnected, setIsConnected] = useState(false);
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

  // 3 Separate SignalR Hub Connections (FE -> BE)
  const tutorHubRef = useRef<signalR.HubConnection | null>(null);
  const transcribeHubRef = useRef<signalR.HubConnection | null>(null);
  const translateHubRef = useRef<signalR.HubConnection | null>(null);

  // Web Audio API Playback & Capture refs
  const playbackCtxRef = useRef<AudioContext | null>(null);
  const captureCtxRef = useRef<AudioContext | null>(null);
  const mediaStreamRef = useRef<MediaStream | null>(null);
  const nextPlayTimeRef = useRef<number>(0);
  const currentUserTurnIdRef = useRef<string>(`u_${Date.now()}`);

  const upsertTurn = useCallback(
    (
      turnId: string,
      speaker: 'user' | 'ai',
      patch: Partial<Pick<DialogueTurnItem, 'transcriptLine1' | 'isTranscriptFinal' | 'translationLine2'>>
    ) => {
      setTurns((prev) => {
        const idx = prev.findIndex((t) => t.turnId === turnId && t.speaker === speaker);
        if (idx === -1) {
          return [
            ...prev,
            {
              turnId,
              speaker,
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
        }
        const updated = [...prev];
        updated[idx] = {
          ...updated[idx],
          ...patch,
        };
        return updated;
      });
    },
    []
  );

  // Play 24kHz 16-bit mono PCM audio returned from Gemini 3.8 Live
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

    const source = ctx.createBufferSource();
    source.buffer = buffer;
    source.connect(ctx.destination);

    const now = ctx.currentTime;
    const startAt = Math.max(now, nextPlayTimeRef.current);
    source.start(startAt);
    nextPlayTimeRef.current = startAt + buffer.duration;
  }, []);

  const connectAllThreeHubs = useCallback(
    async (apiKey: string, cefrLevel: CefrLevel, extendedThinking: boolean) => {
      try {
        setErrorMessage(null);

        // 1. Tutor Hub (Gemini 3.8 Live / 3.8 Live Extended Thinking)
        const tutorConn = new signalR.HubConnectionBuilder()
          .withUrl(`${BACKEND_BASE_URL}/hubs/tutor`)
          .withAutomaticReconnect()
          .build();

        // 2. Dedicated Transcribe Live Hub (Gemini 3.5 Transcribe Live)
        const transcribeConn = new signalR.HubConnectionBuilder()
          .withUrl(`${BACKEND_BASE_URL}/hubs/transcribe`)
          .withAutomaticReconnect()
          .build();

        // 3. Dedicated Translate Live Hub (Gemini 3.5 Live Translate - Text Only)
        const translateConn = new signalR.HubConnectionBuilder()
          .withUrl(`${BACKEND_BASE_URL}/hubs/translate`)
          .withAutomaticReconnect()
          .build();

        // Tutor Hub listeners
        tutorConn.on(
          'ReceiveTutorAudio',
          (dto: { turnId: string; base64Pcm24k: string; interactionStatus: 'IDLE' | 'IN_PROGRESS' }) => {
            setInteractionStatus(dto.interactionStatus);
            enqueuePcm24kAudio(dto.base64Pcm24k);
          }
        );

        tutorConn.on('ReceiveLanguageCorrection', (correction: LanguageCorrectionItem) => {
          setCorrections((prev) => [correction, ...prev]);
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

        // Transcribe Hub listener (Line 1: Larger English Transcript)
        transcribeConn.on(
          'ReceiveTranscriptLine1',
          (dto: { speaker: 'user' | 'ai'; turnId: string; text: string; isFinal: boolean }) => {
            upsertTurn(dto.turnId, dto.speaker, {
              transcriptLine1: dto.text,
              isTranscriptFinal: dto.isFinal,
            });
          }
        );

        // Translate Hub listener (Line 2: Smaller Italic Vietnamese Text Only)
        translateConn.on(
          'ReceiveTranslationLine2',
          (dto: { speaker: 'user' | 'ai'; turnId: string; vietnameseText: string; isFinal: boolean }) => {
            upsertTurn(dto.turnId, dto.speaker, {
              translationLine2: dto.vietnameseText,
            });
          }
        );

        // Start all 3 SignalR hub connections in parallel
        await Promise.all([tutorConn.start(), transcribeConn.start(), translateConn.start()]);

        setHubStatuses({
          tutor: 'Connected',
          transcribe: 'Connected',
          translate: 'Connected',
        });

        // Initialize Google AI Studio sessions on each Hub
        await Promise.all([
          tutorConn.invoke('InitializeTutorSession', {
            sessionId,
            apiKey,
            cefrLevel,
            extendedThinking,
            thinkingLevel: 'medium',
            voiceName: 'Puck',
          }),
          transcribeConn.invoke('InitializeTranscribeSession', { sessionId, apiKey }),
          translateConn.invoke('InitializeTranslateSession', { sessionId, apiKey }),
        ]);

        tutorHubRef.current = tutorConn;
        transcribeHubRef.current = transcribeConn;
        translateHubRef.current = translateConn;
        setIsConnected(true);
      } catch (err: unknown) {
        const msg = err instanceof Error ? err.message : 'Không thể kết nối tới 3 SignalR Hubs trên Backend.';
        setErrorMessage(msg);
      }
    },
    [enqueuePcm24kAudio, sessionId, upsertTurn]
  );

  const startMicrophone = useCallback(async () => {
    if (!isConnected) return;
    currentUserTurnIdRef.current = `u_${Date.now()}`;

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

    try {
      await micCtx.audioWorklet.addModule('/pcm16k-capture-worklet.js');
      const workletNode = new AudioWorkletNode(micCtx, 'pcm16k-capture-processor');
      workletNode.port.onmessage = (event: MessageEvent<ArrayBuffer>) => {
        const base64Pcm16k = arrayBufferToBase64(event.data);
        const payload = {
          sessionId,
          speaker: 'user',
          turnId: currentUserTurnIdRef.current,
          base64Pcm16k,
        };
        tutorHubRef.current?.invoke('StreamUserAudioToTutor', payload);
      };
      source.connect(workletNode);
      workletNode.connect(micCtx.destination);
    } catch {
      // Fallback to ScriptProcessorNode if AudioWorklet is blocked
      const processor = micCtx.createScriptProcessor(2048, 1, 1);
      processor.onaudioprocess = (e) => {
        const floatSamples = e.inputBuffer.getChannelData(0);
        const pcm16 = new Int16Array(floatSamples.length);
        for (let i = 0; i < floatSamples.length; i++) {
          const s = Math.max(-1, Math.min(1, floatSamples[i]));
          pcm16[i] = s < 0 ? s * 0x8000 : s * 0x7fff;
        }
        const base64Pcm16k = arrayBufferToBase64(pcm16.buffer);
        const payload = {
          sessionId,
          speaker: 'user',
          turnId: currentUserTurnIdRef.current,
          base64Pcm16k,
        };
        tutorHubRef.current?.invoke('StreamUserAudioToTutor', payload);
      };
      source.connect(processor);
      processor.connect(micCtx.destination);
    }

    setIsRecording(true);
  }, [isConnected, sessionId]);

  const stopMicrophone = useCallback(async () => {
    mediaStreamRef.current?.getTracks().forEach((t) => t.stop());
    mediaStreamRef.current = null;
    if (captureCtxRef.current) {
      await captureCtxRef.current.close().catch(() => {});
      captureCtxRef.current = null;
    }
    setIsRecording(false);
    await tutorHubRef.current?.invoke('NotifyUserSpeechEnd', sessionId).catch(() => {});
  }, [sessionId]);

  const disconnectAll = useCallback(async () => {
    await stopMicrophone();
    await tutorHubRef.current?.invoke('StopSession', sessionId).catch(() => {});
    await Promise.all([
      tutorHubRef.current?.stop(),
      transcribeHubRef.current?.stop(),
      translateHubRef.current?.stop(),
    ]);
    setIsConnected(false);
    setHubStatuses({
      tutor: 'Disconnected',
      transcribe: 'Disconnected',
      translate: 'Disconnected',
    });
  }, [sessionId, stopMicrophone]);

  useEffect(() => {
    return () => {
      tutorHubRef.current?.stop();
      transcribeHubRef.current?.stop();
      translateHubRef.current?.stop();
    };
  }, []);

  return {
    sessionId,
    isConnected,
    isRecording,
    interactionStatus,
    errorMessage,
    hubStatuses,
    turns,
    corrections,
    connectAllThreeHubs,
    startMicrophone,
    stopMicrophone,
    disconnectAll,
  };
}
