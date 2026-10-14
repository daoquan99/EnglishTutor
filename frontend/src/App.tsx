import React, { useEffect, useRef, useState } from 'react';
import { CefrLevel, TeacherVoiceName } from './types/live';
import { useThreeHubLiveSession } from './hooks/useThreeHubLiveSession';

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

export default function App() {
  const [apiKey, setApiKey] = useState(() => localStorage.getItem('lingomint_api_key') || '');
  const [showKey, setShowKey] = useState(false);
  const [cefrLevel, setCefrLevel] = useState<CefrLevel>('A2');
  const [extendedThinking, setExtendedThinking] = useState(false);
  const [voiceName, setVoiceName] = useState<TeacherVoiceName>('Leda');
  const [speechSpeed, setSpeechSpeed] = useState<number>(0.9);

  // Requirement 1: No Navbar; Left Menu toggles between Expanded and Icon-Only Rail
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
    }
    setIsHistoryDrawerOpen(false);
  };

  const handleCreateNewConversation = async () => {
    if (isConnected) {
      await disconnectAll();
    }
    await createNewConversation(cefrLevel, extendedThinking, voiceName, speechSpeed);
    setIsHistoryDrawerOpen(false);
  };

  const expandAndFocusKey = () => {
    setIsSidebarExpanded(true);
    setTimeout(() => apiKeyInputRef.current?.focus(), 80);
  };

  const filteredConversations = conversations.filter(
    (c) =>
      c.title.toLowerCase().includes(historySearch.toLowerCase()) ||
      c.lastMessagePreview.toLowerCase().includes(historySearch.toLowerCase())
  );

  return (
    <div className="learnerTheme min-h-screen flex flex-col lg:flex-row">
      {/* =====================================================================
          LEFT MENU: Expanded Full Setup OR Collapsed Icon-Only Rail (No Top Navbar!)
      ====================================================================== */}
      <aside
        className={`bg-[var(--bg-surface)] border-b lg:border-b-0 lg:border-r border-[var(--border-strong)] flex flex-col justify-between transition-all duration-200 shrink-0 lg:h-screen lg:sticky lg:top-0 ${
          isSidebarExpanded ? 'w-full lg:w-84 xl:w-90 p-4' : 'w-full lg:w-20 p-3 items-center'
        }`}
        style={{ boxShadow: 'var(--shadow-sm)' }}
      >
        {/* TOP BRAND LOGO + ICON TOGGLE BUTTON */}
        <div
          className={`w-full flex items-center ${
            isSidebarExpanded ? 'justify-between pb-3.5 border-b border-[var(--border-subtle)]' : 'flex-col gap-3 pb-3 border-b border-[var(--border-subtle)]'
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

          {/* Button Icon Toggle for Left Menu */}
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
            COLLAPSED ICON-ONLY RAIL VIEW ("ẩn thì left menu chỉ show icon thôi")
        ================================================================== */}
        {!isSidebarExpanded ? (
          <div className="flex-1 flex flex-row lg:flex-col items-center justify-between py-3 gap-2.5 w-full overflow-x-auto lg:overflow-x-visible">
            <div className="flex flex-row lg:flex-col items-center gap-2.5">
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

              {/* 4. API Key Setup Icon Button */}
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

              {/* 5. Teacher Voice & Speed Icon Button */}
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

              {/* 6. Extended Thinking Toggle Icon Button */}
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

              {/* 7. CEFR Level Icon Button */}
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

            {/* Bottom 3 Hub Status Dots in Icon Rail */}
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
          <div className="flex-1 overflow-y-auto py-3 space-y-4 pr-1">
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

            {/* 1. Google AI Studio API Key */}
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

            {/* 2. Teacher Voice & Speech Speed */}
            <div className="space-y-3 pt-2 border-t border-[var(--border-subtle)]">
              <div>
                <label className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-1.5">
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
                <label className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-1.5">
                  <img src="/assets/icons/navigation/nav-settings.svg" alt="" className="w-4 h-4" />
                  <span>Tốc độ nói của Teacher ({speechSpeed}x)</span>
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

            {/* 3. Extended Thinking Toggle */}
            <div className="pt-2 border-t border-[var(--border-subtle)]">
              <label className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-1.5">
                <img src="/assets/icons/navigation/nav-reviews.svg" alt="" className="w-4 h-4" />
                <span>Chế độ suy luận (Model Voice)</span>
              </label>
              <button
                type="button"
                onClick={() => setExtendedThinking((v) => !v)}
                className={`w-full px-3 py-2.5 text-xs font-semibold border flex items-center justify-between transition-all cursor-pointer ${
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

            {/* 4. CEFR Level Selector A1 -> C2 */}
            <div className="pt-2 border-t border-[var(--border-subtle)]">
              <label className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-1.5">
                <img src="/assets/icons/navigation/nav-lessons.svg" alt="" className="w-4 h-4" />
                <span>Trình độ hiện tại (CEFR: {cefrLevel})</span>
              </label>
              <div className="grid grid-cols-2 gap-1.5">
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
                      <div className="font-bold text-xs text-[var(--text-primary)]">{item.label}</div>
                      <div className="text-[10px] text-[var(--text-secondary)] truncate">{item.desc}</div>
                    </button>
                  );
                })}
              </div>
            </div>

            {/* 5. 3 SignalR Hub Statuses & Connect Button */}
            <div className="pt-3 border-t border-[var(--border-subtle)] space-y-2.5">
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
            <div className="flex flex-wrap items-center justify-between gap-2 pb-4 border-b border-[var(--border-subtle)]">
              <div className="flex items-center gap-3">
                <img
                  src="/assets/mascot/cat/cat-avatar.svg"
                  alt="Teacher Avatar"
                  className="w-10 h-10 shrink-0"
                />
                <div>
                  <h2 className="text-lg font-bold text-[var(--text-primary)]">
                    Hội Thoại Trực Tiếp &amp; Phụ Đề Song Ngữ
                  </h2>
                  <p className="text-xs text-[var(--text-secondary)]">
                    {activeConversationTitle} · <strong>Dòng 1 (Chữ to)</strong>: Transcribe Live · <strong>Dòng 2 (Chữ nghiêng nhỏ)</strong>: Live Translate VI
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
                    Chào mừng bạn đến với phòng luyện nói 1-1!
                  </p>
                  <p className="text-xs max-w-md text-[var(--text-secondary)]">
                    Nhấn <strong>&ldquo;Kết nối 3 Hubs &amp; Bắt đầu&rdquo;</strong> ở menu bên trái rồi bật Mic để trò chuyện trực tiếp với giáo viên AI, hoặc mở <strong>Lịch sử</strong> để tiếp tục bài học trước.
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
                        {/* DÒNG 1: Gemini 3.5 Transcribe Live - Chữ to hơn */}
                        <p
                          className="font-semibold text-[var(--text-primary)] leading-snug"
                          style={{ fontSize: 'var(--font-size-study-body)' }}
                        >
                          {turn.transcriptLine1 || '...'}
                        </p>

                        {/* DÒNG 2: Gemini 3.5 Live Translate - Chữ nghiêng nhỏ hơn, khóa chặt theo đúng TurnId */}
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
                    Phân tích tự động sau mỗi lượt nói &amp; lưu vào SQL Server (<code>LingoMint</code>)
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
                    Khi bạn nói sai ngữ pháp, nhầm mạo từ (a/an), giới từ hoặc từ vựng, thẻ sửa lỗi chi tiết sẽ xuất hiện ngay tại đây.
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
      </main>

      {/* =====================================================================
          SEPARATE CONVERSATION HISTORY DRAWER (Separated from Left Setup Menu)
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
                  placeholder="Tìm kiếm cuộc hội thoại..."
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
                        <div className="flex items-center gap-2">
                          <span className="px-2 py-0.5 text-[10px] font-bold bg-[var(--learner-primary-action)] text-[var(--text-on-primary)] rounded">
                            {conv.cefrLevel}
                          </span>
                          <span className="text-xs font-bold text-[var(--text-primary)] truncate">
                            {conv.title}
                          </span>
                        </div>
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
              Khi chọn lại một cuộc hội thoại cũ và nhấn <strong>Kết nối 3 Hubs</strong>, hệ thống sẽ tự động nạp các tin nhắn cũ vào <code>Gemini 3.8 Live</code> để giáo viên AI bắt nhịp tiếp câu chuyện.
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
