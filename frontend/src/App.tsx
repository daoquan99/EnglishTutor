import React, { useState } from 'react';
import { CefrLevel } from './types/live';
import { useThreeHubLiveSession } from './hooks/useThreeHubLiveSession';

const CEFR_LEVELS: { level: CefrLevel; label: string; desc: string }[] = [
  { level: 'A1', label: 'A1 · Beginner', desc: 'Câu ngắn, từ vựng cơ bản, tốc độ chậm' },
  { level: 'A2', label: 'A2 · Elementary', desc: 'Giao tiếp hàng ngày, thì cơ bản' },
  { level: 'B1', label: 'B1 · Intermediate', desc: 'Phản xạ tự nhiên, phrasal verbs thông dụng' },
  { level: 'B2', label: 'B2 · Upper-Int', desc: 'Tranh luận mạch lạc, cấu trúc câu phức' },
  { level: 'C1', label: 'C1 · Advanced', desc: 'Tốc độ bản xứ, collocations & idioms nâng cao' },
  { level: 'C2', label: 'C2 · Mastery', desc: 'Bậc thầy ngôn ngữ, sắc thái học thuật & chuyên sâu' },
];

export default function App() {
  const [apiKey, setApiKey] = useState('');
  const [cefrLevel, setCefrLevel] = useState<CefrLevel>('B1');
  const [extendedThinking, setExtendedThinking] = useState(false);

  const {
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
  } = useThreeHubLiveSession();

  return (
    <div className="learnerTheme min-h-screen p-6 md:p-8">
      <div className="max-w-7xl mx-auto space-y-6">
        {/* Top Header */}
        <header
          className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4 p-6 bg-[var(--bg-surface)] border border-[var(--border-strong)]"
          style={{ borderRadius: 'var(--radius-xl)', boxShadow: 'var(--shadow-md)' }}
        >
          <div>
            <div className="flex items-center gap-3">
              <span
                className="px-3 py-1 text-xs font-semibold uppercase tracking-wider bg-[var(--bg-subtle)] text-[var(--learner-mint-text)] border border-[var(--learner-mint-border)]"
                style={{ borderRadius: 'var(--radius-full)' }}
              >
                Google AI Studio · 3 SignalR Hubs
              </span>
              <span className="text-xs text-[var(--text-secondary)]">
                FE: React + Vite · BE: .NET 10
              </span>
            </div>
            <h1
              className="mt-2 text-2xl md:text-3xl font-bold text-[var(--text-primary)]"
              style={{ fontFamily: 'var(--font-family-display)' }}
            >
              LingoMint Live English Teacher
            </h1>
            <p className="text-sm text-[var(--text-secondary)]">
              Luyện nói trực tiếp với Gemini 3.8 Live · Phụ đề song ngữ Gemini 3.5 Transcribe Live &amp; Live Translate
            </p>
          </div>

          {/* 3 SignalR Hub Status Badges */}
          <div className="flex flex-wrap items-center gap-2">
            {[
              { name: 'TutorHub (3.8 Live)', state: hubStatuses.tutor },
              { name: 'TranscribeHub (3.5 Transcribe)', state: hubStatuses.transcribe },
              { name: 'TranslateHub (3.5 Translate VI)', state: hubStatuses.translate },
            ].map((h) => (
              <div
                key={h.name}
                className="px-3 py-1.5 text-xs font-medium border bg-[var(--color-primary-50)] border-[var(--learner-mint-border)] flex items-center gap-2"
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <span
                  className={`w-2 h-2 rounded-full ${
                    h.state === 'Connected' ? 'bg-[#38a385]' : 'bg-amber-500'
                  }`}
                />
                <span>{h.name}</span>
              </div>
            ))}
          </div>
        </header>

        {errorMessage && (
          <div
            className="p-4 text-sm bg-red-50 border border-red-200 text-red-800"
            style={{ borderRadius: 'var(--radius-md)' }}
          >
            <strong>Lỗi kết nối:</strong> {errorMessage}
          </div>
        )}

        {/* Configuration Bar: API Key + CEFR Level A1-C2 + Extended Thinking Mode */}
        <section
          className="p-6 bg-[var(--bg-surface)] border border-[var(--border-strong)] space-y-5"
          style={{ borderRadius: 'var(--radius-lg)', boxShadow: 'var(--shadow-sm)' }}
        >
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-4 items-end">
            <div className="lg:col-span-5">
              <label className="block text-xs font-semibold uppercase tracking-wider text-[var(--text-secondary)] mb-2">
                Google AI Studio API Key (aistudio.google.com)
              </label>
              <input
                type="password"
                value={apiKey}
                onChange={(e) => setApiKey(e.target.value)}
                placeholder="AIzaSy... (nhập API key từ Google AI Studio)"
                className="w-full px-4 py-2.5 text-sm bg-[var(--bg-app)] border border-[var(--border-strong)] focus:outline-none"
                style={{ borderRadius: 'var(--radius-sm)' }}
              />
            </div>

            {/* Extended Thinking Toggle */}
            <div className="lg:col-span-4">
              <label className="block text-xs font-semibold uppercase tracking-wider text-[var(--text-secondary)] mb-2">
                Chế độ suy luận (Model Voice)
              </label>
              <button
                type="button"
                onClick={() => setExtendedThinking((v) => !v)}
                className={`w-full px-4 py-2.5 text-sm font-medium border flex items-center justify-between transition-all cursor-pointer ${
                  extendedThinking
                    ? 'bg-[var(--bg-subtle)] border-[var(--learner-accent)] text-[var(--learner-mint-text)]'
                    : 'bg-[var(--bg-app)] border-[var(--border-strong)] text-[var(--text-primary)]'
                }`}
                style={{ borderRadius: 'var(--radius-sm)' }}
              >
                <span>
                  {extendedThinking
                    ? 'Gemini 3.8 Live Extended Thinking'
                    : 'Gemini 3.8 Live (Standard Low-Latency)'}
                </span>
                <span
                  className={`px-2 py-0.5 text-xs font-bold rounded-full ${
                    extendedThinking
                      ? 'bg-[var(--learner-primary-action)] text-[var(--text-on-primary)]'
                      : 'bg-gray-200 text-gray-700'
                  }`}
                >
                  {extendedThinking ? 'THINKING ON' : 'OFF'}
                </span>
              </button>
            </div>

            {/* Connect / Mic Action Buttons */}
            <div className="lg:col-span-3 flex gap-2">
              {!isConnected ? (
                <button
                  type="button"
                  onClick={() => connectAllThreeHubs(apiKey, cefrLevel, extendedThinking)}
                  disabled={!apiKey.trim()}
                  className="w-full px-5 py-2.5 text-sm font-semibold text-[var(--text-on-primary)] bg-[var(--learner-primary-action)] hover:bg-[var(--learner-primary-action-hover)] disabled:opacity-50 cursor-pointer"
                  style={{ borderRadius: 'var(--radius-sm)', boxShadow: 'var(--shadow-sm)' }}
                >
                  Kết nối 3 Hubs
                </button>
              ) : (
                <>
                  <button
                    type="button"
                    onClick={isRecording ? stopMicrophone : startMicrophone}
                    className={`flex-1 px-4 py-2.5 text-sm font-semibold text-white cursor-pointer ${
                      isRecording ? 'bg-red-600 hover:bg-red-700' : 'bg-[#38a385] hover:bg-[#2e886e]'
                    }`}
                    style={{ borderRadius: 'var(--radius-sm)' }}
                  >
                    {isRecording ? 'Dừng nói' : 'Bắt đầu nói (Mic)'}
                  </button>
                  <button
                    type="button"
                    onClick={disconnectAll}
                    className="px-3 py-2.5 text-sm font-medium border border-[var(--border-strong)] text-[var(--text-secondary)] cursor-pointer"
                    style={{ borderRadius: 'var(--radius-sm)' }}
                  >
                    Ngắt
                  </button>
                </>
              )}
            </div>
          </div>

          {/* CEFR Level Selector A1 -> C2 */}
          <div>
            <span className="block text-xs font-semibold uppercase tracking-wider text-[var(--text-secondary)] mb-2">
              Chọn trình độ tiếng Anh hiện tại (CEFR Level: {cefrLevel})
            </span>
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-2.5">
              {CEFR_LEVELS.map((item) => {
                const active = cefrLevel === item.level;
                return (
                  <button
                    key={item.level}
                    type="button"
                    onClick={() => setCefrLevel(item.level)}
                    className={`p-3 text-left border transition-all cursor-pointer ${
                      active
                        ? 'bg-[var(--bg-subtle)] border-[var(--learner-primary-action)] ring-2 ring-[var(--learner-primary-action)]'
                        : 'bg-[var(--bg-app)] border-[var(--border-strong)] hover:bg-[var(--color-primary-50)]'
                    }`}
                    style={{ borderRadius: 'var(--radius-sm)' }}
                  >
                    <div className="font-bold text-sm text-[var(--text-primary)]">{item.label}</div>
                    <div className="text-xs text-[var(--text-secondary)] mt-0.5 line-clamp-2">{item.desc}</div>
                  </button>
                );
              })}
            </div>
          </div>
        </section>

        {/* Main Workspace: Left Dual-Line Live Stream (Transcribe + Translate) | Right Teacher Corrections */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          {/* Left 7 Cols: Live Conversation Stream */}
          <div
            className="lg:col-span-7 p-6 bg-[var(--bg-surface)] border border-[var(--border-strong)] flex flex-col justify-between min-h-[520px]"
            style={{ borderRadius: 'var(--radius-lg)', boxShadow: 'var(--shadow-md)' }}
          >
            <div className="flex items-center justify-between pb-4 border-b border-[var(--border-subtle)]">
              <div>
                <h2 className="text-lg font-bold text-[var(--text-primary)]">
                  Hội Thoại Trực Tiếp &amp; Phụ Đề Song Ngữ
                </h2>
                <p className="text-xs text-[var(--text-secondary)]">
                  Dòng 1 (chữ lớn): Gemini 3.5 Transcribe Live · Dòng 2 (chữ nghiêng nhỏ, chỉ lấy text): Gemini 3.5 Live Translate
                </p>
              </div>
              {extendedThinking && (
                <span
                  className="px-3 py-1 text-xs font-semibold bg-[var(--bg-subtle)] text-[var(--learner-mint-text)]"
                  style={{ borderRadius: 'var(--radius-full)' }}
                >
                  Thinking Status: {interactionStatus}
                </span>
              )}
            </div>

            <div className="flex-1 overflow-y-auto py-4 space-y-4">
              {turns.length === 0 ? (
                <div className="h-full flex items-center justify-center text-sm text-[var(--text-muted)]">
                  Nhấn &ldquo;Kết nối 3 Hubs&rdquo; và bật Mic để bắt đầu hội thoại trực tiếp với giáo viên AI...
                </div>
              ) : (
                turns.map((turn) => (
                  <div
                    key={`${turn.speaker}-${turn.turnId}`}
                    className={`p-4 border ${
                      turn.speaker === 'user'
                        ? 'ml-8 bg-[var(--bg-subtle)] border-[var(--learner-mint-border)]'
                        : 'mr-8 bg-[var(--bg-app)] border-[var(--border-strong)]'
                    }`}
                    style={{ borderRadius: 'var(--radius-md)' }}
                  >
                    <div className="flex items-center justify-between text-xs font-semibold text-[var(--text-secondary)] mb-1.5">
                      <span>
                        {turn.speaker === 'user'
                          ? `Bạn (Level ${cefrLevel})`
                          : `AI English Teacher (${extendedThinking ? 'Gemini 3.8 Live Extended Thinking' : 'Gemini 3.8 Live'})`}
                      </span>
                      <span>{turn.timestamp}</span>
                    </div>

                    {/* DÒNG 1: Gemini 3.5 Transcribe Live - Chữ to hơn (18px / --font-size-study-body) */}
                    <p
                      className="font-semibold text-[var(--text-primary)] leading-snug"
                      style={{ fontSize: 'var(--font-size-study-body)' }}
                    >
                      {turn.transcriptLine1 || '...'}
                    </p>

                    {/* DÒNG 2: Gemini 3.5 Live Translate - Chữ nghiêng nhỏ hơn, chỉ lấy text tiếng Việt */}
                    <p className="mt-1.5 text-sm italic text-[var(--text-secondary)] leading-relaxed">
                      {turn.translationLine2 || 'Đang dịch sang tiếng Việt...'}
                    </p>
                  </div>
                ))
              )}
            </div>
          </div>

          {/* Right 5 Cols: Real-time Spelling, Grammar & Pronunciation Corrections */}
          <div
            className="lg:col-span-5 p-6 bg-[var(--bg-surface)] border border-[var(--border-strong)] flex flex-col min-h-[520px]"
            style={{ borderRadius: 'var(--radius-lg)', boxShadow: 'var(--shadow-md)' }}
          >
            <div className="pb-4 border-b border-[var(--border-subtle)]">
              <h2 className="text-lg font-bold text-[var(--text-primary)]">
                Sửa Lỗi Chính Tả, Từ Vựng &amp; Ngữ Pháp
              </h2>
              <p className="text-xs text-[var(--text-secondary)]">
                Gemini 3.8 Live phát hiện lỗi khi bạn nói và gửi thẻ sửa lỗi tức thì qua non-blocking tool call
              </p>
            </div>

            <div className="flex-1 overflow-y-auto py-4 space-y-3">
              {corrections.length === 0 ? (
                <div className="h-full flex items-center justify-center text-sm text-[var(--text-muted)]">
                  Các lỗi chính tả, từ vựng và ngữ pháp khi bạn nói sẽ hiển thị tức thì tại đây...
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
          </div>
        </div>
      </div>
    </div>
  );
}
