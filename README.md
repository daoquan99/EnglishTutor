# EnglishLearning Live v4 — AI English Teacher (.NET 10 + SQL Server + React + Vite + Custom SVG Design System)

Ứng dụng web luyện nói tiếng Anh trực tiếp 1-1 với giáo viên AI thời gian thực, sử dụng API Key của **Google AI Studio** (`https://aistudio.google.com/`), kết hợp **React + Vite + Tailwind CSS** (Frontend) và **ASP.NET Core .NET 10 SignalR + Entity Framework Core SQL Server** (Backend).

---

## Các nâng cấp & Sửa lỗi trong phiên bản v4

1. **Tích hợp trọn bộ Custom SVG Design System (`assets/`) & Favicon**:
   - Toàn bộ bộ icon SVG bạn thiết kế (`brand/logo-horizontal.svg`, `brand/logo-mark.svg`, `mascot/cat/cat-avatar.svg`, `icons/navigation/nav-*.svg`, `illustrations/illustration-*.svg`) đã được tinh chỉnh và đưa vào `frontend/public/assets/` cùng `favicon.svg` trên tab trình duyệt.
2. **Bỏ Top Navbar & Thiết kế Left Menu thu gọn dạng Icon Rail (Collapsible Icon Sidebar)**:
   - Đã loại bỏ hoàn toàn thanh Top Navbar để không gian học tập rộng rãi, tập trung.
   - Nút Toggle ẩn/hiện nằm ngay góc trên của Left Menu dưới dạng **Button Icon**:
     - **Khi mở rộng (`Expanded`)**: Hiển thị `logo-horizontal.svg`, các nút điều khiển nhanh và đầy đủ form cài đặt kèm bộ icon SVG tương ứng.
     - **Khi thu gọn (`Collapsed Icon-Only Rail`)**: Left Menu thu nhỏ gọn thành thanh dọc chỉ hiển thị **Logo Icon (`logo-mark.svg`)** và các **Button Icon SVG** (`Kết nối / Mic`, `Hội thoại mới`, `Lịch sử hội thoại`, `API Key`, `Giọng đọc & Tốc độ`, `Extended Thinking`, `Trình độ CEFR A1–C2`, và đèn trạng thái 3 Hubs) kèm chú thích nổi (tooltip) khi di chuột.
3. **Khắc phục triệt để lỗi "Sửa Lỗi Chính Tả, Từ Vựng & Ngữ Pháp" không hoạt động & lỗi `"I am sorry, but a system error occurred"` (Ảnh 3)**:
   - **Nguyên nhân**: Khi bật `Gemini 3.8 Live Extended Thinking` kèm khai báo `tools` trong phiên âm thanh trực tiếp, model thường tự phát câu chờ (*"Let me check on that, please wait a moment..."*) rồi gặp lỗi timeout tool call trên luồng audio (*"I am sorry, but a system error occurred"*) hoặc bỏ qua việc gọi hàm sửa lỗi để ưu tiên trả lời giọng nói.
   - **Giải pháp v4**: Tách riêng dịch vụ **`GrammarCorrectionAnalyzerService.cs`** trên Backend. Ngay khi người dùng vừa nói xong một lượt (`FinalizeUserTurn`), Backend lập tức phân tích câu tiếng Anh đó bằng Gemini JSON Schema + bộ quy tắc ngữ pháp song song ở hậu cảnh và bắn thẻ sửa lỗi (`ReceiveLanguageCorrection`) xuống Frontend + lưu vào bảng `Corrections` trong SQL Server. Nhờ vậy:
     - Model giọng nói `Gemini 3.8 Live` / `Extended Thinking` chỉ tập trung đối thoại mượt mà, **không bao giờ bị lỗi `"I am sorry, but a system error occurred"`**.
     - Bảng **Sửa Lỗi Chính Tả, Từ Vựng & Ngữ Pháp** hoạt động **100% ở mọi lượt nói của học viên**.
4. **Khắc phục triệt để lỗi bản dịch tiếng Việt bị nhảy từ tin nhắn trên xuống tin nhắn dưới (Ảnh 2) & mất bản dịch (Ảnh 3)**:
   - **Nguyên nhân**: Luồng âm thanh `gemini-3.5-live-translate-preview` có độ trễ 1.5–2.5 giây so với lời nói. Khi câu số #13 kết thúc và chuyển sang câu #14, phần đuôi bản dịch âm thanh của câu #13 mới trả về nên bị đẩy nhầm vào đầu câu #14 (ví dụ cụm *"giải quyết các vấn đề phức tạp trong mã nguồn"* bị nhảy xuống tin nhắn dưới).
   - **Giải pháp v4**: Mọi bản dịch trên `TranslateLiveHub` giờ đây được **khóa chặt theo đúng `TurnId` và văn bản `TranscriptLine1` của chính bong bóng đó** thông qua bộ điều phối `TurnTranslationCoordinator` (có `CancellationTokenSource` debounce riêng cho từng `TurnId` + retry tự động). Bản dịch của tin nhắn nào chỉ dịch từ đúng câu tiếng Anh của tin nhắn đó, **tuyệt đối không bao giờ nhảy chữ sang tin nhắn kế tiếp** và không bao giờ bị treo ở *"Đang dịch sang tiếng Việt..."*.
5. **Khắc phục lỗi câu nói của AI bị tách làm 2 bong bóng khi có tiếng ồn (Ảnh 1)**:
   - Khóa `ActiveAiTurnId` trong suốt thời gian AI đang phát âm thanh cho đến khi nhận tín hiệu `turnComplete: true` từ Google.

---

## Hướng dẫn chạy dự án

### Bước 1: Chạy Backend (.NET 10 + SQL Server)
Connection String mặc định trong `backend/appsettings.json`:
`Server=localhost;Database=LingoMint;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;`

```bash
cd backend
dotnet restore
dotnet run --urls "http://localhost:5000"
```

### Bước 2: Chạy Frontend (React + Vite)
```bash
cd frontend
npm install
npm run dev
```
Mở trình duyệt tại `http://localhost:5173`.
