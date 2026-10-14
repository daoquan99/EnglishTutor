# EnglishLearning — Live AI Teacher (19 Modes) + Daily Vocabulary Flashcards & Pronunciation Assessment (v8)

Giải pháp Full-Stack **React + Vite + Tailwind CSS (`.learnerTheme` + 23 Custom SVG Design System)** kết nối **ASP.NET Core .NET 10 + SignalR (3 Hubs độc lập) + Entity Framework Core (SQL Server `Database=LingoMint`)**.

## 1. Các tính năng giữ nguyên 100% (v1 -> v7)
- **3 SignalR Hubs thời gian thực**:
  - `/hubs/tutor`: `models/gemini-3.8-live` & `models/gemini-3.8-live-extended-thinking`
  - `/hubs/transcribe`: `models/gemini-3.5-transcribe-live` (Phụ đề tiếng Anh dòng 1 cỡ lớn)
  - `/hubs/translate`: `models/gemini-3.5-live-translate-preview` + REST Fallback (Dịch tiếng Việt dòng 2 chữ nghiêng)
- **19 Chế độ luyện nói & phỏng vấn (12 Interview Modes + 7 Communication Modes)**, **Topic (Chủ đề)** và **Optional Custom System Prompt** cho từng cuộc hội thoại.
- **Left Menu thu gọn dạng Icon-Only Rail** (không dùng Top Navbar) và **Ngăn kéo Lịch sử hội thoại (Conversation History Drawer)** độc lập.
- **Sửa lỗi Chính tả, Từ vựng & Ngữ pháp (GrammarCorrectionAnalyzerService)** tách biệt hoàn toàn ở cột phải.

## 2. Tính năng mới phát triển (v8 — Daily Vocabulary Flashcards & AI Pronunciation Assessment)
- **Rich Sense-Based Vocabulary Domain Model** (`backend/Data/VocabularyDomainEntities.cs`):
  - `Vocabularies`, `VocabularySenses` (hỗ trợ từ đa nghĩa như `"bank"` tài chính vs. `"bank"` bờ sông), `VocabularyPronunciations` (`UK` / `US` IPA, số âm tiết, trọng âm), `VocabularyRelations` (`Synonym` / `Antonym` theo từng Sense), `VocabularyWordFamilies`, `VocabularyCollocations`, `VocabularyExamples`, `VocabularyGrammarPatterns`.
- **AI Model Catalog & Capability Validation** (`AiModelCatalog`):
  - Seed đầy đủ 10 model tạo từ vựng / chấm điểm (`gemini-2.5-flash`, `gemini-2.5-flash-lite`, `gemini-3-flash-preview`, `gemini-3.1-flash-lite`, `gemini-3.5-flash-lite`, `gemini-3.6-flash`, `gemini-3.7-flash` [Deprecated -> routes to `gemini-3.8-flash`], `gemini-3.8-flash` [Default], `gemma-4-26b-a4b-it` [Text Only], `gemma-4-31b-it` [Text Only]) cùng 4 model Realtime Live.
  - Hai bộ chọn model độc lập trên Frontend (Vocabulary Generation Model & Pronunciation Assessment Model). Các dòng `Gemma 4` không hỗ trợ Audio Input nên bị vô hiệu hóa ở bộ chọn Chấm phát âm và được kiểm tra chặn ở tầng Backend.
- **Daily Vocabulary Learning & Spaced Repetition (SM-2)** (`VocabularyLearningService.cs`):
  - Tự động khởi tạo kế hoạch học hằng ngày theo múi giờ (`Asia/Ho_Chi_Minh`, mục tiêu mặc định `20 từ mới/ngày`), tự động chuyển tiếp từ chưa hoàn thành (`CarryOver`), nạp các từ đến hạn ôn tập (`Review`), lấy từ kho từ vựng có sẵn trước khi gọi AI tạo thêm từ mới.
  - Thuật toán lặp lại ngắt quãng **SM-2** với 4 mức đánh giá: `Again (+4h)`, `Hard (+1d)`, `Good (+3d)`, `Easy (+5d)`.
- **Pronunciation Scoring Rubric & Audio Assessment** (`PronunciationAssessmentService.cs`):
  - Seed rubric mặc định `ENGLISH_PRONUNCIATION_V1` với trọng số: **Accuracy (40%)**, **Clarity (30%)**, **Fluency (30%)**.
  - Hỗ trợ luyện phát âm cả **Từ đơn (`Word`)** và **Câu ví dụ (`Sentence`)**.
  - Tính điểm tổng có trọng số (`OverallScore = SUM(CriterionScore * CriterionWeight / 100)`) một cách tất định (deterministic) trên Backend và lưu snapshot tiêu chí vào `PronunciationCriterionScores`.
- **Unified Microphone Pipeline + Tích hợp Hub 2 & Hub 3 trong Flashcard**:
  - Thu âm giọng đọc của học viên để gửi chấm điểm phát âm đồng thời truyền luồng PCM 16kHz sang **Hub 2 (`gemini-3.5-transcribe-live`)** và tùy chọn bật/tắt **Hub 3 (`Enable Vietnamese Live Translation` — `gemini-3.5-live-translate-preview`, mặc định Tắt)**.
