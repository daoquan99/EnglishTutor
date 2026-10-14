import React, { useCallback, useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import {
  AiModelCatalogItem,
  DailyVocabularyPlanResponseDto,
  PronunciationAssessmentResultDto,
  VocabularyFlashcardDto,
} from '../types/vocabulary';
import { CefrLevel } from '../types/live';

const BACKEND_BASE_URL = import.meta.env.VITE_BACKEND_URL || 'http://localhost:5000';

// Fallback catalog if backend is not yet started
const FALLBACK_MODELS: AiModelCatalogItem[] = [
  {
    id: '10000000-0000-0000-0000-000000000008',
    displayName: 'Gemini 3.8 Flash (Recommended)',
    modelCode: 'gemini-3.8-flash',
    supportsTextGeneration: true,
    supportsStructuredOutput: true,
    supportsAudioInput: true,
    supportsPronunciationAssessment: true,
    supportsLiveAudio: false,
    supportsTranscription: true,
    supportsTranslation: true,
    supportsTts: false,
    supportsStreaming: true,
    isAvailable: true,
    isDeprecated: false,
    isEnabled: true,
    capabilityNote: 'Model mặc định tốc độ cao cho tạo từ vựng và chấm điểm phát âm trực tiếp.',
    displayOrder: 1,
  },
  {
    id: '10000000-0000-0000-0000-000000000006',
    displayName: 'Gemini 3.6 Flash',
    modelCode: 'gemini-3.6-flash',
    supportsTextGeneration: true,
    supportsStructuredOutput: true,
    supportsAudioInput: true,
    supportsPronunciationAssessment: true,
    supportsLiveAudio: false,
    supportsTranscription: true,
    supportsTranslation: true,
    supportsTts: false,
    supportsStreaming: true,
    isAvailable: true,
    isDeprecated: false,
    isEnabled: true,
    capabilityNote: 'Hỗ trợ tạo từ vựng và chấm điểm phát âm đa phương thức.',
    displayOrder: 2,
  },
  {
    id: '10000000-0000-0000-0000-000000000005',
    displayName: 'Gemini 3.5 Flash Lite',
    modelCode: 'gemini-3.5-flash-lite',
    supportsTextGeneration: true,
    supportsStructuredOutput: true,
    supportsAudioInput: true,
    supportsPronunciationAssessment: true,
    supportsLiveAudio: false,
    supportsTranscription: true,
    supportsTranslation: true,
    supportsTts: false,
    supportsStreaming: true,
    isAvailable: true,
    isDeprecated: false,
    isEnabled: true,
    capabilityNote: 'Nhanh, tiết kiệm quota, hỗ trợ cả tạo từ vựng và chấm phát âm.',
    displayOrder: 3,
  },
  {
    id: '10000000-0000-0000-0000-000000000004',
    displayName: 'Gemini 3.1 Flash Lite',
    modelCode: 'gemini-3.1-flash-lite',
    supportsTextGeneration: true,
    supportsStructuredOutput: true,
    supportsAudioInput: true,
    supportsPronunciationAssessment: true,
    supportsLiveAudio: false,
    supportsTranscription: true,
    supportsTranslation: true,
    supportsTts: false,
    supportsStreaming: true,
    isAvailable: true,
    isDeprecated: false,
    isEnabled: true,
    displayOrder: 4,
  },
  {
    id: '10000000-0000-0000-0000-000000000003',
    displayName: 'Gemini 3 Flash Preview',
    modelCode: 'gemini-3-flash-preview',
    supportsTextGeneration: true,
    supportsStructuredOutput: true,
    supportsAudioInput: true,
    supportsPronunciationAssessment: true,
    supportsLiveAudio: false,
    supportsTranscription: true,
    supportsTranslation: true,
    supportsTts: false,
    supportsStreaming: true,
    isAvailable: true,
    isDeprecated: false,
    isEnabled: true,
    displayOrder: 5,
  },
  {
    id: '10000000-0000-0000-0000-000000000001',
    displayName: 'Gemini 2.5 Flash',
    modelCode: 'gemini-2.5-flash',
    supportsTextGeneration: true,
    supportsStructuredOutput: true,
    supportsAudioInput: true,
    supportsPronunciationAssessment: true,
    supportsLiveAudio: false,
    supportsTranscription: true,
    supportsTranslation: true,
    supportsTts: false,
    supportsStreaming: true,
    isAvailable: true,
    isDeprecated: false,
    isEnabled: true,
    capabilityNote: 'Có thể giới hạn theo tài khoản Google AI Studio.',
    displayOrder: 6,
  },
  {
    id: '10000000-0000-0000-0000-000000000002',
    displayName: 'Gemini 2.5 Flash Lite',
    modelCode: 'gemini-2.5-flash-lite',
    supportsTextGeneration: true,
    supportsStructuredOutput: true,
    supportsAudioInput: true,
    supportsPronunciationAssessment: true,
    supportsLiveAudio: false,
    supportsTranscription: true,
    supportsTranslation: true,
    supportsTts: false,
    supportsStreaming: true,
    isAvailable: true,
    isDeprecated: false,
    isEnabled: true,
    displayOrder: 7,
  },
  {
    id: '10000000-0000-0000-0000-000000000007',
    displayName: 'Gemini 3.7 Flash (Deprecated → 3.8 Flash)',
    modelCode: 'gemini-3.7-flash',
    supportsTextGeneration: true,
    supportsStructuredOutput: true,
    supportsAudioInput: true,
    supportsPronunciationAssessment: true,
    supportsLiveAudio: false,
    supportsTranscription: true,
    supportsTranslation: true,
    supportsTts: false,
    supportsStreaming: true,
    isAvailable: true,
    isDeprecated: true,
    isEnabled: true,
    replacementModelCode: 'gemini-3.8-flash',
    capabilityNote: 'Đã ngừng khuyến nghị (Deprecated) — tự động định tuyến sang gemini-3.8-flash.',
    displayOrder: 8,
  },
  {
    id: '10000000-0000-0000-0000-000000000009',
    displayName: 'Gemma 4 26B (Text Only)',
    modelCode: 'gemma-4-26b-a4b-it',
    supportsTextGeneration: true,
    supportsStructuredOutput: true,
    supportsAudioInput: false,
    supportsPronunciationAssessment: false,
    supportsLiveAudio: false,
    supportsTranscription: false,
    supportsTranslation: true,
    supportsTts: false,
    supportsStreaming: true,
    isAvailable: true,
    isDeprecated: false,
    isEnabled: true,
    capabilityNote: 'Chỉ hỗ trợ tạo từ vựng văn bản; KHÔNG hỗ trợ đầu vào âm thanh chấm phát âm.',
    displayOrder: 9,
  },
  {
    id: '10000000-0000-0000-0000-000000000010',
    displayName: 'Gemma 4 31B (Text Only)',
    modelCode: 'gemma-4-31b-it',
    supportsTextGeneration: true,
    supportsStructuredOutput: true,
    supportsAudioInput: false,
    supportsPronunciationAssessment: false,
    supportsLiveAudio: false,
    supportsTranscription: false,
    supportsTranslation: true,
    supportsTts: false,
    supportsStreaming: true,
    isAvailable: true,
    isDeprecated: false,
    isEnabled: true,
    capabilityNote: 'Chỉ hỗ trợ tạo từ vựng văn bản; KHÔNG hỗ trợ đầu vào âm thanh chấm phát âm.',
    displayOrder: 10,
  },
];

interface Props {
  apiKey: string;
  cefrLevel: CefrLevel;
  defaultTopic: string;
  onFocusApiKey: () => void;
}

export const DailyVocabularyWorkspace: React.FC<Props> = ({
  apiKey,
  cefrLevel,
  defaultTopic,
  onFocusApiKey,
}) => {
  const [models, setModels] = useState<AiModelCatalogItem[]>(FALLBACK_MODELS);
  const [generationModel, setGenerationModel] = useState<string>('gemini-3.8-flash');
  const [assessmentModel, setAssessmentModel] = useState<string>('gemini-3.8-flash');

  const [dailyTarget, setDailyTarget] = useState<number>(20);
  const [genTopic, setGenTopic] = useState<string>(defaultTopic || 'Software Engineering & Daily Communication');
  const [genCount, setGenCount] = useState<number>(3);

  const [plan, setPlan] = useState<DailyVocabularyPlanResponseDto | null>(null);
  const [currentIndex, setCurrentIndex] = useState<number>(0);
  const [isFlipped, setIsFlipped] = useState<boolean>(false);

  const [isLoadingPlan, setIsLoadingPlan] = useState<boolean>(false);
  const [isGeneratingVocab, setIsGeneratingVocab] = useState<boolean>(false);
  const [isSubmittingReview, setIsSubmittingReview] = useState<boolean>(false);
  const [statusBanner, setStatusBanner] = useState<{ type: 'info' | 'error' | 'success'; text: string } | null>(null);

  // Pronunciation Practice & Recording State
  const [practiceTargetType, setPracticeTargetType] = useState<'Word' | 'Sentence'>('Word');
  const [selectedExampleId, setSelectedExampleId] = useState<string | null>(null);
  const [practiceAccent, setPracticeAccent] = useState<'US' | 'UK'>('US');

  // Hub 2 (Live Speech) & Hub 3 (Vietnamese Live Translation) Toggles (Sections 12, 13, 14)
  const [enableLiveSpeechHub2, setEnableLiveSpeechHub2] = useState<boolean>(true);
  const [enableLiveTranslateHub3, setEnableLiveTranslateHub3] = useState<boolean>(false); // Default Disabled per Section 13
  const [liveTranscriptLine1, setLiveTranscriptLine1] = useState<string>('');
  const [liveTranslationLine2, setLiveTranslationLine2] = useState<string>('');
  const [hub2Status, setHub2Status] = useState<string>('Idle');
  const [hub3Status, setHub3Status] = useState<string>('Disabled');

  // Unified Microphone & MediaRecorder refs
  const [isRecordingAudio, setIsRecordingAudio] = useState<boolean>(false);
  const [recordingDurationMs, setRecordingDurationMs] = useState<number>(0);
  const [micInputLevel, setMicInputLevel] = useState<number>(0);
  const [recordedAudioUrl, setRecordedAudioUrl] = useState<string | null>(null);
  const [recordedBase64, setRecordedBase64] = useState<string | null>(null);
  const [recordedMimeType, setRecordedMimeType] = useState<string>('audio/webm');

  const [isScoringAudio, setIsScoringAudio] = useState<boolean>(false);
  const [assessmentResult, setAssessmentResult] = useState<PronunciationAssessmentResultDto | null>(null);
  const [assessmentHistory, setAssessmentHistory] = useState<PronunciationAssessmentResultDto[]>([]);

  const mediaStreamRef = useRef<MediaStream | null>(null);
  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const audioChunksRef = useRef<Blob[]>([]);
  const audioCtxRef = useRef<AudioContext | null>(null);
  const timerIntervalRef = useRef<number | null>(null);
  const startTimeRef = useRef<number>(0);

  const transcribeHubRef = useRef<signalR.HubConnection | null>(null);
  const translateHubRef = useRef<signalR.HubConnection | null>(null);

  const flashcards = plan?.flashcards ?? [];
  const activeCard: VocabularyFlashcardDto | undefined = flashcards[currentIndex];

  const loadModelsAndTodayPlan = useCallback(async (targetOverride?: number) => {
    setIsLoadingPlan(true);
    try {
      const [modelsRes, planRes] = await Promise.all([
        fetch(`${BACKEND_BASE_URL}/api/pronunciation/models`).catch(() => null),
        fetch(
          `${BACKEND_BASE_URL}/api/learning/flashcards/today?timeZoneId=Asia/Ho_Chi_Minh&target=${
            targetOverride ?? dailyTarget
          }`
        ).catch(() => null),
      ]);

      if (modelsRes && modelsRes.ok) {
        const loadedModels: AiModelCatalogItem[] = await modelsRes.json();
        if (loadedModels.length > 0) setModels(loadedModels);
      }

      if (planRes && planRes.ok) {
        const loadedPlan: DailyVocabularyPlanResponseDto = await planRes.json();
        setPlan(loadedPlan);
      }
    } finally {
      setIsLoadingPlan(false);
    }
  }, [dailyTarget]);

  useEffect(() => {
    loadModelsAndTodayPlan();
  }, [loadModelsAndTodayPlan]);

  // Reset per-card states when switching flashcards
  useEffect(() => {
    setIsFlipped(false);
    setPracticeTargetType('Word');
    setSelectedExampleId(activeCard?.examples?.[0]?.id ?? null);
    setAssessmentResult(null);
    setRecordedAudioUrl(null);
    setRecordedBase64(null);
    setLiveTranscriptLine1('');
    setLiveTranslationLine2('');
  }, [currentIndex, activeCard?.senseId]);

  // Speak reference word or sentence via AudioUrl or Web Speech API
  const playReferenceAudio = (text: string, accent: 'UK' | 'US', audioUrl?: string | null) => {
    if (audioUrl) {
      const audio = new Audio(audioUrl);
      audio.play().catch(() => {});
      return;
    }
    if ('speechSynthesis' in window) {
      window.speechSynthesis.cancel();
      const utter = new SpeechSynthesisUtterance(text);
      utter.lang = accent === 'UK' ? 'en-GB' : 'en-US';
      utter.rate = 0.92;
      window.speechSynthesis.speak(utter);
    }
  };

  // Generate new vocabulary via selected AI model (Section 6)
  const handleGenerateAiVocabulary = async () => {
    if (!apiKey.trim()) {
      setStatusBanner({
        type: 'error',
        text: 'Vui lòng nhập Google AI Studio API Key ở thanh bên trái trước khi tạo từ vựng bằng AI.',
      });
      onFocusApiKey();
      return;
    }

    setIsGeneratingVocab(true);
    setStatusBanner({
      type: 'info',
      text: `Đang tạo ${genCount} từ vựng mới trình độ ${cefrLevel} bằng model ${generationModel}...`,
    });

    try {
      const res = await fetch(`${BACKEND_BASE_URL}/api/vocabularies/generate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          apiKey,
          modelCode: generationModel,
          requestedCount: genCount,
          cefrLevel,
          topic: genTopic,
          assignToTodayPlan: true,
        }),
      });

      const data = await res.json();
      if (!res.ok) {
        throw new Error(data?.error || 'Không thể tạo từ vựng bằng AI.');
      }

      await loadModelsAndTodayPlan();
      setStatusBanner({
        type: 'success',
        text: `Đã tạo và bổ sung ${Array.isArray(data) ? data.length : genCount} thẻ từ vựng mới vào kế hoạch hôm nay!`,
      });
    } catch (err: any) {
      setStatusBanner({
        type: 'error',
        text: err?.message || 'Lỗi khi gọi API tạo từ vựng.',
      });
    } finally {
      setIsGeneratingVocab(false);
    }
  };

  // Submit Spaced Repetition (SM-2) rating: Again | Hard | Good | Easy (Section 7.4)
  const handleSubmitReviewRating = async (rating: 'Again' | 'Hard' | 'Good' | 'Easy') => {
    if (!activeCard) return;
    setIsSubmittingReview(true);
    try {
      const res = await fetch(
        `${BACKEND_BASE_URL}/api/learning/flashcards/${activeCard.dailyItemId}/review`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            rating,
            timeZoneId: 'Asia/Ho_Chi_Minh',
          }),
        }
      );

      if (res.ok) {
        const updatedCard: VocabularyFlashcardDto = await res.json();
        setPlan((prev) => {
          if (!prev) return prev;
          const updatedList = prev.flashcards.map((c) =>
            c.dailyItemId === updatedCard.dailyItemId ? updatedCard : c
          );
          return {
            ...prev,
            completedCount: updatedList.filter((c) => c.itemStatus === 'Completed').length,
            flashcards: updatedList,
          };
        });
        setStatusBanner({
          type: 'success',
          text: `Đã lưu đánh giá "${rating}" cho từ "${activeCard.word}" (Mastery: ${updatedCard.masteryLevel}%, ôn tập tiếp sau ${updatedCard.intervalDays} ngày).`,
        });

        if (currentIndex < flashcards.length - 1) {
          setTimeout(() => setCurrentIndex((i) => Math.min(flashcards.length - 1, i + 1)), 350);
        }
      }
    } finally {
      setIsSubmittingReview(false);
    }
  };

  // Unified Microphone Capture Pipeline (Section 14):
  // Simultaneously records MediaRecorder audio blob for Pronunciation Assessment AND streams 16kHz PCM to Hub 2 / Hub 3 when enabled.
  const startUnifiedRecording = async () => {
    try {
      setAssessmentResult(null);
      setRecordedAudioUrl(null);
      setRecordedBase64(null);
      setLiveTranscriptLine1('');
      setLiveTranslationLine2('');

      const stream = await navigator.mediaDevices.getUserMedia({
        audio: {
          sampleRate: 16000,
          channelCount: 1,
          echoCancellation: true,
          noiseSuppression: true,
        },
      });
      mediaStreamRef.current = stream;

      // Determine supported browser MIME type at runtime (Section 10.1)
      const candidates = ['audio/webm;codecs=opus', 'audio/webm', 'audio/mp4', 'audio/ogg'];
      const chosenMime = candidates.find((m) => MediaRecorder.isTypeSupported(m)) || '';
      setRecordedMimeType(chosenMime || 'audio/webm');

      const recorder = chosenMime
        ? new MediaRecorder(stream, { mimeType: chosenMime })
        : new MediaRecorder(stream);

      audioChunksRef.current = [];
      recorder.ondataavailable = (ev) => {
        if (ev.data && ev.data.size > 0) {
          audioChunksRef.current.push(ev.data);
        }
      };

      recorder.onstop = () => {
        const blob = new Blob(audioChunksRef.current, { type: chosenMime || 'audio/webm' });
        const url = URL.createObjectURL(blob);
        setRecordedAudioUrl(url);

        const reader = new FileReader();
        reader.onloadend = () => {
          const dataUrl = reader.result as string;
          const base64 = dataUrl.includes(',') ? dataUrl.split(',')[1] : dataUrl;
          setRecordedBase64(base64);
        };
        reader.readAsDataURL(blob);
      };

      recorder.start(150);
      mediaRecorderRef.current = recorder;

      // Optional Realtime Hub 2 (Transcribe) & Hub 3 (Translate VI) streaming
      const flashcardSessionId = `fc_${Date.now().toString(36)}`;
      const turnId = `t_${Date.now()}`;

      if (apiKey.trim() && (enableLiveSpeechHub2 || enableLiveTranslateHub3)) {
        if (enableLiveSpeechHub2) {
          setHub2Status('Connecting...');
          const trConn = new signalR.HubConnectionBuilder()
            .withUrl(`${BACKEND_BASE_URL}/hubs/transcribe`)
            .withAutomaticReconnect()
            .build();

          trConn.on('ReceiveTranscriptLine1', (dto: { text: string }) => {
            if (dto.text) setLiveTranscriptLine1(dto.text);
          });

          await trConn.start();
          await trConn.invoke('InitializeTranscribeSession', {
            sessionId: flashcardSessionId,
            apiKey,
          });
          transcribeHubRef.current = trConn;
          setHub2Status('Streaming (3.5 Transcribe)');
        }

        if (enableLiveTranslateHub3) {
          setHub3Status('Connecting...');
          const tlConn = new signalR.HubConnectionBuilder()
            .withUrl(`${BACKEND_BASE_URL}/hubs/translate`)
            .withAutomaticReconnect()
            .build();

          tlConn.on('ReceiveTranslationLine2', (dto: { vietnameseText: string }) => {
            if (dto.vietnameseText) setLiveTranslationLine2(dto.vietnameseText);
          });

          await tlConn.start();
          await tlConn.invoke('InitializeTranslateSession', {
            sessionId: flashcardSessionId,
            apiKey,
          });
          translateHubRef.current = tlConn;
          setHub3Status('Streaming (3.5 Translate VI)');
        } else {
          setHub3Status('Disabled');
        }
      }

      // Web Audio API 16kHz PCM extractor + input level meter
      const audioCtx = new AudioContext({ sampleRate: 16000 });
      audioCtxRef.current = audioCtx;
      const source = audioCtx.createMediaStreamSource(stream);
      const processor = audioCtx.createScriptProcessor(2048, 1, 1);

      processor.onaudioprocess = (e) => {
        const floatSamples = e.inputBuffer.getChannelData(0);
        let sumSq = 0;
        const pcm16 = new Int16Array(floatSamples.length);
        for (let i = 0; i < floatSamples.length; i++) {
          const s = Math.max(-1, Math.min(1, floatSamples[i]));
          sumSq += s * s;
          pcm16[i] = s < 0 ? s * 0x8000 : s * 0x7fff;
        }
        const rms = Math.sqrt(sumSq / floatSamples.length);
        setMicInputLevel(Math.min(100, Math.round(rms * 320)));

        if (transcribeHubRef.current || translateHubRef.current) {
          const bytes = new Uint8Array(pcm16.buffer);
          let binary = '';
          for (let i = 0; i < bytes.byteLength; i++) {
            binary += String.fromCharCode(bytes[i]);
          }
          const base64Pcm16k = btoa(binary);
          const chunkPayload = {
            sessionId: flashcardSessionId,
            speaker: 'user',
            turnId,
            base64Pcm16k,
          };

          transcribeHubRef.current
            ?.invoke('StreamAudioForTranscription', chunkPayload)
            .catch(() => {});
          if (enableLiveTranslateHub3) {
            translateHubRef.current
              ?.invoke('StreamAudioForTranslation', chunkPayload)
              .catch(() => {});
          }
        }
      };

      source.connect(processor);
      processor.connect(audioCtx.destination);

      startTimeRef.current = Date.now();
      setRecordingDurationMs(0);
      timerIntervalRef.current = window.setInterval(() => {
        setRecordingDurationMs(Date.now() - startTimeRef.current);
      }, 100);

      setIsRecordingAudio(true);
    } catch (err: any) {
      setStatusBanner({
        type: 'error',
        text: `Không thể truy cập Microphone: ${err?.message || 'Vui lòng cấp quyền Microphone.'}`,
      });
    }
  };

  const stopUnifiedRecording = async () => {
    if (timerIntervalRef.current) {
      clearInterval(timerIntervalRef.current);
      timerIntervalRef.current = null;
    }

    if (mediaRecorderRef.current && mediaRecorderRef.current.state !== 'inactive') {
      mediaRecorderRef.current.stop();
    }

    mediaStreamRef.current?.getTracks().forEach((t) => t.stop());
    mediaStreamRef.current = null;

    if (audioCtxRef.current) {
      await audioCtxRef.current.close().catch(() => {});
      audioCtxRef.current = null;
    }

    if (transcribeHubRef.current) {
      await transcribeHubRef.current.stop().catch(() => {});
      transcribeHubRef.current = null;
      setHub2Status('Idle');
    }

    if (translateHubRef.current) {
      await translateHubRef.current.stop().catch(() => {});
      translateHubRef.current = null;
      setHub3Status(enableLiveTranslateHub3 ? 'Idle' : 'Disabled');
    }

    setMicInputLevel(0);
    setIsRecordingAudio(false);
  };

  // Submit recorded audio to selected Pronunciation Assessment AI Model (Section 10)
  const handleSubmitPronunciationAssessment = async () => {
    if (!activeCard) return;
    if (!apiKey.trim()) {
      setStatusBanner({
        type: 'error',
        text: 'Vui lòng nhập Google AI Studio API Key ở thanh bên trái để chấm điểm phát âm.',
      });
      onFocusApiKey();
      return;
    }
    if (!recordedBase64) {
      setStatusBanner({
        type: 'error',
        text: 'Vui lòng thu âm giọng đọc của bạn trước khi bấm Chấm điểm.',
      });
      return;
    }

    const selectedEx =
      activeCard.examples.find((e) => e.id === selectedExampleId) || activeCard.examples[0];
    const expectedText =
      practiceTargetType === 'Sentence' && selectedEx ? selectedEx.sentenceEn : activeCard.word;

    setIsScoringAudio(true);
    setStatusBanner({
      type: 'info',
      text: `Đang chấm điểm phát âm (${practiceTargetType}: "${expectedText}") bằng model ${assessmentModel} theo rubric ENGLISH_PRONUNCIATION_V1...`,
    });

    try {
      const res = await fetch(`${BACKEND_BASE_URL}/api/pronunciation/assessments`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          apiKey,
          vocabularyId: activeCard.vocabularyId,
          senseId: activeCard.senseId,
          targetType: practiceTargetType,
          targetId: practiceTargetType === 'Sentence' ? selectedEx?.id : null,
          referenceText: expectedText,
          accent: practiceAccent,
          assessmentModelCode: assessmentModel,
          base64Audio: recordedBase64,
          contentType: recordedMimeType,
          durationMilliseconds: Math.max(500, recordingDurationMs),
        }),
      });

      const data = await res.json();
      if (!res.ok) {
        throw new Error(data?.error || 'Không thể chấm điểm phát âm.');
      }

      const scored: PronunciationAssessmentResultDto = data;
      setAssessmentResult(scored);
      setAssessmentHistory((prev) => [scored, ...prev.slice(0, 9)]);

      if (scored.overallScore != null) {
        setPlan((prev) => {
          if (!prev) return prev;
          return {
            ...prev,
            flashcards: prev.flashcards.map((c) =>
              c.senseId === activeCard.senseId
                ? { ...c, latestPronunciationScore: scored.overallScore }
                : c
            ),
          };
        });
      }

      setStatusBanner({
        type: 'success',
        text: scored.isAssessable
          ? `Hoàn tất chấm phát âm! Điểm tổng (Weighted Overall): ${scored.overallScore}/100.`
          : 'Không nhận diện được giọng nói rõ ràng (NotAssessable). Hãy thử thu âm lại.',
      });
    } catch (err: any) {
      setStatusBanner({
        type: 'error',
        text: err?.message || 'Lỗi khi gửi yêu cầu chấm điểm phát âm.',
      });
    } finally {
      setIsScoringAudio(false);
    }
  };

  const currentSentenceObj =
    activeCard?.examples?.find((e) => e.id === selectedExampleId) || activeCard?.examples?.[0];

  return (
    <div className="space-y-5">
      {/* =====================================================================
          1. DAILY VOCABULARY SUMMARY HEADER & DUAL AI MODEL SELECTORS
      ====================================================================== */}
      <section
        className="p-5 bg-[var(--bg-surface)] border border-[var(--border-strong)] space-y-4"
        style={{ borderRadius: 'var(--radius-lg)', boxShadow: 'var(--shadow-md)' }}
      >
        <div className="flex flex-wrap items-center justify-between gap-4 pb-3.5 border-b border-[var(--border-subtle)]">
          <div className="flex items-center gap-3">
            <img
              src="/assets/icons/navigation/nav-today.svg"
              alt="Daily Vocabulary"
              className="w-10 h-10 shrink-0"
            />
            <div>
              <div className="flex flex-wrap items-center gap-2">
                <h1
                  className="text-lg md:text-xl font-bold text-[var(--text-primary)]"
                  style={{ fontFamily: 'var(--font-family-display)' }}
                >
                  Daily Vocabulary Flashcards &amp; AI Pronunciation
                </h1>
                <span
                  className="px-2.5 py-0.5 text-[11px] font-bold bg-[var(--bg-subtle)] text-[var(--learner-mint-text)] border border-[var(--learner-mint-border)]"
                  style={{ borderRadius: 'var(--radius-full)' }}
                >
                  Ngày học: {plan?.learningDate || new Date().toISOString().slice(0, 10)} ({plan?.timeZoneId || 'Asia/Ho_Chi_Minh'})
                </span>
              </div>
              <p className="text-xs text-[var(--text-secondary)]">
                Học từ vựng đa nghĩa (Sense-based Flashcards) · Lặp lại ngắt quãng SM-2 · Chấm phát âm theo Rubric (Accuracy 40% · Clarity 30% · Fluency 30%)
              </p>
            </div>
          </div>

          {/* Daily Stats Pills */}
          <div className="flex flex-wrap items-center gap-2">
            <div
              className="px-3 py-1.5 text-xs bg-[var(--color-primary-50)] border border-[var(--learner-mint-border)]"
              style={{ borderRadius: 'var(--radius-xs)' }}
            >
              Tiến độ hôm nay:{' '}
              <strong className="text-[var(--learner-mint-text)]">
                {plan?.completedCount ?? 0}/{plan?.totalAssignedCount ?? flashcards.length}
              </strong>
            </div>
            <div
              className="px-3 py-1.5 text-xs bg-[var(--bg-app)] border border-[var(--border-strong)]"
              style={{ borderRadius: 'var(--radius-xs)' }}
            >
              Từ mới: <strong>{plan?.newCount ?? 0}</strong> (Mục tiêu: {dailyTarget})
            </div>
            <div
              className="px-3 py-1.5 text-xs bg-[var(--bg-app)] border border-[var(--border-strong)]"
              style={{ borderRadius: 'var(--radius-xs)' }}
            >
              Cần ôn tập: <strong>{plan?.pendingReviewCount ?? 0}</strong> · Tồn đọng:{' '}
              <strong>{plan?.carryOverCount ?? 0}</strong>
            </div>
          </div>
        </div>

        {/* Two Independent AI Model Selectors (Section 5.3) + AI Generation Controls */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-3 items-end">
          {/* Selector 1: Vocabulary Generation Model */}
          <div className="lg:col-span-4">
            <label className="block text-[11px] font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-1">
              1. Model Tạo Từ Vựng AI (Gemini &amp; Gemma 4)
            </label>
            <select
              value={generationModel}
              onChange={(e) => setGenerationModel(e.target.value)}
              className="w-full px-3 py-2 text-xs font-semibold bg-[var(--bg-app)] border border-[var(--border-strong)] focus:outline-none focus:ring-2 focus:ring-[#26443c]"
              style={{ borderRadius: 'var(--radius-sm)' }}
            >
              {models
                .filter((m) => m.supportsTextGeneration && !m.supportsLiveAudio)
                .map((m) => (
                  <option key={m.modelCode} value={m.modelCode} disabled={!m.isAvailable || !m.isEnabled}>
                    {m.displayName} ({m.modelCode}){m.isDeprecated ? ' [Deprecated → 3.8]' : ''}
                  </option>
                ))}
            </select>
          </div>

          {/* Selector 2: Pronunciation Assessment Model (Gemma 4 disabled per Section 5.2!) */}
          <div className="lg:col-span-4">
            <label className="block text-[11px] font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-1">
              2. Model Chấm Phát Âm Audio (Chỉ nhận Model có Audio Input)
            </label>
            <select
              value={assessmentModel}
              onChange={(e) => setAssessmentModel(e.target.value)}
              className="w-full px-3 py-2 text-xs font-semibold bg-[var(--bg-app)] border border-[var(--border-strong)] focus:outline-none focus:ring-2 focus:ring-[#26443c]"
              style={{ borderRadius: 'var(--radius-sm)' }}
            >
              {models
                .filter((m) => !m.supportsLiveAudio)
                .map((m) => {
                  const audioSupported = m.supportsAudioInput && m.supportsPronunciationAssessment;
                  return (
                    <option
                      key={m.modelCode}
                      value={m.modelCode}
                      disabled={!audioSupported || !m.isAvailable || !m.isEnabled}
                    >
                      {m.displayName} ({m.modelCode})
                      {!audioSupported ? ' — [Không hỗ trợ Audio Input]' : ''}
                      {m.isDeprecated ? ' — [Deprecated → 3.8]' : ''}
                    </option>
                  );
                })}
            </select>
          </div>

          {/* Topic + Generate AI Vocabulary Action */}
          <div className="lg:col-span-4 flex gap-2">
            <div className="flex-1">
              <label className="block text-[11px] font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-1">
                Chủ đề tạo thêm từ ({cefrLevel})
              </label>
              <input
                type="text"
                value={genTopic}
                onChange={(e) => setGenTopic(e.target.value)}
                placeholder="VD: System Design, Travel, IELTS..."
                className="w-full px-3 py-2 text-xs bg-[var(--bg-app)] border border-[var(--border-strong)] focus:outline-none"
                style={{ borderRadius: 'var(--radius-sm)' }}
              />
            </div>
            <div>
              <label className="block text-[11px] font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-1">
                Số từ
              </label>
              <select
                value={genCount}
                onChange={(e) => setGenCount(Number(e.target.value))}
                className="px-2 py-2 text-xs font-bold bg-[var(--bg-app)] border border-[var(--border-strong)]"
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <option value={3}>+3</option>
                <option value={5}>+5</option>
                <option value={10}>+10</option>
              </select>
            </div>
            <button
              type="button"
              onClick={handleGenerateAiVocabulary}
              disabled={isGeneratingVocab}
              className="px-3.5 py-2 text-xs font-bold text-[var(--text-on-primary)] bg-[var(--learner-primary-action)] hover:bg-[var(--learner-primary-action-hover)] disabled:opacity-50 cursor-pointer shrink-0"
              style={{ borderRadius: 'var(--radius-sm)', boxShadow: 'var(--shadow-sm)' }}
            >
              {isGeneratingVocab ? 'Đang tạo...' : '+ Tạo bằng AI'}
            </button>
          </div>
        </div>

        {statusBanner && (
          <div
            className={`px-3.5 py-2 text-xs font-medium border flex items-center justify-between ${
              statusBanner.type === 'error'
                ? 'bg-red-50 border-red-200 text-red-800'
                : statusBanner.type === 'success'
                ? 'bg-[var(--bg-subtle)] border-[var(--learner-mint-border)] text-[var(--learner-mint-text)]'
                : 'bg-amber-50 border-amber-200 text-amber-900'
            }`}
            style={{ borderRadius: 'var(--radius-xs)' }}
          >
            <span>{statusBanner.text}</span>
            <button
              type="button"
              onClick={() => setStatusBanner(null)}
              className="text-[11px] font-bold underline ml-3 cursor-pointer"
            >
              Đóng
            </button>
          </div>
        )}
      </section>

      {/* =====================================================================
          2. MAIN FLASHCARD & PRONUNCIATION PRACTICE GRID
      ====================================================================== */}
      {isLoadingPlan ? (
        <div
          className="p-12 bg-[var(--bg-surface)] border border-[var(--border-strong)] text-center text-sm text-[var(--text-secondary)]"
          style={{ borderRadius: 'var(--radius-lg)' }}
        >
          Đang tải kế hoạch từ vựng hôm nay...
        </div>
      ) : !activeCard ? (
        <div
          className="p-12 bg-[var(--bg-surface)] border border-[var(--border-strong)] text-center space-y-3"
          style={{ borderRadius: 'var(--radius-lg)' }}
        >
          <p className="text-base font-bold text-[var(--text-primary)]">
            Chưa có thẻ từ vựng nào trong kế hoạch hôm nay
          </p>
          <button
            type="button"
            onClick={() => loadModelsAndTodayPlan()}
            className="px-4 py-2 text-xs font-bold bg-[var(--learner-primary-action)] text-[var(--text-on-primary)] cursor-pointer"
            style={{ borderRadius: 'var(--radius-sm)' }}
          >
            Khởi tạo danh sách từ vựng mẫu
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          {/* =================================================================
              LEFT 7 COLS: INTERACTIVE FLASHCARD (FRONT / BACK) + SM-2 CONTROLS
          ================================================================== */}
          <div className="lg:col-span-7 flex flex-col gap-4">
            {/* Card Selector Strip */}
            <div className="flex items-center gap-1.5 overflow-x-auto pb-1">
              {flashcards.map((fc, idx) => {
                const isCurrent = idx === currentIndex;
                const isDone = fc.itemStatus === 'Completed';
                return (
                  <button
                    key={fc.senseId}
                    type="button"
                    onClick={() => setCurrentIndex(idx)}
                    className={`px-3 py-1.5 text-xs font-bold border shrink-0 transition-all cursor-pointer flex items-center gap-1.5 ${
                      isCurrent
                        ? 'bg-[var(--learner-primary-action)] text-[var(--text-on-primary)] border-[var(--learner-primary-action)]'
                        : isDone
                        ? 'bg-[var(--bg-subtle)] text-[var(--learner-mint-text)] border-[var(--learner-mint-border)]'
                        : 'bg-[var(--bg-surface)] text-[var(--text-primary)] border-[var(--border-strong)]'
                    }`}
                    style={{ borderRadius: 'var(--radius-full)' }}
                  >
                    <span>
                      {idx + 1}. {fc.word}
                    </span>
                    <span className="text-[10px] opacity-80">({fc.partOfSpeech})</span>
                    {isDone && <span>✓</span>}
                  </button>
                );
              })}
            </div>

            {/* Interactive Flashcard Surface */}
            <div
              className="p-6 md:p-7 bg-[var(--bg-surface)] border border-[var(--border-strong)] flex flex-col justify-between min-h-[470px]"
              style={{ borderRadius: 'var(--radius-xl)', boxShadow: 'var(--shadow-lg)' }}
            >
              {/* Card Top Meta Row */}
              <div className="flex flex-wrap items-center justify-between gap-2 pb-4 border-b border-[var(--border-subtle)]">
                <div className="flex flex-wrap items-center gap-2">
                  <span
                    className="px-2.5 py-0.5 text-xs font-bold bg-[var(--learner-primary-action)] text-[var(--text-on-primary)]"
                    style={{ borderRadius: 'var(--radius-full)' }}
                  >
                    CEFR {activeCard.cefrLevel}
                  </span>
                  <span
                    className="px-2.5 py-0.5 text-xs font-semibold bg-[var(--bg-subtle)] text-[var(--learner-mint-text)] border border-[var(--learner-mint-border)]"
                    style={{ borderRadius: 'var(--radius-full)' }}
                  >
                    {activeCard.partOfSpeech}
                  </span>
                  <span
                    className="px-2.5 py-0.5 text-[11px] font-semibold bg-[var(--bg-app)] text-[var(--text-secondary)] border border-[var(--border-strong)]"
                    style={{ borderRadius: 'var(--radius-full)' }}
                  >
                    Loại thẻ: {activeCard.itemType} · Trạng thái: {activeCard.learningStatus}
                  </span>
                </div>

                <div className="flex items-center gap-2 text-xs font-bold">
                  <span className="text-[var(--learner-mint-text)]">
                    Mastery: {activeCard.masteryLevel}%
                  </span>
                  {activeCard.latestPronunciationScore != null && (
                    <span
                      className="px-2.5 py-0.5 bg-emerald-100 text-emerald-900 border border-emerald-300"
                      style={{ borderRadius: 'var(--radius-full)' }}
                    >
                      Điểm phát âm: {activeCard.latestPronunciationScore}/100
                    </span>
                  )}
                </div>
              </div>

              {/* CARD FRONT OR BACK CONTENT */}
              {!isFlipped ? (
                /* FRONT SIDE (Section 19.2) */
                <div className="my-auto py-8 text-center space-y-6">
                  <div>
                    <div className="text-xs font-semibold uppercase tracking-widest text-[var(--text-muted)] mb-2">
                      Mặt trước (Front Side) · Thẻ {currentIndex + 1}/{flashcards.length}
                    </div>
                    <h2
                      className="text-4xl md:text-5xl font-extrabold text-[var(--text-primary)] tracking-tight"
                      style={{ fontFamily: 'var(--font-family-display)' }}
                    >
                      {activeCard.word}
                    </h2>
                    <p className="mt-2 text-sm font-medium text-[var(--text-secondary)]">
                      ({activeCard.partOfSpeech}) — Nhấn nghe phát âm mẫu UK / US hoặc lật thẻ để xem chi tiết
                    </p>
                  </div>

                  {/* UK / US IPA & Audio Playback Buttons */}
                  <div className="flex flex-wrap items-center justify-center gap-3">
                    {activeCard.pronunciations.length > 0 ? (
                      activeCard.pronunciations.map((p) => (
                        <button
                          key={`${p.accent}-${p.ipa}`}
                          type="button"
                          onClick={() => playReferenceAudio(activeCard.word, p.accent as 'UK' | 'US', p.audioUrl)}
                          className="px-4 py-2.5 text-xs font-bold bg-[var(--color-primary-50)] hover:bg-[var(--bg-subtle)] text-[var(--learner-mint-text)] border border-[var(--learner-mint-border)] flex items-center gap-2 cursor-pointer transition-all"
                          style={{ borderRadius: 'var(--radius-md)', boxShadow: 'var(--shadow-sm)' }}
                        >
                          <span>🔊 {p.accent}</span>
                          <span className="font-mono text-sm">{p.ipa}</span>
                          {p.stressPattern && (
                            <span className="text-[10px] text-[var(--text-secondary)]">
                              [{p.stressPattern}]
                            </span>
                          )}
                        </button>
                      ))
                    ) : (
                      <>
                        <button
                          type="button"
                          onClick={() => playReferenceAudio(activeCard.word, 'UK')}
                          className="px-4 py-2 text-xs font-bold bg-[var(--color-primary-50)] border border-[var(--learner-mint-border)] cursor-pointer"
                          style={{ borderRadius: 'var(--radius-sm)' }}
                        >
                          🔊 Nghe chuẩn UK
                        </button>
                        <button
                          type="button"
                          onClick={() => playReferenceAudio(activeCard.word, 'US')}
                          className="px-4 py-2 text-xs font-bold bg-[var(--color-primary-50)] border border-[var(--learner-mint-border)] cursor-pointer"
                          style={{ borderRadius: 'var(--radius-sm)' }}
                        >
                          🔊 Nghe chuẩn US
                        </button>
                      </>
                    )}
                  </div>

                  <div className="pt-2">
                    <button
                      type="button"
                      onClick={() => setIsFlipped(true)}
                      className="px-6 py-3 text-xs font-bold uppercase tracking-wider bg-[var(--learner-primary-action)] hover:bg-[var(--learner-primary-action-hover)] text-[var(--text-on-primary)] cursor-pointer"
                      style={{ borderRadius: 'var(--radius-full)', boxShadow: 'var(--shadow-md)' }}
                    >
                      ↻ Lật thẻ xem Nghĩa tiếng Việt, Collocations &amp; Câu ví dụ
                    </button>
                  </div>
                </div>
              ) : (
                /* BACK SIDE (Section 19.2: Full Rich Sense Data) */
                <div className="py-4 space-y-4 overflow-y-auto max-h-[420px] pr-1">
                  <div className="p-3.5 bg-[var(--bg-subtle)] border border-[var(--learner-mint-border)] space-y-1" style={{ borderRadius: 'var(--radius-md)' }}>
                    <div className="flex items-center justify-between">
                      <span className="text-lg font-extrabold text-[var(--learner-mint-text)]">
                        {activeCard.word} ({activeCard.partOfSpeech}) — {activeCard.meaningVi}
                      </span>
                      <button
                        type="button"
                        onClick={() => playReferenceAudio(activeCard.word, practiceAccent)}
                        className="px-2.5 py-1 text-xs font-bold bg-white border border-[var(--learner-mint-border)] cursor-pointer"
                        style={{ borderRadius: 'var(--radius-xs)' }}
                      >
                        🔊 Phát âm ({practiceAccent})
                      </button>
                    </div>
                    <p className="text-xs font-medium text-[var(--text-primary)]">
                      <strong>Definition (EN):</strong> {activeCard.definitionEn}
                    </p>
                    {activeCard.descriptionEn && (
                      <p className="text-[11px] text-[var(--text-secondary)]">
                        <strong>Ngữ cảnh sử dụng:</strong> {activeCard.descriptionEn}
                      </p>
                    )}
                  </div>

                  {/* Synonyms, Antonyms & Word Family */}
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-2.5 text-xs">
                    <div className="p-2.5 bg-[var(--bg-app)] border border-[var(--border-strong)]" style={{ borderRadius: 'var(--radius-sm)' }}>
                      <div className="font-bold text-[11px] uppercase text-[var(--text-secondary)] mb-1">
                        Từ đồng nghĩa (Synonyms)
                      </div>
                      <div className="flex flex-wrap gap-1">
                        {activeCard.synonyms.length > 0 ? (
                          activeCard.synonyms.map((s) => (
                            <span key={s} className="px-2 py-0.5 bg-[var(--color-primary-50)] border border-[var(--learner-mint-border)] font-semibold rounded-full">
                              {s}
                            </span>
                          ))
                        ) : (
                          <span className="text-[var(--text-muted)]">—</span>
                        )}
                      </div>
                    </div>

                    <div className="p-2.5 bg-[var(--bg-app)] border border-[var(--border-strong)]" style={{ borderRadius: 'var(--radius-sm)' }}>
                      <div className="font-bold text-[11px] uppercase text-[var(--text-secondary)] mb-1">
                        Từ trái nghĩa (Antonyms)
                      </div>
                      <div className="flex flex-wrap gap-1">
                        {activeCard.antonyms.length > 0 ? (
                          activeCard.antonyms.map((a) => (
                            <span key={a} className="px-2 py-0.5 bg-red-50 text-red-800 border border-red-200 font-semibold rounded-full">
                              {a}
                            </span>
                          ))
                        ) : (
                          <span className="text-[var(--text-muted)]">—</span>
                        )}
                      </div>
                    </div>

                    <div className="p-2.5 bg-[var(--bg-app)] border border-[var(--border-strong)]" style={{ borderRadius: 'var(--radius-sm)' }}>
                      <div className="font-bold text-[11px] uppercase text-[var(--text-secondary)] mb-1">
                        Họ từ (Word Family)
                      </div>
                      <div className="flex flex-wrap gap-1">
                        {activeCard.wordFamilies.length > 0 ? (
                          activeCard.wordFamilies.map((wf) => (
                            <span key={wf.id} className="px-2 py-0.5 bg-white border border-[var(--border-strong)] font-medium rounded-full">
                              <strong>{wf.relatedWord}</strong> ({wf.partOfSpeech})
                            </span>
                          ))
                        ) : (
                          <span className="text-[var(--text-muted)]">—</span>
                        )}
                      </div>
                    </div>
                  </div>

                  {/* Collocations & Grammar Patterns */}
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-2.5 text-xs">
                    <div className="p-3 bg-[var(--bg-app)] border border-[var(--border-strong)] space-y-1" style={{ borderRadius: 'var(--radius-sm)' }}>
                      <div className="font-bold text-[11px] uppercase text-[var(--text-secondary)]">
                        Cụm từ cố định (Collocations)
                      </div>
                      {activeCard.collocations.map((c) => (
                        <div key={c.id} className="flex items-center justify-between gap-2 py-0.5 border-b border-[var(--border-subtle)] last:border-0">
                          <span className="font-bold text-[var(--text-primary)]">{c.phraseEn}</span>
                          <span className="text-[var(--text-secondary)] italic">{c.meaningVi}</span>
                        </div>
                      ))}
                    </div>

                    <div className="p-3 bg-[var(--bg-app)] border border-[var(--border-strong)] space-y-1" style={{ borderRadius: 'var(--radius-sm)' }}>
                      <div className="font-bold text-[11px] uppercase text-[var(--text-secondary)]">
                        Cấu trúc ngữ pháp (Grammar Patterns)
                      </div>
                      {activeCard.grammarPatterns.map((g) => (
                        <div key={g.id} className="py-0.5 border-b border-[var(--border-subtle)] last:border-0">
                          <div className="font-bold text-[var(--learner-mint-text)]">{g.pattern}</div>
                          {g.explanation && <div className="text-[11px] text-[var(--text-secondary)]">{g.explanation}</div>}
                          {g.example && <div className="text-[11px] italic">VD: "{g.example}"</div>}
                        </div>
                      ))}
                    </div>
                  </div>

                  {/* Example Sentences with Sentence Pronunciation Practice button */}
                  <div className="p-3 bg-[var(--bg-app)] border border-[var(--border-strong)] space-y-2" style={{ borderRadius: 'var(--radius-sm)' }}>
                    <div className="font-bold text-[11px] uppercase text-[var(--text-secondary)]">
                      Câu ví dụ thực tế (Bấm "Luyện câu này" để chấm phát âm cả câu)
                    </div>
                    {activeCard.examples.map((ex) => (
                      <div
                        key={ex.id}
                        className="p-2.5 bg-white border border-[var(--border-subtle)] flex flex-col sm:flex-row sm:items-center justify-between gap-2"
                        style={{ borderRadius: 'var(--radius-xs)' }}
                      >
                        <div className="space-y-0.5">
                          <div className="text-xs font-bold text-[var(--text-primary)]">{ex.sentenceEn}</div>
                          {ex.sentenceVi && (
                            <div className="text-xs italic text-[var(--text-secondary)]">{ex.sentenceVi}</div>
                          )}
                        </div>
                        <div className="flex items-center gap-1.5 shrink-0">
                          <button
                            type="button"
                            onClick={() => playReferenceAudio(ex.sentenceEn, practiceAccent, ex.audioUrl)}
                            className="px-2.5 py-1 text-[11px] font-semibold bg-[var(--color-primary-50)] border border-[var(--learner-mint-border)] cursor-pointer"
                            style={{ borderRadius: 'var(--radius-xs)' }}
                          >
                            🔊 Nghe câu
                          </button>
                          <button
                            type="button"
                            onClick={() => {
                              setPracticeTargetType('Sentence');
                              setSelectedExampleId(ex.id);
                            }}
                            className={`px-2.5 py-1 text-[11px] font-bold border cursor-pointer ${
                              practiceTargetType === 'Sentence' && selectedExampleId === ex.id
                                ? 'bg-[var(--learner-primary-action)] text-[var(--text-on-primary)]'
                                : 'bg-[var(--bg-subtle)] text-[var(--learner-mint-text)] border-[var(--learner-mint-border)]'
                            }`}
                            style={{ borderRadius: 'var(--radius-xs)' }}
                          >
                            🎙️ Luyện câu này
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Bottom Flip & SM-2 Spaced Repetition Action Bar (Section 19.5) */}
              <div className="pt-4 border-t border-[var(--border-subtle)] space-y-3">
                <div className="flex items-center justify-between gap-2">
                  <button
                    type="button"
                    disabled={currentIndex === 0}
                    onClick={() => setCurrentIndex((i) => Math.max(0, i - 1))}
                    className="px-3 py-1.5 text-xs font-bold border border-[var(--border-strong)] bg-[var(--bg-app)] disabled:opacity-40 cursor-pointer"
                    style={{ borderRadius: 'var(--radius-xs)' }}
                  >
                    ← Thẻ trước
                  </button>

                  <button
                    type="button"
                    onClick={() => setIsFlipped((f) => !f)}
                    className="px-4 py-1.5 text-xs font-bold border border-[var(--learner-mint-border)] bg-[var(--bg-subtle)] text-[var(--learner-mint-text)] cursor-pointer"
                    style={{ borderRadius: 'var(--radius-xs)' }}
                  >
                    {isFlipped ? '↺ Quay lại mặt trước' : '↻ Lật mặt sau xem chi tiết'}
                  </button>

                  <button
                    type="button"
                    disabled={currentIndex >= flashcards.length - 1}
                    onClick={() => setCurrentIndex((i) => Math.min(flashcards.length - 1, i + 1))}
                    className="px-3 py-1.5 text-xs font-bold border border-[var(--border-strong)] bg-[var(--bg-app)] disabled:opacity-40 cursor-pointer"
                    style={{ borderRadius: 'var(--radius-xs)' }}
                  >
                    Thẻ tiếp theo →
                  </button>
                </div>

                {/* Spaced Repetition SM-2 Buttons */}
                <div>
                  <div className="text-[11px] font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-1.5">
                    Đánh giá mức độ ghi nhớ (Spaced Repetition SM-2):
                  </div>
                  <div className="grid grid-cols-4 gap-2">
                    {[
                      { rating: 'Again', label: 'Again · Học lại', sub: '+4 giờ', cls: 'bg-red-50 hover:bg-red-100 text-red-900 border-red-300' },
                      { rating: 'Hard', label: 'Hard · Khó', sub: '+1 ngày', cls: 'bg-amber-50 hover:bg-amber-100 text-amber-900 border-amber-300' },
                      { rating: 'Good', label: 'Good · Nhớ tốt', sub: '+3 ngày', cls: 'bg-[var(--bg-subtle)] hover:bg-[var(--learner-mint-surface-hover)] text-[var(--learner-mint-text)] border-[var(--learner-mint-border)]' },
                      { rating: 'Easy', label: 'Easy · Rất dễ', sub: '+5 ngày', cls: 'bg-emerald-600 hover:bg-emerald-700 text-white border-emerald-700' },
                    ].map((btn) => (
                      <button
                        key={btn.rating}
                        type="button"
                        disabled={isSubmittingReview}
                        onClick={() => handleSubmitReviewRating(btn.rating as 'Again' | 'Hard' | 'Good' | 'Easy')}
                        className={`p-2 text-center border transition-all cursor-pointer ${btn.cls}`}
                        style={{ borderRadius: 'var(--radius-sm)' }}
                      >
                        <div className="text-xs font-extrabold">{btn.label}</div>
                        <div className="text-[10px] opacity-80">{btn.sub}</div>
                      </button>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* =================================================================
              RIGHT 5 COLS: PRONUNCIATION PRACTICE, LIVE HUB 2/3 & SCORING RESULT
          ================================================================== */}
          <div className="lg:col-span-5 flex flex-col gap-4">
            <div
              className="p-5 bg-[var(--bg-surface)] border border-[var(--border-strong)] space-y-4"
              style={{ borderRadius: 'var(--radius-lg)', boxShadow: 'var(--shadow-md)' }}
            >
              <div className="pb-3 border-b border-[var(--border-subtle)] flex items-center justify-between">
                <div>
                  <h3 className="text-base font-bold text-[var(--text-primary)]">
                    🎙️ Luyện Phát Âm &amp; Chấm Điểm AI
                  </h3>
                  <p className="text-xs text-[var(--text-secondary)]">
                    Rubric: Accuracy 40% · Clarity 30% · Fluency 30%
                  </p>
                </div>

                {/* Accent Selector US / UK */}
                <div className="flex items-center gap-1">
                  {(['US', 'UK'] as const).map((acc) => (
                    <button
                      key={acc}
                      type="button"
                      onClick={() => setPracticeAccent(acc)}
                      className={`px-2.5 py-1 text-xs font-bold border cursor-pointer ${
                        practiceAccent === acc
                          ? 'bg-[var(--learner-primary-action)] text-[var(--text-on-primary)]'
                          : 'bg-[var(--bg-app)] text-[var(--text-secondary)] border-[var(--border-strong)]'
                      }`}
                      style={{ borderRadius: 'var(--radius-xs)' }}
                    >
                      {acc}
                    </button>
                  ))}
                </div>
              </div>

              {/* Target Type Switcher: Word vs. Example Sentence */}
              <div className="space-y-2">
                <div className="grid grid-cols-2 gap-2">
                  <button
                    type="button"
                    onClick={() => setPracticeTargetType('Word')}
                    className={`py-2 px-3 text-xs font-bold border cursor-pointer ${
                      practiceTargetType === 'Word'
                        ? 'bg-[var(--bg-subtle)] border-[var(--learner-primary-action)] text-[var(--learner-mint-text)]'
                        : 'bg-[var(--bg-app)] border-[var(--border-strong)] text-[var(--text-secondary)]'
                    }`}
                    style={{ borderRadius: 'var(--radius-sm)' }}
                  >
                    🔤 Luyện Từ đơn (Word)
                  </button>
                  <button
                    type="button"
                    onClick={() => setPracticeTargetType('Sentence')}
                    className={`py-2 px-3 text-xs font-bold border cursor-pointer ${
                      practiceTargetType === 'Sentence'
                        ? 'bg-[var(--bg-subtle)] border-[var(--learner-primary-action)] text-[var(--learner-mint-text)]'
                        : 'bg-[var(--bg-app)] border-[var(--border-strong)] text-[var(--text-secondary)]'
                    }`}
                    style={{ borderRadius: 'var(--radius-sm)' }}
                  >
                    📝 Luyện Câu ví dụ (Sentence)
                  </button>
                </div>

                {/* Target Reference Box */}
                <div
                  className="p-3 bg-[var(--color-primary-50)] border border-[var(--learner-mint-border)] space-y-1.5"
                  style={{ borderRadius: 'var(--radius-sm)' }}
                >
                  <div className="flex items-center justify-between">
                    <span className="text-[11px] font-bold uppercase text-[var(--learner-mint-text)]">
                      Mẫu cần đọc ({practiceTargetType} · {practiceAccent}):
                    </span>
                    <button
                      type="button"
                      onClick={() =>
                        playReferenceAudio(
                          practiceTargetType === 'Sentence' && currentSentenceObj
                            ? currentSentenceObj.sentenceEn
                            : activeCard.word,
                          practiceAccent
                        )
                      }
                      className="text-xs font-bold text-[var(--learner-mint-text)] underline cursor-pointer"
                    >
                      🔊 Nghe mẫu
                    </button>
                  </div>
                  <div className="text-sm font-extrabold text-[var(--text-primary)]">
                    {practiceTargetType === 'Sentence' && currentSentenceObj
                      ? currentSentenceObj.sentenceEn
                      : activeCard.word}
                  </div>
                  {practiceTargetType === 'Sentence' && currentSentenceObj?.sentenceVi && (
                    <div className="text-xs italic text-[var(--text-secondary)]">
                      {currentSentenceObj.sentenceVi}
                    </div>
                  )}
                </div>
              </div>

              {/* Hub 2 (Live Speech) & Hub 3 (Vietnamese Live Translation) Toggles (Sections 12 & 13) */}
              <div className="p-3 bg-[var(--bg-app)] border border-[var(--border-strong)] space-y-2" style={{ borderRadius: 'var(--radius-sm)' }}>
                <div className="flex items-center justify-between text-xs">
                  <label className="flex items-center gap-2 font-semibold cursor-pointer">
                    <input
                      type="checkbox"
                      checked={enableLiveSpeechHub2}
                      onChange={(e) => setEnableLiveSpeechHub2(e.target.checked)}
                    />
                    <span>Bật Live Speech (Hub 2: gemini-3.5-transcribe-live)</span>
                  </label>
                  <span className="text-[10px] font-bold text-[var(--learner-mint-text)]">{hub2Status}</span>
                </div>

                <div className="flex items-center justify-between text-xs">
                  <label className="flex items-center gap-2 font-semibold cursor-pointer">
                    <input
                      type="checkbox"
                      checked={enableLiveTranslateHub3}
                      onChange={(e) => {
                        setEnableLiveTranslateHub3(e.target.checked);
                        setHub3Status(e.target.checked ? 'Ready' : 'Disabled');
                      }}
                    />
                    <span>Enable Vietnamese Live Translation (Hub 3: 3.5 Translate)</span>
                  </label>
                  <span className="text-[10px] font-bold text-[var(--text-secondary)]">{hub3Status}</span>
                </div>

                {(liveTranscriptLine1 || liveTranslationLine2 || isRecordingAudio) && (
                  <div className="pt-2 border-t border-[var(--border-subtle)] space-y-1">
                    <div className="text-xs font-bold text-[var(--text-primary)]">
                      Dòng 1 (Transcribe): {liveTranscriptLine1 || (isRecordingAudio ? 'Đang lắng nghe...' : '—')}
                    </div>
                    {enableLiveTranslateHub3 && (
                      <div className="text-xs italic text-[var(--text-secondary)]">
                        Dòng 2 (Dịch tiếng Việt): {liveTranslationLine2 || 'Đang dịch...'}
                      </div>
                    )}
                  </div>
                )}
              </div>

              {/* Recording Controls & Input Level Meter */}
              <div className="space-y-2.5">
                {isRecordingAudio && (
                  <div className="space-y-1">
                    <div className="flex justify-between text-xs font-bold text-red-700">
                      <span>● Đang thu âm... ({(recordingDurationMs / 1000).toFixed(1)}s)</span>
                      <span>Âm lượng Mic: {micInputLevel}%</span>
                    </div>
                    <div className="w-full h-2 bg-gray-200 rounded-full overflow-hidden">
                      <div
                        className="h-full bg-[#38a385] transition-all duration-75"
                        style={{ width: `${micInputLevel}%` }}
                      />
                    </div>
                  </div>
                )}

                <div className="flex flex-wrap gap-2">
                  {!isRecordingAudio ? (
                    <button
                      type="button"
                      onClick={startUnifiedRecording}
                      className="flex-1 py-2.5 px-4 text-xs font-bold text-white bg-[#38a385] hover:bg-[#2e886e] cursor-pointer"
                      style={{ borderRadius: 'var(--radius-sm)' }}
                    >
                      🎙️ {recordedBase64 ? 'Thu âm lại (Re-record)' : 'Bắt đầu thu âm'}
                    </button>
                  ) : (
                    <button
                      type="button"
                      onClick={stopUnifiedRecording}
                      className="flex-1 py-2.5 px-4 text-xs font-bold text-white bg-red-600 hover:bg-red-700 cursor-pointer"
                      style={{ borderRadius: 'var(--radius-sm)' }}
                    >
                      ⏹ Dừng thu âm ({(recordingDurationMs / 1000).toFixed(1)}s)
                    </button>
                  )}

                  {recordedBase64 && !isRecordingAudio && (
                    <button
                      type="button"
                      onClick={handleSubmitPronunciationAssessment}
                      disabled={isScoringAudio}
                      className="flex-1 py-2.5 px-4 text-xs font-bold text-[var(--text-on-primary)] bg-[var(--learner-primary-action)] hover:bg-[var(--learner-primary-action-hover)] disabled:opacity-50 cursor-pointer"
                      style={{ borderRadius: 'var(--radius-sm)', boxShadow: 'var(--shadow-sm)' }}
                    >
                      {isScoringAudio ? 'Đang chấm điểm...' : '✨ Gửi AI Chấm Phát Âm'}
                    </button>
                  )}
                </div>

                {recordedAudioUrl && !isRecordingAudio && (
                  <div className="pt-1">
                    <audio controls src={recordedAudioUrl} className="w-full h-9" />
                  </div>
                )}
              </div>

              {/* Pronunciation Assessment Result Component (Section 19.4) */}
              {assessmentResult && (
                <div
                  className="p-4 bg-[var(--bg-app)] border border-[var(--learner-mint-border)] space-y-3"
                  style={{ borderRadius: 'var(--radius-md)' }}
                >
                  <div className="flex items-center justify-between border-b border-[var(--border-subtle)] pb-2">
                    <div>
                      <div className="text-xs font-bold uppercase text-[var(--text-secondary)]">
                        Kết quả chấm phát âm ({assessmentResult.modelCode})
                      </div>
                      <div className="text-xs text-[var(--text-secondary)]">
                        Nhận diện: <strong>"{assessmentResult.recognizedText || '—'}"</strong>
                      </div>
                    </div>
                    <div className="text-right">
                      <div className="text-2xl font-extrabold text-[var(--learner-mint-text)]">
                        {assessmentResult.overallScore != null
                          ? `${assessmentResult.overallScore}/100`
                          : 'NotAssessable'}
                      </div>
                      <div className="text-[10px] text-[var(--text-muted)]">
                        Weighted Rubric v{assessmentResult.rubricVersion}
                      </div>
                    </div>
                  </div>

                  {/* Criterion Score Bars: Accuracy (40%), Clarity (30%), Fluency (30%) */}
                  <div className="space-y-2">
                    {assessmentResult.criteria.map((c) => (
                      <div key={c.code} className="space-y-1 text-xs">
                        <div className="flex justify-between font-bold">
                          <span>
                            {c.code} (Trọng số {c.weight}%)
                          </span>
                          <span>
                            {c.score}/100 → <strong>+{c.weightedScore}đ</strong>
                          </span>
                        </div>
                        <div className="w-full h-2 bg-gray-200 rounded-full overflow-hidden">
                          <div
                            className="h-full bg-[var(--learner-primary-action)]"
                            style={{ width: `${Math.min(100, Math.max(0, c.score))}%` }}
                          />
                        </div>
                        <div className="text-[11px] text-[var(--text-secondary)]">{c.feedback}</div>
                      </div>
                    ))}
                  </div>

                  {assessmentResult.strengths.length > 0 && (
                    <div className="text-xs">
                      <strong className="text-emerald-800">✓ Điểm mạnh:</strong>{' '}
                      {assessmentResult.strengths.join(' · ')}
                    </div>
                  )}

                  {assessmentResult.improvements.length > 0 && (
                    <div className="text-xs">
                      <strong className="text-amber-800">⚡ Cần cải thiện:</strong>{' '}
                      {assessmentResult.improvements.join(' · ')}
                    </div>
                  )}

                  {assessmentResult.suggestedPractice.length > 0 && (
                    <div className="text-xs">
                      <strong className="text-[var(--learner-mint-text)]">💡 Gợi ý luyện tập:</strong>{' '}
                      {assessmentResult.suggestedPractice.join(' · ')}
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
