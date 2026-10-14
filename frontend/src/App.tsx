import React, { useEffect, useRef, useState } from 'react';
import {
  CefrLevel,
  ConversationModeId,
  ConversationModeOption,
  TeacherVoiceName,
} from './types/live';
import { useThreeHubLiveSession } from './hooks/useThreeHubLiveSession';
import { DailyVocabularyWorkspace } from './components/DailyVocabularyWorkspace';

const CEFR_LEVELS: { level: CefrLevel; label: string; desc: string }[] = [
  { level: 'A1', label: 'A1 · Beginner', desc: 'Câu ngắn 5-8 từ, tốc độ chậm' },
  { level: 'A2', label: 'A2 · Elementary', desc: 'Giao tiếp hàng ngày cơ bản' },
  { level: 'B1', label: 'B1 · Intermediate', desc: 'Phản xạ tự nhiên, phrasal verbs' },
  { level: 'B2', label: 'B2 · Upper-Int', desc: 'Tranh luận mạch lạc, câu phức' },
  { level: 'C1', label: 'C1 · Advanced', desc: 'Tốc độ bản xứ, idioms nâng cao' },
  { level: 'C2', label: 'C2 · Mastery', desc: 'Sắc thái học thuật & chuyên sâu' },
];

const TEACHER_VOICES: { id: TeacherVoiceName; label: string; style: string }[] = [
  { id: 'Aoede', label: 'Aoede (Nữ)', style: 'Truyền cảm, ấm áp & rõ chữ' },
  { id: 'Puck', label: 'Puck (Nam)', style: 'Thân thiện, tự nhiên & năng động' },
  { id: 'Kore', label: 'Kore (Nữ)', style: 'Điềm tĩnh, chuẩn mực sư phạm' },
  { id: 'Charon', label: 'Charon (Nam)', style: 'Trầm ấm, phong thái học thuật' },
  { id: 'Fenrir', label: 'Fenrir (Nam)', style: 'Dứt khoát, rõ ràng, tốc độ tốt' },
  { id: 'Leda', label: 'Leda (Nữ)', style: 'Nhẹ nhàng, kiên nhẫn cho A1-B1' },
];

const SPEECH_SPEEDS: { value: number; label: string }[] = [
  { value: 0.75, label: '0.75x' },
  { value: 0.9, label: '0.9x' },
  { value: 1.0, label: '1.0x' },
  { value: 1.15, label: '1.15x' },
  { value: 1.25, label: '1.25x' },
];

export const CONVERSATION_MODES: ConversationModeOption[] = [
  // --- 12 INTERVIEW & COACHING MODES ---
  {
    id: 'practice_interview',
    group: 'interview',
    name: '1. Practice Mode',
    descVi: 'Phỏng vấn và sửa ngữ pháp, từ vựng sau mỗi câu trả lời.',
    suggestedTopics: ['Backend .NET Developer Interview', 'Fullstack React & .NET Interview', 'Self-Introduction & Strengths'],
  },
  {
    id: 'real_interview',
    group: 'interview',
    name: '2. Real Interview Mode',
    descVi: 'Phỏng vấn như thực tế, không ngắt lời, đánh giá cuối buổi.',
    suggestedTopics: ['Senior Software Engineer Mock Interview', 'Product Engineer Final Round', 'Remote Global Team Interview'],
  },
  {
    id: 'hr_interview',
    group: 'interview',
    name: '3. HR Interview Mode',
    descVi: 'Hỏi về bản thân, kinh nghiệm, mức lương, lý do nghỉ việc, mục tiêu nghề nghiệp.',
    suggestedTopics: ['Salary Negotiation & Career Goals', 'Why Are You Leaving Your Current Job?', 'Culture Fit & Work Style'],
  },
  {
    id: 'technical_interview',
    group: 'interview',
    name: '4. Technical Interview Mode',
    descVi: 'Hỏi chuyên sâu về C#, .NET, SQL Server, API, Architecture...',
    suggestedTopics: ['C# .NET 10, EF Core & SQL Server Indexing', 'SignalR, WebSockets & Async/Await Internals', 'Clean Architecture, SOLID & DI'],
  },
  {
    id: 'system_design',
    group: 'interview',
    name: '5. System Design Mode',
    descVi: 'Yêu cầu thiết kế hệ thống, thảo luận kiến trúc, scalability, database.',
    suggestedTopics: ['Design a Real-Time Chat & Voice Streaming System', 'High-Traffic E-Commerce Checkout & Caching', 'Distributed Rate Limiter & Notification Service'],
  },
  {
    id: 'behavioral_star',
    group: 'interview',
    name: '6. Behavioral Interview Mode (STAR)',
    descVi: 'Hỏi tình huống teamwork, conflict, leadership theo phương pháp STAR.',
    suggestedTopics: ['Resolving Disagreement with a Teammate', 'Handling a Critical Production Outage Under Pressure', 'Leading a Feature with Tight Deadlines'],
  },
  {
    id: 'stress_interview',
    group: 'interview',
    name: '7. Stress Interview Mode',
    descVi: 'Đặt câu hỏi khó, phản biện và gây áp lực như một buổi phỏng vấn căng thẳng.',
    suggestedTopics: ['Defending Your Architecture Under 10x Traffic', 'Why Should We Hire You Over More Experienced Candidates?', 'Handling Scope Creep & Missed Deadlines'],
  },
  {
    id: 'rapid_fire',
    group: 'interview',
    name: '8. Rapid Fire Mode',
    descVi: 'Hỏi liên tục những câu ngắn, yêu cầu trả lời nhanh trong 15–30 giây.',
    suggestedTopics: ['Quick Backend & Database Concepts', 'Rapid Workplace & Daily Q&A', 'Elevator Pitch & Quick Reflexes'],
  },
  {
    id: 'deep_dive',
    group: 'interview',
    name: '9. Deep Dive Mode',
    descVi: 'Đào sâu câu trả lời bằng các câu hỏi "Why?", "How?", "What if?".',
    suggestedTopics: ['Database Query Optimization & Locking', 'Microservices vs Modular Monolith Trade-offs', 'Authentication, JWT & Security Best Practices'],
  },
  {
    id: 'english_coach',
    group: 'interview',
    name: '10. English Coach Mode',
    descVi: 'Tập trung sửa ngữ pháp, từ vựng, độ trôi chảy và diễn đạt chuyên nghiệp.',
    suggestedTopics: ['Speaking Fluently About My Workday', 'Upgrading Basic Sentences to Business English', 'Using Natural Idioms & Collocations'],
  },
  {
    id: 'pronunciation_coach',
    group: 'interview',
    name: '11. Pronunciation Mode',
    descVi: 'Luyện phát âm, trọng âm, ngữ điệu và độ rõ ràng khi trả lời.',
    suggestedTopics: ['Ending Sounds (/s/, /z/, /t/, /d/) & Linking', 'Word Stress in Technical Vocabulary', 'Natural Intonation & Sentence Rhythm'],
  },
  {
    id: 'resume_deep_dive',
    group: 'interview',
    name: '12. Resume Deep Dive Mode',
    descVi: 'AI đóng vai nhà tuyển dụng, phỏng vấn dựa trên CV và các dự án bạn từng làm.',
    suggestedTopics: ['My Recent .NET & React Project Architecture', 'Most Challenging Bug & Performance Bottleneck I Solved', 'My Role, Tech Stack & Team Impact'],
  },

  // --- 7 COMMUNICATION & WORKPLACE MODES ---
  {
    id: 'daily_conversation',
    group: 'communication',
    name: '13. Daily Conversation',
    descVi: 'Trò chuyện tự nhiên về cuộc sống hằng ngày, sở thích, du lịch.',
    suggestedTopics: ['Travel, Food & Exploring New Places', 'Weekend Plans, Sports & Video Games', 'Morning Routine & Work-Life Balance'],
  },
  {
    id: 'daily_standup',
    group: 'communication',
    name: '14. Daily Standup (Scrum)',
    descVi: 'Giả lập họp Scrum hằng ngày: báo cáo hôm qua, hôm nay và các blocker.',
    suggestedTopics: ['Sprint 14 Backend API Progress & DB Blocker', 'Frontend UI Release & QA Bug Triage', 'Pre-Release Deployment Sync'],
  },
  {
    id: 'technical_discussion',
    group: 'communication',
    name: '15. Technical Discussion',
    descVi: 'Tranh luận về giải pháp kỹ thuật và code review với đồng nghiệp.',
    suggestedTopics: ['Choosing Between Redis Cache & In-Memory Cache', 'Refactoring Legacy SQL Stored Procedures', 'Code Review: Async Streaming & Memory Allocation'],
  },
  {
    id: 'problem_solving',
    group: 'communication',
    name: '16. Problem Solving',
    descVi: 'Cùng AI phân tích bug, sự cố hệ thống và tìm hướng giải quyết.',
    suggestedTopics: ['Investigating High CPU & Slow SQL Queries in Production', 'Fixing WebSocket Disconnection After 10 Minutes', 'Debugging Memory Leak in Background Worker'],
  },
  {
    id: 'client_meeting',
    group: 'communication',
    name: '17. Client Meeting',
    descVi: 'Trao đổi yêu cầu và giải thích vấn đề kỹ thuật với khách hàng.',
    suggestedTopics: ['Clarifying New Feature Requirements with US Client', 'Explaining Why a Technical Migration Takes 2 Weeks', 'Demoing New Product Release to Stakeholders'],
  },
  {
    id: 'debate_mode',
    group: 'communication',
    name: '18. Debate Mode',
    descVi: 'Tranh luận đa chiều để luyện phản xạ và lập luận tiếng Anh sắc bén.',
    suggestedTopics: ['Will AI Replace Software Engineers or Empower Them?', 'Remote Work vs Office Work for Engineering Teams', 'Speed of Delivery vs Code Quality in Startups'],
  },
  {
    id: 'roleplay_mode',
    group: 'communication',
    name: '19. Roleplay Mode',
    descVi: 'AI đóng vai khách hàng, quản lý, đồng nghiệp hoặc người lạ theo kịch bản.',
    suggestedTopics: ['1-on-1 Performance Review with Engineering Manager', 'Checking In at an International Hotel & Airport', 'Onboarding a New Foreign Teammate'],
  },
];

