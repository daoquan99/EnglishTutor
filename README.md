# LingoMint Live — AI English Teacher (Gemini 3.8 Live & Gemini 3.5 Audio)

Ứng dụng web luyện nói tiếng Anh trực tiếp 1-1 với giáo viên AI thời gian thực, sử dụng API Key của **Google AI Studio** (`https://aistudio.google.com/`), kết hợp **React + Vite + Tailwind CSS** (Frontend) và **ASP.NET Core .NET 10 SignalR** (Backend).

---

## 1. Kiến trúc 3 SignalR Hubs & Các Model Sử Dụng

Hệ thống tách biệt **3 kết nối SignalR Hub riêng biệt** từ Frontend (`React + Vite`) lên Backend (`.NET 10`), và từ Backend mở các kết nối WebSocket hai chiều (`BidiGenerateContent`) tới Google AI Studio qua API Key:

1. **`/hubs/tutor` (`TutorConversationHub`)**:
   - **Chế độ thường**: Dùng model `models/gemini-3.8-live` để trò chuyện giọng nói trực tiếp (`AUDIO` 24kHz) với độ trễ cực thấp.
   - **Chế độ Extended Thinking**: Khi người dùng bật nút **Thinking Mode**, hệ thống chuyển sang model `models/gemini-3.8-live-extended-thinking` kèm cấu hình `thinkingConfig: { thinkingLevel: "medium" }` và theo dõi trạng thái `interactionStatus` (`IN_PROGRESS` / `IDLE`).
   - **Chọn trình độ CEFR (`A1` → `C2`)**: Tùy biến `systemInstruction` theo đúng cấp độ từ `A1`, `A2`, `B1`, `B2`, `C1` đến `C2`.
   - **Sửa lỗi chính tả, từ vựng & ngữ pháp tức thì**: Tích hợp hàm bất đồng bộ `report_language_correction` (`behavior: "NON_BLOCKING"`) giúp giáo viên AI vừa nói chuyện tự nhiên vừa đẩy thẻ sửa lỗi chi tiết (kèm giải thích tiếng Việt và mẹo theo level CEFR) xuống giao diện.

2. **`/hubs/transcribe` (`TranscribeLiveHub`)**:
   - Kết nối Hub riêng biệt sử dụng model `models/gemini-3.5-transcribe-live` (`responseModalities: ["TEXT"]`, `inputAudioTranscription: { mode: "smart" }`).
   - Nhận luồng âm thanh 16kHz PCM của cả **User** và **AI** để hiển thị văn bản tiếng Anh thời gian thực ở **Dòng 1 (cỡ chữ lớn `18px`, in đậm rõ nét)**.

3. **`/hubs/translate` (`TranslateLiveHub`)**:
   - Kết nối Hub riêng biệt sử dụng model `models/gemini-3.5-live-translate-preview` (`translationConfig: { targetLanguageCode: "vi", echoTargetLanguage: true }`).
   - **Chỉ lấy Text, không lấy Audio**: Backend chỉ trích xuất `serverContent.outputTranscription.text` (bản dịch tiếng Việt) và bỏ qua hoàn toàn dữ liệu audio dịch để đẩy xuống Frontend hiển thị ở **Dòng 2 (chữ nghiêng nhỏ hơn)**.

---

## 2. Cấu trúc thư mục

```text
english-live-tutor/
├── README.md
├── .gitignore
├── backend/
│   ├── EnglishLiveTutor.Api.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Models/
│   │   └── LiveSessionDtos.cs
│   ├── Services/
│   │   ├── CefrPromptBuilder.cs
│   │   ├── PcmAudioResampler.cs
│   │   ├── GeminiLiveSessionBridge.cs
│   │   └── GeminiLiveConnectionManager.cs
│   └── Hubs/
│       ├── TutorConversationHub.cs
│       ├── TranscribeLiveHub.cs
│       └── TranslateLiveHub.cs
└── frontend/
    ├── package.json
    ├── vite.config.ts
    ├── tsconfig.json
    ├── index.html
    ├── .env.example
    ├── public/
    │   └── pcm16k-capture-worklet.js
    └── src/
        ├── main.tsx
        ├── index.css
        ├── vite-env.d.ts
        ├── types/
        │   └── live.ts
        ├── hooks/
        │   └── useThreeHubLiveSession.ts
        └── App.tsx
```

---

## 3. Hướng dẫn cài đặt & chạy dự án

### Yêu cầu hệ thống
- **.NET 10 SDK** (`dotnet --version` >= 10.0)
- **Node.js** (>= 20.x) & `npm`
- **Google AI Studio API Key**: Lấy tại [https://aistudio.google.com/](https://aistudio.google.com/)

### Bước 1: Chạy Backend (.NET 10)
```bash
cd backend
dotnet restore
dotnet run --urls "http://localhost:5000"
```
Backend sẽ khởi chạy tại `http://localhost:5000` với 3 SignalR Hubs:
- `http://localhost:5000/hubs/tutor`
- `http://localhost:5000/hubs/transcribe`
- `http://localhost:5000/hubs/translate`
- Healthcheck: `http://localhost:5000/api/health`

### Bước 2: Chạy Frontend (React + Vite + Tailwind CSS)
Mở một cửa sổ terminal mới:
```bash
cd frontend
npm install
npm run dev
```
Truy cập `http://localhost:5173`:
1. Dán **API Key** từ Google AI Studio vào ô cấu hình.
2. Chọn trình độ hiện tại của bạn (**A1 → C2**).
3. Bật hoặc tắt chế độ **Extended Thinking** (`Gemini 3.8 Live Extended Thinking`).
4. Nhấn **Kết nối 3 Hubs**, sau đó nhấn **Bắt đầu nói (Mic)** để trò chuyện trực tiếp với giáo viên AI!