export default function App() {
  const [apiKey, setApiKey] = useState(() => localStorage.getItem('lingomint_api_key') || '');
  const [showKey, setShowKey] = useState(false);
  const [cefrLevel, setCefrLevel] = useState<CefrLevel>('A2');
  const [extendedThinking, setExtendedThinking] = useState(false);
  const [voiceName, setVoiceName] = useState<TeacherVoiceName>('Leda');
  const [speechSpeed, setSpeechSpeed] = useState<number>(0.9);

  // New v7 State: Conversation Mode (19 modes), Topic, and Optional Custom System Prompt
  const [conversationMode, setConversationMode] = useState<ConversationModeId>('practice_interview');
  const [topic, setTopic] = useState<string>('Backend .NET Developer Interview');
  const [customSystemPrompt, setCustomSystemPrompt] = useState<string>('');
  const [showCustomPromptBox, setShowCustomPromptBox] = useState<boolean>(false);

  const [activeWorkspace, setActiveWorkspace] = useState<'live_tutor' | 'daily_vocab'>('live_tutor');
  const [isSidebarExpanded, setIsSidebarExpanded] = useState(true);
  const [isHistoryDrawerOpen, setIsHistoryDrawerOpen] = useState(false);
  const [historySearch, setHistorySearch] = useState('');

  const chatEndRef = useRef<HTMLDivElement | null>(null);
  const apiKeyInputRef = useRef<HTMLInputElement | null>(null);

  const {
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
    selectConversation,
    createNewConversation,
    deleteConversation,
    connectAllThreeHubs,
    startMicrophone,
    stopMicrophone,
    disconnectAll,
    requestManualGrammarCheck,
  } = useThreeHubLiveSession(speechSpeed);

  useEffect(() => {
    localStorage.setItem('lingomint_api_key', apiKey);
  }, [apiKey]);

  useEffect(() => {
    chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [turns]);

  const activeModeObj =
    CONVERSATION_MODES.find((m) => m.id === conversationMode) || CONVERSATION_MODES[0];

  const handleModeChange = (newModeId: ConversationModeId) => {
    setConversationMode(newModeId);
    const found = CONVERSATION_MODES.find((m) => m.id === newModeId);
    if (found && found.suggestedTopics.length > 0 && !topic.trim()) {
      setTopic(found.suggestedTopics[0]);
    }
  };

  const handleSelectOldConversation = async (convId: string) => {
    if (isConnected) {
      await disconnectAll();
    }
    const loaded = await selectConversation(convId);
    if (loaded) {
      if (loaded.cefrLevel) setCefrLevel(loaded.cefrLevel as CefrLevel);
      if (typeof loaded.extendedThinking === 'boolean') setExtendedThinking(loaded.extendedThinking);
      if (loaded.voiceName) setVoiceName(loaded.voiceName as TeacherVoiceName);
      if (loaded.speechSpeed) setSpeechSpeed(Number(loaded.speechSpeed));
      if (loaded.conversationMode) setConversationMode(loaded.conversationMode as ConversationModeId);
      setTopic(loaded.topic || '');
      setCustomSystemPrompt(loaded.customSystemPrompt || '');
      if (loaded.customSystemPrompt) setShowCustomPromptBox(true);
    }
    setIsHistoryDrawerOpen(false);
  };

  const handleCreateNewConversation = async () => {
    if (isConnected) {
      await disconnectAll();
    }
    await createNewConversation(
      cefrLevel,
      extendedThinking,
      voiceName,
      speechSpeed,
      conversationMode,
      topic,
      customSystemPrompt
    );
    setIsHistoryDrawerOpen(false);
  };

  const expandAndFocusKey = () => {
    setIsSidebarExpanded(true);
    setTimeout(() => apiKeyInputRef.current?.focus(), 80);
  };

  const filteredConversations = conversations.filter(
    (c) =>
      c.title.toLowerCase().includes(historySearch.toLowerCase()) ||
      (c.topic || '').toLowerCase().includes(historySearch.toLowerCase()) ||
      c.lastMessagePreview.toLowerCase().includes(historySearch.toLowerCase())
  );

  return (
    <div className="learnerTheme min-h-screen flex flex-col lg:flex-row">
      {/* =====================================================================
          LEFT MENU: Expanded Full Setup OR Collapsed Icon-Only Rail
      ====================================================================== */}
      <aside
        className={`bg-[var(--bg-surface)] border-b lg:border-b-0 lg:border-r border-[var(--border-strong)] flex flex-col justify-between transition-all duration-200 shrink-0 lg:h-screen lg:sticky lg:top-0 ${
          isSidebarExpanded ? 'w-full lg:w-92 xl:w-96 p-4' : 'w-full lg:w-20 p-3 items-center'
        }`}
        style={{ boxShadow: 'var(--shadow-sm)' }}
      >
        {/* TOP BRAND LOGO + ICON TOGGLE BUTTON */}
        <div
          className={`w-full flex items-center ${
            isSidebarExpanded
              ? 'justify-between pb-3.5 border-b border-[var(--border-subtle)]'
              : 'flex-col gap-3 pb-3 border-b border-[var(--border-subtle)]'
          }`}
        >
          {isSidebarExpanded ? (
            <img
              src="/assets/brand/logo-horizontal.svg"
              alt="EnglishLearning Logo"
              className="h-11 w-auto select-none"
            />
          ) : (
            <img
              src="/assets/brand/logo-mark.svg"
              alt="Logo Mark"
              title="EnglishLearning — AI Live English Tutor"
              className="w-11 h-11 select-none cursor-pointer"
              onClick={() => setIsSidebarExpanded(true)}
            />
          )}

          <button
            type="button"
            onClick={() => setIsSidebarExpanded((prev) => !prev)}
            title={isSidebarExpanded ? 'Thu gọn Left Menu (chỉ hiện Icon)' : 'Mở rộng Left Menu'}
            className="w-9 h-9 flex items-center justify-center border border-[var(--border-strong)] bg-[var(--bg-app)] hover:bg-[var(--bg-subtle)] text-[var(--text-primary)] cursor-pointer transition-all shrink-0"
            style={{ borderRadius: 'var(--radius-xs)' }}
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 24 24"
              width="18"
              height="18"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <rect x="3" y="3" width="18" height="18" rx="4" />
              <line x1="9" y1="3" x2="9" y2="21" />
              {isSidebarExpanded ? (
                <polyline points="15 9 12 12 15 15" />
              ) : (
                <polyline points="13 9 16 12 13 15" />
              )}
            </svg>
          </button>
        </div>

        {/* =================================================================
            COLLAPSED ICON-ONLY RAIL VIEW
        ================================================================== */}
        {!isSidebarExpanded ? (
          <div className="flex-1 flex flex-row lg:flex-col items-center justify-between py-3 gap-2.5 w-full overflow-x-auto lg:overflow-x-visible">
            <div className="flex flex-row lg:flex-col items-center gap-2.5">
              {/* 0. Workspace Switcher Icon Buttons: Live Tutor vs Daily Vocabulary Flashcards */}
              <button
                type="button"
                onClick={() => setActiveWorkspace('live_tutor')}
                title="Hội thoại trực tiếp với AI Teacher (19 Modes)"
                className={`w-11 h-11 flex items-center justify-center border cursor-pointer ${
                  activeWorkspace === 'live_tutor'
                    ? 'bg-[var(--bg-subtle)] border-[var(--learner-primary-action)] ring-2 ring-[var(--learner-primary-action)]'
                    : 'bg-[var(--bg-app)] border-[var(--border-strong)]'
                }`}
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <img src="/assets/icons/navigation/nav-conversations.svg" alt="Live Tutor" className="w-6 h-6" />
              </button>

              <button
                type="button"
                onClick={() => setActiveWorkspace('daily_vocab')}
                title="Daily Vocabulary Flashcards & Chấm phát âm AI"
                className={`w-11 h-11 flex items-center justify-center border cursor-pointer ${
                  activeWorkspace === 'daily_vocab'
                    ? 'bg-[var(--bg-subtle)] border-[var(--learner-primary-action)] ring-2 ring-[var(--learner-primary-action)]'
                    : 'bg-[var(--bg-app)] border-[var(--border-strong)]'
                }`}
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <img src="/assets/icons/navigation/nav-today.svg" alt="Daily Vocab" className="w-6 h-6" />
              </button>
              {/* 1. Connect / Mic Icon Button */}
              {!isConnected ? (
                <button
                  type="button"
                  onClick={() =>
                    apiKey.trim()
                      ? connectAllThreeHubs(
                          apiKey,
                          cefrLevel,
                          extendedThinking,
                          voiceName,
                          speechSpeed,
                          conversationMode,
                          topic,
                          customSystemPrompt,
                          activeConversationId
                        )
                      : expandAndFocusKey()
                  }
                  title={apiKey.trim() ? 'Kết nối 3 Hubs' : 'Nhập API Key để kết nối'}
                  className="w-11 h-11 flex items-center justify-center bg-[var(--learner-primary-action)] hover:bg-[var(--learner-primary-action-hover)] text-[var(--text-on-primary)] cursor-pointer relative"
                  style={{ borderRadius: 'var(--radius-sm)', boxShadow: 'var(--shadow-sm)' }}
                >
                  <img src="/assets/icons/navigation/nav-study.svg" alt="Connect" className="w-6 h-6" />
                </button>
              ) : (
                <button
                  type="button"
                  onClick={isRecording ? stopMicrophone : startMicrophone}
                  title={isRecording ? 'Dừng nói (Mic đang bật)' : 'Bắt đầu nói (Mic)'}
                  className={`w-11 h-11 flex items-center justify-center text-white cursor-pointer relative ${
                    isRecording ? 'bg-red-600 hover:bg-red-700' : 'bg-[#38a385] hover:bg-[#2e886e]'
                  }`}
                  style={{ borderRadius: 'var(--radius-sm)', boxShadow: 'var(--shadow-sm)' }}
                >
                  <img src="/assets/icons/navigation/nav-study.svg" alt="Mic" className="w-6 h-6" />
                  <span
                    className={`absolute -top-1 -right-1 w-3 h-3 rounded-full border-2 border-white ${
                      isRecording ? 'bg-red-500 animate-ping' : 'bg-emerald-400'
                    }`}
                  />
                </button>
              )}

              {/* 2. New Conversation Icon Button */}
              <button
                type="button"
                onClick={handleCreateNewConversation}
                title="Tạo cuộc hội thoại mới"
                className="w-11 h-11 flex items-center justify-center border border-[var(--border-strong)] bg-[var(--bg-app)] hover:bg-[var(--bg-subtle)] cursor-pointer"
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <img src="/assets/icons/navigation/nav-conversations.svg" alt="New Chat" className="w-6 h-6" />
              </button>

              {/* 3. Open Separate History Drawer Icon Button */}
              <button
                type="button"
                onClick={() => setIsHistoryDrawerOpen(true)}
                title={`Lịch sử hội thoại (${conversations.length})`}
                className="w-11 h-11 flex items-center justify-center border border-[var(--learner-mint-border)] bg-[var(--bg-subtle)] hover:bg-[var(--learner-mint-surface-hover)] cursor-pointer relative"
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <img src="/assets/icons/navigation/nav-history.svg" alt="History" className="w-6 h-6" />
                <span className="absolute -top-1 -right-1 px-1.5 py-0.2 text-[9px] font-bold bg-[var(--learner-primary-action)] text-[var(--text-on-primary)] rounded-full">
                  {conversations.length}
                </span>
              </button>

              {/* 4. Mode & Topic Icon Button */}
              <button
                type="button"
                onClick={() => setIsSidebarExpanded(true)}
                title={`Chế độ: ${activeModeObj.name} | Chủ đề: ${topic || 'Tự do'}`}
                className="w-11 h-11 flex items-center justify-center border border-[var(--learner-mint-border)] bg-[var(--color-primary-50)] hover:bg-[var(--bg-subtle)] cursor-pointer"
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <img src="/assets/icons/navigation/nav-catalog.svg" alt="Mode & Topic" className="w-6 h-6" />
              </button>

              {/* 5. API Key Setup Icon Button */}
              <button
                type="button"
                onClick={expandAndFocusKey}
                title="Cài đặt Google AI Studio API Key"
                className="w-11 h-11 flex items-center justify-center border border-[var(--border-strong)] bg-[var(--bg-app)] hover:bg-[var(--bg-subtle)] cursor-pointer relative"
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <img src="/assets/icons/navigation/nav-admin.svg" alt="API Key" className="w-6 h-6" />
                <span
                  className={`absolute bottom-1 right-1 w-2 h-2 rounded-full ${
                    apiKey.trim() ? 'bg-[#38a385]' : 'bg-amber-500'
                  }`}
                />
              </button>

              {/* 6. Teacher Voice & Speed Icon Button */}
              <button
                type="button"
                onClick={() => setIsSidebarExpanded(true)}
                title={`Giọng Teacher: ${voiceName} (${speechSpeed}x)`}
                className="w-11 h-11 flex flex-col items-center justify-center border border-[var(--border-strong)] bg-[var(--bg-app)] hover:bg-[var(--bg-subtle)] cursor-pointer"
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <img src="/assets/icons/navigation/nav-account.svg" alt="Voice" className="w-5 h-5" />
                <span className="text-[9px] font-bold text-[var(--text-primary)] leading-none mt-0.5">
                  {speechSpeed}x
                </span>
              </button>

              {/* 7. Extended Thinking Toggle Icon Button */}
              <button
                type="button"
                onClick={() => setExtendedThinking((v) => !v)}
                title={`Extended Thinking: ${extendedThinking ? 'ON' : 'OFF'}`}
                className={`w-11 h-11 flex flex-col items-center justify-center border cursor-pointer ${
                  extendedThinking
                    ? 'bg-[var(--bg-subtle)] border-[var(--learner-accent)]'
                    : 'bg-[var(--bg-app)] border-[var(--border-strong)]'
                }`}
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <img src="/assets/icons/navigation/nav-reviews.svg" alt="Thinking" className="w-5 h-5" />
                <span className="text-[8px] font-bold text-[var(--learner-mint-text)] leading-none mt-0.5">
                  {extendedThinking ? 'THINK' : 'STD'}
                </span>
              </button>

              {/* 8. CEFR Level Icon Button */}
              <button
                type="button"
                onClick={() => setIsSidebarExpanded(true)}
                title={`Trình độ CEFR hiện tại: ${cefrLevel}`}
                className="w-11 h-11 flex flex-col items-center justify-center border border-[var(--learner-mint-border)] bg-[var(--color-primary-50)] hover:bg-[var(--bg-subtle)] cursor-pointer"
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <img src="/assets/icons/navigation/nav-lessons.svg" alt="CEFR" className="w-5 h-5" />
                <span className="text-[10px] font-extrabold text-[var(--text-primary)] leading-none mt-0.5">
                  {cefrLevel}
                </span>
              </button>
            </div>

            <div
              className="hidden lg:flex flex-col items-center gap-1.5 p-2 border border-[var(--learner-mint-border)] bg-[var(--color-primary-50)]"
              style={{ borderRadius: 'var(--radius-xs)' }}
              title={`Tutor: ${hubStatuses.tutor} | Transcribe: ${hubStatuses.transcribe} | Translate: ${hubStatuses.translate}`}
            >
              <span
                className={`w-2.5 h-2.5 rounded-full ${
                  hubStatuses.tutor === 'Connected' ? 'bg-[#38a385]' : 'bg-amber-500'
                }`}
              />
              <span
                className={`w-2.5 h-2.5 rounded-full ${
                  hubStatuses.transcribe === 'Connected' ? 'bg-[#38a385]' : 'bg-amber-500'
                }`}
              />
              <span
                className={`w-2.5 h-2.5 rounded-full ${
                  hubStatuses.translate === 'Connected' ? 'bg-[#38a385]' : 'bg-amber-500'
                }`}
              />
            </div>
          </div>
        ) : (
          /* =================================================================
              EXPANDED LEFT MENU VIEW
          ================================================================== */
          <div className="flex-1 overflow-y-auto py-3 space-y-3.5 pr-1">
            {/* Workspace Feature Switcher: Live AI Tutor vs. Daily Vocabulary Flashcards */}
            <div
              className="p-1 bg-[var(--bg-app)] border border-[var(--border-strong)] grid grid-cols-2 gap-1"
              style={{ borderRadius: 'var(--radius-sm)' }}
            >
              <button
                type="button"
                onClick={() => setActiveWorkspace('live_tutor')}
                className={`py-2 px-2 text-xs font-bold flex items-center justify-center gap-1.5 transition-all cursor-pointer ${
                  activeWorkspace === 'live_tutor'
                    ? 'bg-[var(--learner-primary-action)] text-[var(--text-on-primary)]'
                    : 'text-[var(--text-secondary)] hover:bg-[var(--color-primary-50)]'
                }`}
                style={{ borderRadius: 'var(--radius-xs)' }}
              >
                <img src="/assets/icons/navigation/nav-study.svg" alt="" className="w-4 h-4" />
                <span>Live AI Tutor</span>
              </button>
              <button
                type="button"
                onClick={() => setActiveWorkspace('daily_vocab')}
                className={`py-2 px-2 text-xs font-bold flex items-center justify-center gap-1.5 transition-all cursor-pointer ${
                  activeWorkspace === 'daily_vocab'
                    ? 'bg-[var(--learner-primary-action)] text-[var(--text-on-primary)]'
                    : 'text-[var(--text-secondary)] hover:bg-[var(--color-primary-50)]'
                }`}
                style={{ borderRadius: 'var(--radius-xs)' }}
              >
                <img src="/assets/icons/navigation/nav-today.svg" alt="" className="w-4 h-4" />
                <span>Daily Flashcards</span>
              </button>
            </div>
            {/* Quick Actions: New Chat & Open Separate History Drawer */}
            <div className="grid grid-cols-2 gap-2">
              <button
                type="button"
                onClick={handleCreateNewConversation}
                className="p-2.5 text-xs font-bold border border-[var(--border-strong)] bg-[var(--bg-app)] hover:bg-[var(--bg-subtle)] flex items-center justify-center gap-2 cursor-pointer"
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <img src="/assets/icons/navigation/nav-conversations.svg" alt="" className="w-4 h-4" />
                <span>+ Hội thoại mới</span>
              </button>

              <button
                type="button"
                onClick={() => setIsHistoryDrawerOpen(true)}
                className="p-2.5 text-xs font-bold border border-[var(--learner-mint-border)] bg-[var(--bg-subtle)] hover:bg-[var(--learner-mint-surface-hover)] text-[var(--learner-mint-text)] flex items-center justify-center gap-1.5 cursor-pointer"
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <img src="/assets/icons/navigation/nav-history.svg" alt="" className="w-4 h-4" />
                <span>Lịch sử ({conversations.length})</span>
              </button>
            </div>

            {/* 1. CONVERSATION MODE SELECTOR (19 Modes: 12 Interview + 7 Communication) */}
            <div className="space-y-1.5 pt-2 border-t border-[var(--border-subtle)]">
              <label className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                <img src="/assets/icons/navigation/nav-catalog.svg" alt="" className="w-4 h-4" />
                <span>Chế độ luyện nói (19 Modes)</span>
              </label>
              <select
                value={conversationMode}
                onChange={(e) => handleModeChange(e.target.value as ConversationModeId)}
                className="w-full px-3 py-2 text-xs font-semibold bg-[var(--bg-app)] border border-[var(--border-strong)] focus:outline-none focus:ring-2 focus:ring-[#26443c]"
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <optgroup label="🎯 Chế Độ Phỏng Vấn & Huấn Luyện (12 Modes)">
                  {CONVERSATION_MODES.filter((m) => m.group === 'interview').map((m) => (
                    <option key={m.id} value={m.id}>
                      {m.name}
                    </option>
                  ))}
                </optgroup>
                <optgroup label="💬 Giao Tiếp & Công Việc Thực Tế (7 Modes)">
                  {CONVERSATION_MODES.filter((m) => m.group === 'communication').map((m) => (
                    <option key={m.id} value={m.id}>
                      {m.name}
                    </option>
                  ))}
                </optgroup>
              </select>
              <div
                className="p-2 text-[11px] bg-[var(--color-primary-50)] text-[var(--learner-mint-text)] border border-[var(--learner-mint-border)] leading-snug"
                style={{ borderRadius: 'var(--radius-xs)' }}
              >
                <strong>Chức năng:</strong> {activeModeObj.descVi}
              </div>
            </div>

            {/* 2. CONVERSATION TOPIC + QUICK SUGGESTIONS */}
            <div className="space-y-1.5 pt-2 border-t border-[var(--border-subtle)]">
              <label className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                <img src="/assets/icons/navigation/nav-today.svg" alt="" className="w-4 h-4" />
                <span>Chủ đề cuộc trò chuyện (Topic)</span>
              </label>
              <input
                type="text"
                value={topic}
                onChange={(e) => setTopic(e.target.value)}
                placeholder="Nhập chủ đề (VD: C# .NET 10, SQL Server, Daily Standup...)"
                className="w-full px-3 py-2 text-xs bg-[var(--bg-app)] border border-[var(--border-strong)] focus:outline-none focus:ring-2 focus:ring-[#26443c]"
                style={{ borderRadius: 'var(--radius-sm)' }}
              />
              <div className="flex flex-wrap gap-1 pt-0.5">
                {activeModeObj.suggestedTopics.map((sug) => (
                  <button
                    key={sug}
                    type="button"
                    onClick={() => setTopic(sug)}
                    className={`px-2 py-0.5 text-[10px] font-medium border transition-all cursor-pointer truncate max-w-full ${
                      topic === sug
                        ? 'bg-[var(--bg-subtle)] border-[var(--learner-primary-action)] text-[var(--learner-mint-text)] font-bold'
                        : 'bg-[var(--bg-app)] border-[var(--border-subtle)] text-[var(--text-secondary)] hover:bg-[var(--color-primary-50)]'
                    }`}
                    style={{ borderRadius: 'var(--radius-full)' }}
                  >
                    + {sug}
                  </button>
                ))}
              </div>
            </div>

            {/* 3. OPTIONAL CUSTOM SYSTEM PROMPT PER CONVERSATION */}
            <div className="space-y-1.5 pt-2 border-t border-[var(--border-subtle)]">
              <div className="flex items-center justify-between">
                <label className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                  <img src="/assets/icons/navigation/nav-sentences.svg" alt="" className="w-4 h-4" />
                  <span>System Prompt riêng (Optional)</span>
                </label>
                <button
                  type="button"
                  onClick={() => setShowCustomPromptBox((prev) => !prev)}
                  className="text-[11px] font-bold text-[var(--learner-mint-text)] underline cursor-pointer"
                >
                  {showCustomPromptBox ? 'Thu gọn' : '+ Thêm Prompt / CV / JD'}
                </button>
              </div>
              {showCustomPromptBox && (
                <textarea
                  rows={3}
                  value={customSystemPrompt}
                  onChange={(e) => setCustomSystemPrompt(e.target.value)}
                  placeholder="Tùy chọn: Dán tóm tắt CV, JD vị trí ứng tuyển hoặc chỉ dẫn riêng cho AI trong cuộc hội thoại này..."
                  className="w-full p-2.5 text-xs bg-[var(--bg-app)] border border-[var(--border-strong)] focus:outline-none focus:ring-2 focus:ring-[#26443c] resize-y"
                  style={{ borderRadius: 'var(--radius-sm)' }}
                />
              )}
            </div>

            {/* 4. Google AI Studio API Key */}
            <div className="space-y-1.5 pt-2 border-t border-[var(--border-subtle)]">
              <label className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                <img src="/assets/icons/navigation/nav-admin.svg" alt="" className="w-4 h-4" />
                <span>Google AI Studio API Key</span>
              </label>
              <div className="relative flex items-center">
                <input
                  ref={apiKeyInputRef}
                  type={showKey ? 'text' : 'password'}
                  value={apiKey}
                  onChange={(e) => setApiKey(e.target.value)}
                  placeholder="AIzaSy... (aistudio.google.com)"
                  className="w-full pl-3 pr-14 py-2 text-xs bg-[var(--bg-app)] border border-[var(--border-strong)] focus:outline-none focus:ring-2 focus:ring-[#26443c]"
                  style={{ borderRadius: 'var(--radius-sm)' }}
                />
                <button
                  type="button"
                  onClick={() => setShowKey((s) => !s)}
                  className="absolute right-2 text-[11px] font-semibold text-[var(--text-secondary)] hover:text-[var(--text-primary)] cursor-pointer"
                >
                  {showKey ? 'Ẩn' : 'Hiện'}
                </button>
              </div>
            </div>

            {/* 5. Teacher Voice & Speech Speed */}
            <div className="space-y-2.5 pt-2 border-t border-[var(--border-subtle)]">
              <div>
                <label className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-1">
                  <img src="/assets/icons/navigation/nav-account.svg" alt="" className="w-4 h-4" />
                  <span>Giọng nói của Teacher ({voiceName})</span>
                </label>
                <select
                  value={voiceName}
                  onChange={(e) => setVoiceName(e.target.value as TeacherVoiceName)}
                  className="w-full px-3 py-2 text-xs font-medium bg-[var(--bg-app)] border border-[var(--border-strong)] focus:outline-none"
                  style={{ borderRadius: 'var(--radius-sm)' }}
                >
                  {TEACHER_VOICES.map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.label} — {v.style}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-1">
                  <img src="/assets/icons/navigation/nav-settings.svg" alt="" className="w-4 h-4" />
                  <span>Tốc độ nói ({speechSpeed}x)</span>
                </label>
                <div className="grid grid-cols-5 gap-1.5">
                  {SPEECH_SPEEDS.map((s) => {
                    const active = Math.abs(speechSpeed - s.value) < 0.01;
                    return (
                      <button
                        key={s.value}
                        type="button"
                        onClick={() => setSpeechSpeed(s.value)}
                        className={`py-1.5 text-xs font-bold border transition-all cursor-pointer ${
                          active
                            ? 'bg-[var(--learner-primary-action)] text-[var(--text-on-primary)] border-[var(--learner-primary-action)]'
                            : 'bg-[var(--bg-app)] text-[var(--text-primary)] border-[var(--border-strong)] hover:bg-[var(--color-primary-50)]'
                        }`}
                        style={{ borderRadius: 'var(--radius-xs)' }}
                      >
                        {s.label}
                      </button>
                    );
                  })}
                </div>
              </div>
            </div>

            {/* 6. Extended Thinking Toggle */}
            <div className="pt-2 border-t border-[var(--border-subtle)]">
              <label className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-1">
                <img src="/assets/icons/navigation/nav-reviews.svg" alt="" className="w-4 h-4" />
                <span>Chế độ suy luận (Model Voice)</span>
              </label>
              <button
                type="button"
                onClick={() => setExtendedThinking((v) => !v)}
                className={`w-full px-3 py-2 text-xs font-semibold border flex items-center justify-between transition-all cursor-pointer ${
                  extendedThinking
                    ? 'bg-[var(--bg-subtle)] border-[var(--learner-accent)] text-[var(--learner-mint-text)]'
                    : 'bg-[var(--bg-app)] border-[var(--border-strong)] text-[var(--text-primary)]'
                }`}
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <span className="truncate">
                  {extendedThinking ? 'Gemini 3.8 Live Extended Thinking' : 'Gemini 3.8 Live (Standard)'}
                </span>
                <span
                  className={`px-2 py-0.5 text-[10px] font-bold rounded-full ${
                    extendedThinking
                      ? 'bg-[var(--learner-primary-action)] text-[var(--text-on-primary)]'
                      : 'bg-gray-200 text-gray-700'
                  }`}
                >
                  {extendedThinking ? 'THINKING ON' : 'OFF'}
                </span>
              </button>
            </div>

            {/* 7. CEFR Level Selector A1 -> C2 */}
            <div className="pt-2 border-t border-[var(--border-subtle)]">
              <label className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-1">
                <img src="/assets/icons/navigation/nav-lessons.svg" alt="" className="w-4 h-4" />
                <span>Trình độ hiện tại (CEFR: {cefrLevel})</span>
              </label>
              <div className="grid grid-cols-3 gap-1.5">
                {CEFR_LEVELS.map((item) => {
                  const active = cefrLevel === item.level;
                  return (
                    <button
                      key={item.level}
                      type="button"
                      onClick={() => setCefrLevel(item.level)}
                      className={`p-2 text-left border transition-all cursor-pointer ${
                        active
                          ? 'bg-[var(--bg-subtle)] border-[var(--learner-primary-action)] ring-1 ring-[var(--learner-primary-action)]'
                          : 'bg-[var(--bg-app)] border-[var(--border-strong)] hover:bg-[var(--color-primary-50)]'
                      }`}
                      style={{ borderRadius: 'var(--radius-xs)' }}
                    >
                      <div className="font-bold text-xs text-[var(--text-primary)]">{item.level}</div>
                      <div className="text-[10px] text-[var(--text-secondary)] truncate">{item.desc}</div>
                    </button>
                  );
                })}
              </div>
            </div>

            {/* 8. 3 SignalR Hub Statuses & Connect Button */}
            <div className="pt-2.5 border-t border-[var(--border-subtle)] space-y-2">
              <div className="space-y-1">
                {[
                  { name: 'Hub 1: Tutor (3.8 Live)', state: hubStatuses.tutor },
                  { name: 'Hub 2: Transcribe (3.5 Line 1)', state: hubStatuses.transcribe },
                  { name: 'Hub 3: Translate VI (3.5 Line 2)', state: hubStatuses.translate },
                ].map((h) => (
                  <div
                    key={h.name}
                    className="px-2.5 py-1 text-[11px] font-medium border bg-[var(--color-primary-50)] border-[var(--learner-mint-border)] flex items-center justify-between"
                    style={{ borderRadius: 'var(--radius-xs)' }}
                  >
                    <span>{h.name}</span>
                    <span className="flex items-center gap-1 font-semibold">
                      <span
                        className={`w-2 h-2 rounded-full ${
                          h.state === 'Connected' ? 'bg-[#38a385]' : 'bg-amber-500'
                        }`}
                      />
                      {h.state}
                    </span>
                  </div>
                ))}
              </div>

              {!isConnected ? (
                <button
                  type="button"
                  onClick={() =>
                    connectAllThreeHubs(
                      apiKey,
                      cefrLevel,
                      extendedThinking,
                      voiceName,
                      speechSpeed,
                      conversationMode,
                      topic,
                      customSystemPrompt,
                      activeConversationId
                    )
                  }
                  disabled={!apiKey.trim() || isConnecting}
                  className="w-full py-2.5 text-xs font-bold uppercase tracking-wider text-[var(--text-on-primary)] bg-[var(--learner-primary-action)] hover:bg-[var(--learner-primary-action-hover)] disabled:opacity-50 flex items-center justify-center gap-2 cursor-pointer"
                  style={{ borderRadius: 'var(--radius-sm)', boxShadow: 'var(--shadow-sm)' }}
                >
                  <img src="/assets/icons/navigation/nav-study.svg" alt="" className="w-4 h-4" />
                  <span>{isConnecting ? 'Đang kết nối...' : 'Kết nối 3 Hubs & Bắt đầu'}</span>
                </button>
              ) : (
                <div className="flex gap-2">
                  <button
                    type="button"
                    onClick={isRecording ? stopMicrophone : startMicrophone}
                    className={`flex-1 py-2.5 text-xs font-bold text-white cursor-pointer ${
                      isRecording ? 'bg-red-600 hover:bg-red-700' : 'bg-[#38a385] hover:bg-[#2e886e]'
                    }`}
                    style={{ borderRadius: 'var(--radius-sm)' }}
                  >
                    {isRecording ? '⏹ Dừng nói' : '🎙️ Bắt đầu nói (Mic)'}
                  </button>
                  <button
                    type="button"
                    onClick={disconnectAll}
                    className="px-3 py-2.5 text-xs font-semibold border border-[var(--border-strong)] text-[var(--text-secondary)] hover:bg-red-50 hover:text-red-700 cursor-pointer"
                    style={{ borderRadius: 'var(--radius-sm)' }}
                  >
                    Ngắt
                  </button>
                </div>
              )}
            </div>
          </div>
        )}
      </aside>

      {/* =====================================================================
          MAIN WORKSPACE: Live Bilingual Messenger + Grammar & Spelling Corrections
      ====================================================================== */}
      <main className="flex-1 p-4 md:p-6 flex flex-col gap-4 max-w-7xl mx-auto w-full">
        {activeWorkspace === 'daily_vocab' ? (
          <DailyVocabularyWorkspace
            apiKey={apiKey}
            cefrLevel={cefrLevel}
            defaultTopic={topic}
            onFocusApiKey={expandAndFocusKey}
          />
        ) : (
          <>
        {errorMessage && (
          <div
            className="p-4 bg-red-50 border border-red-200 text-red-800 flex items-center gap-4"
            style={{ borderRadius: 'var(--radius-md)' }}
          >
            <img
              src="/assets/illustrations/illustration-load-error.svg"
              alt="Error"
              className="w-16 h-14 shrink-0"
            />
            <div className="text-xs space-y-1">
              <div className="font-bold text-sm">Không thể kết nối phiên trực tiếp</div>
              <div>{errorMessage}</div>
            </div>
          </div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 flex-1">
          {/* Center 7 Cols: Chat Messenger Stream */}
          <section
            className="lg:col-span-7 p-5 md:p-6 bg-[var(--bg-surface)] border border-[var(--border-strong)] flex flex-col justify-between min-h-[680px] max-h-[calc(100vh-48px)]"
            style={{ borderRadius: 'var(--radius-lg)', boxShadow: 'var(--shadow-md)' }}
          >
            <div className="flex flex-wrap items-center justify-between gap-2 pb-3.5 border-b border-[var(--border-subtle)]">
              <div className="flex items-center gap-3 min-w-0">
                <img
                  src="/assets/mascot/cat/cat-avatar.svg"
                  alt="Teacher Avatar"
                  className="w-10 h-10 shrink-0"
                />
                <div className="min-w-0">
                  <div className="flex flex-wrap items-center gap-1.5">
                    <h2 className="text-base md:text-lg font-bold text-[var(--text-primary)] truncate">
                      Hội Thoại Trực Tiếp &amp; Phụ Đề Song Ngữ
                    </h2>
                    <span
                      className="px-2 py-0.5 text-[10px] font-bold bg-[var(--learner-primary-action)] text-[var(--text-on-primary)]"
                      style={{ borderRadius: 'var(--radius-full)' }}
                    >
                      {activeModeObj.name}
                    </span>
                  </div>
                  <p className="text-xs text-[var(--text-secondary)] truncate">
                    <strong>Chủ đề:</strong> {topic || 'Giao tiếp tự do'}
                    {customSystemPrompt.trim() ? ' · ✨ Có Custom System Prompt' : ''}
                  </p>
                </div>
              </div>

              <div className="flex items-center gap-2">
                <span
                  className="px-2.5 py-1 text-xs font-semibold bg-[var(--bg-subtle)] text-[var(--learner-mint-text)] border border-[var(--learner-mint-border)]"
                  style={{ borderRadius: 'var(--radius-full)' }}
                >
                  🎙️ {voiceName} · {speechSpeed}x · {interactionStatus}
                </span>
              </div>
            </div>

            <div className="flex-1 overflow-y-auto py-4 space-y-4 pr-1">
              {turns.length === 0 ? (
                <div className="h-full flex flex-col items-center justify-center text-center p-6 text-sm text-[var(--text-muted)] space-y-3">
                  <img
                    src="/assets/illustrations/illustration-welcome.svg"
                    alt="Welcome"
                    className="w-44 h-36 select-none"
                  />
                  <p className="font-bold text-base text-[var(--text-primary)]">
                    Sẵn sàng luyện nói: {activeModeObj.name}
                  </p>
                  <p className="text-xs max-w-md text-[var(--text-secondary)]">
                    {activeModeObj.descVi} <br />
                    <strong>Chủ đề đang chọn:</strong> {topic || 'Tự do'}. Nhấn <strong>&ldquo;Kết nối 3 Hubs &amp; Bắt đầu&rdquo;</strong> để bắt đầu!
                  </p>
                </div>
              ) : (
                turns.map((turn) => {
                  const isUser = turn.speaker === 'user';
                  return (
                    <div
                      key={`${turn.speaker}-${turn.turnId}`}
                      className={`flex flex-col ${isUser ? 'items-end' : 'items-start'}`}
                    >
                      <div className="flex items-center gap-2 text-[11px] font-semibold text-[var(--text-secondary)] mb-1 px-1">
                        {!isUser && (
                          <img
                            src="/assets/mascot/cat/cat-avatar.svg"
                            alt="AI"
                            className="w-5 h-5 rounded-full"
                          />
                        )}
                        <span>
                          {isUser
                            ? `Bạn (Level ${cefrLevel})`
                            : `AI English Teacher (${
                                extendedThinking
                                  ? 'Gemini 3.8 Live Extended Thinking'
                                  : 'Gemini 3.8 Live'
                              } · ${voiceName} ${speechSpeed}x)`}
                        </span>
                        {isUser && (
                          <img
                            src="/assets/icons/navigation/nav-account.svg"
                            alt="User"
                            className="w-4 h-4 rounded-full"
                          />
                        )}
                        <span className="text-[10px] text-[var(--text-muted)]">
                          #{turn.sequenceOrder} · {turn.timestamp}
                        </span>
                        {isUser && turn.transcriptLine1 && (
                          <button
                            type="button"
                            onClick={() =>
                              requestManualGrammarCheck(turn.turnId, turn.transcriptLine1, cefrLevel)
                            }
                            className="ml-1 px-2 py-0.5 text-[10px] font-bold bg-[var(--bg-subtle)] text-[var(--learner-mint-text)] border border-[var(--learner-mint-border)] rounded-full hover:bg-[var(--learner-mint-surface-hover)] cursor-pointer"
                            title="Phân tích ngữ pháp & từ vựng câu này"
                          >
                            ✓ Check lỗi
                          </button>
                        )}
                      </div>

                      <div
                        className={`max-w-[88%] p-4 border transition-all ${
                          isUser
                            ? 'bg-[var(--bg-subtle)] border-[var(--learner-mint-border)] rounded-2xl rounded-tr-xs'
                            : 'bg-[var(--bg-app)] border-[var(--border-strong)] rounded-2xl rounded-tl-xs'
                        }`}
                        style={{ boxShadow: 'var(--shadow-sm)' }}
                      >
                        <p
                          className="font-semibold text-[var(--text-primary)] leading-snug"
                          style={{ fontSize: 'var(--font-size-study-body)' }}
                        >
                          {turn.transcriptLine1 || '...'}
                        </p>

                        <p className="mt-1.5 text-sm italic text-[var(--text-secondary)] leading-relaxed border-t border-black/5 pt-1.5">
                          {turn.translationLine2 || 'Đang dịch sang tiếng Việt...'}
                        </p>
                      </div>
                    </div>
                  );
                })
              )}
              <div ref={chatEndRef} />
            </div>
          </section>

          {/* Right 5 Cols: Real-time Spelling, Grammar & Pronunciation Corrections */}
          <section
            className="lg:col-span-5 p-5 md:p-6 bg-[var(--bg-surface)] border border-[var(--border-strong)] flex flex-col min-h-[680px] max-h-[calc(100vh-48px)]"
            style={{ borderRadius: 'var(--radius-lg)', boxShadow: 'var(--shadow-md)' }}
          >
            <div className="pb-4 border-b border-[var(--border-subtle)] flex items-center justify-between">
              <div className="flex items-center gap-2.5">
                <img
                  src="/assets/icons/navigation/nav-sentences.svg"
                  alt="Corrections"
                  className="w-7 h-7 shrink-0"
                />
                <div>
                  <h2 className="text-lg font-bold text-[var(--text-primary)]">
                    Sửa Lỗi Chính Tả, Từ Vựng &amp; Ngữ Pháp
                  </h2>
                  <p className="text-xs text-[var(--text-secondary)]">
                    Chỉ hiển thị gợi ý tại đây · Không thay đổi tin nhắn gốc của bạn
                  </p>
                </div>
              </div>
              <span
                className="px-2.5 py-1 text-xs font-bold bg-[var(--bg-subtle)] text-[var(--learner-mint-text)] border border-[var(--learner-mint-border)]"
                style={{ borderRadius: 'var(--radius-full)' }}
              >
                {corrections.length} lỗi
              </span>
            </div>

            <div className="flex-1 overflow-y-auto py-4 space-y-3 pr-1">
              {corrections.length === 0 ? (
                <div className="h-full flex flex-col items-center justify-center text-center text-sm text-[var(--text-muted)] p-6 space-y-3">
                  <img
                    src="/assets/illustrations/illustration-calm-rest.svg"
                    alt="No corrections yet"
                    className="w-40 h-32 select-none"
                  />
                  <p className="font-semibold text-[var(--text-primary)]">
                    Chưa phát hiện lỗi ngữ pháp hay từ vựng nào
                  </p>
                  <p className="text-xs max-w-xs text-[var(--text-secondary)]">
                    Khi bạn nói sai ngữ pháp, thiếu âm cuối số nhiều (-s/-es) hoặc dùng từ chưa tự nhiên, thẻ sửa lỗi chi tiết sẽ xuất hiện ngay tại đây.
                  </p>
                </div>
              ) : (
                corrections.map((item, idx) => (
                  <div
                    key={`${item.turnId}-${idx}`}
                    className="p-4 bg-[var(--bg-app)] border border-[var(--border-strong)] space-y-2"
                    style={{ borderRadius: 'var(--radius-md)', boxShadow: 'var(--shadow-sm)' }}
                  >
                    <div className="flex items-center justify-between">
                      <span
                        className="px-2.5 py-0.5 text-xs font-bold bg-[var(--bg-subtle)] text-[var(--learner-mint-text)] border border-[var(--learner-mint-border)]"
                        style={{ borderRadius: 'var(--radius-full)' }}
                      >
                        {item.errorCategory}
                      </span>
                      <span className="text-xs text-[var(--text-muted)]">{item.timestamp}</span>
                    </div>

                    <div className="text-sm">
                      <span className="line-through text-red-700/80 mr-2">{item.originalText}</span>
                      <span className="font-bold text-[#1b4b3e]">→ {item.correctedText}</span>
                    </div>

                    <p className="text-xs text-[var(--text-secondary)] leading-relaxed">
                      <strong>Giải thích:</strong> {item.explanationVi}
                    </p>

                    <div
                      className="p-2.5 text-xs bg-[var(--color-primary-50)] text-[var(--learner-reading-text)] border border-[var(--learner-mint-border)]"
                      style={{ borderRadius: 'var(--radius-xs)' }}
                    >
                      <strong>Mẹo CEFR ({cefrLevel}):</strong> {item.cefrTip}
                    </div>
                  </div>
                ))
              )}
            </div>
          </section>
        </div>
          </>
        )}
      </main>

      {/* =====================================================================
          SEPARATE CONVERSATION HISTORY DRAWER
      ====================================================================== */}
      {isHistoryDrawerOpen && (
        <div className="fixed inset-0 z-50 flex justify-end bg-black/30 backdrop-blur-xs">
          <div
            className="w-full max-w-md bg-[var(--bg-surface)] h-full border-l border-[var(--border-strong)] p-6 flex flex-col justify-between"
            style={{ boxShadow: 'var(--shadow-xl)' }}
          >
            <div className="space-y-4">
              <div className="flex items-center justify-between pb-3 border-b border-[var(--border-subtle)]">
                <div className="flex items-center gap-2.5">
                  <img
                    src="/assets/icons/navigation/nav-history.svg"
                    alt="History"
                    className="w-7 h-7"
                  />
                  <div>
                    <span
                      className="px-2 py-0.5 text-[10px] font-bold uppercase bg-[var(--bg-subtle)] text-[var(--learner-mint-text)] border border-[var(--learner-mint-border)]"
                      style={{ borderRadius: 'var(--radius-full)' }}
                    >
                      SQL Server · LingoMint DB
                    </span>
                    <h3
                      className="mt-0.5 text-xl font-bold text-[var(--text-primary)]"
                      style={{ fontFamily: 'var(--font-family-display)' }}
                    >
                      Lịch Sử Hội Thoại ({conversations.length})
                    </h3>
                  </div>
                </div>
                <button
                  type="button"
                  onClick={() => setIsHistoryDrawerOpen(false)}
                  className="px-3 py-1.5 text-xs font-bold border border-[var(--border-strong)] bg-[var(--bg-app)] hover:bg-[var(--bg-subtle)] cursor-pointer"
                  style={{ borderRadius: 'var(--radius-xs)' }}
                >
                  Đóng ✕
                </button>
              </div>

              <div className="flex gap-2">
                <input
                  type="text"
                  value={historySearch}
                  onChange={(e) => setHistorySearch(e.target.value)}
                  placeholder="Tìm theo tiêu đề hoặc chủ đề..."
                  className="flex-1 px-3 py-2 text-xs bg-[var(--bg-app)] border border-[var(--border-strong)] focus:outline-none"
                  style={{ borderRadius: 'var(--radius-xs)' }}
                />
                <button
                  type="button"
                  onClick={handleCreateNewConversation}
                  className="px-3.5 py-2 text-xs font-bold text-[var(--text-on-primary)] bg-[var(--learner-primary-action)] hover:bg-[var(--learner-primary-action-hover)] cursor-pointer"
                  style={{ borderRadius: 'var(--radius-xs)' }}
                >
                  + Tạo mới
                </button>
              </div>
            </div>

            <div className="flex-1 overflow-y-auto my-4 space-y-2.5 pr-1">
              {filteredConversations.length === 0 ? (
                <div className="p-6 flex flex-col items-center text-center text-xs text-[var(--text-muted)] bg-[var(--bg-app)] border border-[var(--border-subtle)] rounded-[12px] space-y-2">
                  <img
                    src="/assets/illustrations/illustration-searching.svg"
                    alt="Empty search"
                    className="w-32 h-28"
                  />
                  <span>Chưa có cuộc hội thoại nào khớp tìm kiếm.</span>
                </div>
              ) : (
                filteredConversations.map((conv) => {
                  const isSelected = conv.id === activeConversationId;
                  const modeLabel =
                    CONVERSATION_MODES.find((m) => m.id === conv.conversationMode)?.name ||
                    conv.conversationMode ||
                    'Daily Conversation';
                  return (
                    <div
                      key={conv.id}
                      onClick={() => handleSelectOldConversation(conv.id)}
                      className={`p-3.5 border transition-all cursor-pointer flex items-start justify-between gap-3 ${
                        isSelected
                          ? 'bg-[var(--bg-subtle)] border-[var(--learner-primary-action)] ring-1 ring-[var(--learner-primary-action)]'
                          : 'bg-[var(--bg-app)] border-[var(--border-strong)] hover:bg-[var(--color-primary-50)]'
                      }`}
                      style={{ borderRadius: 'var(--radius-sm)' }}
                    >
                      <div className="min-w-0 flex-1 space-y-1">
                        <div className="flex flex-wrap items-center gap-1.5">
                          <span className="px-2 py-0.5 text-[10px] font-bold bg-[var(--learner-primary-action)] text-[var(--text-on-primary)] rounded">
                            {conv.cefrLevel}
                          </span>
                          <span className="px-2 py-0.5 text-[10px] font-semibold bg-[var(--color-primary-50)] text-[var(--learner-mint-text)] border border-[var(--learner-mint-border)] rounded">
                            {modeLabel}
                          </span>
                        </div>
                        <div className="text-xs font-bold text-[var(--text-primary)] truncate">
                          {conv.title}
                        </div>
                        {conv.topic && (
                          <div className="text-[11px] font-medium text-[var(--learner-mint-text)] truncate">
                            📌 Topic: {conv.topic}
                          </div>
                        )}
                        <p className="text-xs text-[var(--text-secondary)] line-clamp-2">
                          {conv.lastMessagePreview}
                        </p>
                        <div className="text-[11px] text-[var(--text-muted)] flex items-center gap-2">
                          <span>💬 {conv.messageCount} tin nhắn</span>
                          <span>·</span>
                          <span>🎙️ {conv.voiceName} ({conv.speechSpeed}x)</span>
                        </div>
                      </div>
                      <button
                        type="button"
                        onClick={(e) => {
                          e.stopPropagation();
                          deleteConversation(conv.id);
                        }}
                        title="Xóa hội thoại"
                        className="text-xs text-[var(--text-muted)] hover:text-red-600 px-1.5 py-1"
                      >
                        🗑️
                      </button>
                    </div>
                  );
                })
              )}
            </div>

            <div className="pt-3 border-t border-[var(--border-subtle)] text-xs text-[var(--text-secondary)]">
              Khi chọn lại cuộc hội thoại cũ, hệ thống tự động khôi phục cả <strong>Mode</strong>, <strong>Topic</strong>, <strong>System Prompt riêng</strong> và lịch sử tin nhắn để AI bắt nhịp ngay.
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
