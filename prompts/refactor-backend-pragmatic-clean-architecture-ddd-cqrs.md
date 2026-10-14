# Refactor backend: Pragmatic Clean Architecture + DDD + CQRS

Bạn đang làm việc tại `D:\Projects\EnglishTutor\backend`.

Hãy refactor backend hiện tại theo hướng **Pragmatic Clean Architecture + Domain Driven Design + CQRS**. Thực hiện trên mã nguồn thực tế, giữ nguyên chức năng và các thay đổi chưa commit.

## 1. Khảo sát trước khi sửa

- Đọc `AGENTS.md` và hướng dẫn áp dụng trong repository.
- Kiểm tra `git status`, project references, DI và các luồng xử lý hiện tại.
- Backend hiện là một project `EnglishLiveTutor.Api.csproj`, target `net10.0`.
- Các thành phần cần khảo sát:
  - `Program.cs`: cấu hình, DI, migration, seeding và REST endpoints.
  - `Data/AppDbContext.cs`.
  - `Data/Entities.cs`.
  - `Data/VocabularyDomainEntities.cs`.
  - `Data/DatabaseSeeder.cs`.
  - `Services/ConversationRepository.cs`.
  - `Services/VocabularyLearningService.cs`.
  - `Services/PronunciationAssessmentService.cs`.
  - `Services/GeminiLiveConnectionManager.cs`.
  - `Services/GeminiLiveSessionBridge.cs`.
  - `Services/CefrPromptBuilder.cs`.
  - Các service dịch, sửa ngữ pháp, xử lý PCM và deduplicate text.
  - `Models`, `Hubs` và `Migrations`.

Không reset, checkout hoặc ghi đè các thay đổi có sẵn. Dùng trạng thái working tree hiện tại làm baseline.

## 2. Kiến trúc đích

Tổ chức thành bốn project:

```text
backend/
  EnglishLiveTutor.Api.csproj
  Program.cs
  Endpoints/
  Hubs/
  src/
    EnglishLiveTutor.Domain/
    EnglishLiveTutor.Application/
    EnglishLiveTutor.Infrastructure/
  docs/
    architecture.md
```

Giữ đường dẫn project API hiện tại để hạn chế ảnh hưởng đến lệnh chạy và tooling.

### Dependency

- Domain không phụ thuộc các project còn lại.
- Application phụ thuộc Domain.
- Infrastructure phụ thuộc Application và Domain.
- Api phụ thuộc Application; tham chiếu Infrastructure để cấu hình DI.

Cấu hình Compile exclusions trong API để không biên dịch lại source, obj và bin của các project con.

### Domain

- Chứa entity, aggregate, value object và quy tắc nghiệp vụ.
- Không phụ thuộc EF Core, ASP.NET Core, SignalR hoặc HttpClient.
- Mapping database nằm trong Infrastructure.
- Không tạo base class, generic repository hoặc domain event nếu chưa có nhu cầu thực tế.

### Application

- Chứa command/query, handler, DTO và các interface cho persistence, AI và realtime.
- Tổ chức theo feature/use case.
- Handler điều phối nghiệp vụ; không chứa HTTP request đến Google hoặc phụ thuộc hub cụ thể.
- Có thể dùng EF Core qua abstraction do Application sở hữu nếu giúp giữ truy vấn rõ ràng và tránh repository phức tạp. Ghi rõ lựa chọn pragmatic này trong tài liệu.
- Không tham chiếu `AppDbContext` hoặc implementation trong Infrastructure.

### Infrastructure

- Chứa AppDbContext, mappings, migrations, seeding, persistence implementation và Google AI integrations.
- Chứa WebSocket bridge, connection lifecycle và các thành phần kỹ thuật xử lý audio.
- Không tham chiếu project Api.

### Api

- Là composition root.
- Endpoint và hub làm nhiệm vụ nhận request, gọi use case, trả response và chuyển tiếp sự kiện.
- `Program.cs` chỉ giữ bootstrap, middleware và mapping.

## 3. Ranh giới nghiệp vụ và DDD

Dựa trên mã hiện tại để tổ chức các feature:

- Conversations.
- Vocabulary.
- Learning.
- Pronunciation.
- AI model catalog/integration.

Các aggregate cần xem xét:

- Conversation và lịch sử message/correction.
- Vocabulary và các sense, pronunciation, word family.
- DailyVocabularyPlan và các item.
- UserVocabularyProgress.
- ScoringRubric và criteria.
- PronunciationAssessment và criterion score snapshots.

Không coi tất cả bảng là aggregate root. Xác định ranh giới nhất quán và quyền sở hữu dữ liệu trước khi tách.

Đưa các quy tắc hiện có về Domain:

- Chuẩn hóa vocabulary.
- Cập nhật tiến độ, mastery, ease factor và lịch ôn tập.
- Hoàn thành item và kế hoạch học.
- Kiểm tra trọng số rubric.
- Tính weighted score và overall score.
- Quy tắc cập nhật conversation nếu phù hợp.

Bảo toàn công thức, rounding, ngưỡng điểm, thời gian ôn tập và fallback hiện tại. Không tự đổi thuật toán trong đợt refactor.

Không thêm validation mới khiến request đang hợp lệ bị từ chối mà không ghi rõ đây là thay đổi hành vi.

## 4. CQRS theo use case thực tế

Tách các service lớn thành handler riêng; không chỉ tạo handler bọc quanh nguyên service cũ.

### Commands

- CreateConversation.
- UpdateConversationSettings.
- DeleteConversation.
- GenerateVocabulary.
- InitializeDailyVocabularyPlan.
- SubmitVocabularyReview.
- AssessPronunciation.
- UpdateScoringRubric.

### Queries

- ListConversations.
- GetConversationHistory.
- GetVocabularyFlashcard.
- GetVocabularyGenerationModels.
- GetLearningProgress.
- GetLearningHistory.
- GetPronunciationModels.
- GetActiveScoringRubric.
- GetPronunciationHistory.

Có thể dùng `ICommand<TResult>`, `IQuery<TResult>` và handler interfaces đơn giản, inject trực tiếp qua DI. Không thêm MediatR hoặc framework khác chỉ để triển khai CQRS.

Lưu ý: `GET /api/learning/flashcards/today` hiện gọi `GetOrCreateDailyPlanAsync` và có thể ghi dữ liệu. Giữ route tương thích, nhưng phân loại use case này theo side effect thực tế; không gọi nó là query thuần đọc.

Tách shared projection/helper khi nhiều handler cần dùng. Tránh sao chép truy vấn flashcard và thuật toán giữa các handler.

## 5. Tách external integrations và realtime

- Tách Google AI HTTP calls, provider response parsing và WebSocket protocol khỏi nghiệp vụ học tập.
- Application định nghĩa interface phù hợp cho vocabulary generation và pronunciation assessment.
- Bảo toàn prompt version, model routing, capability checks, audit và rubric snapshots.
- Không sửa danh sách model hoặc cấu hình provider trong đợt refactor.

`GeminiLiveConnectionManager` hiện phụ thuộc trực tiếp `IHubContext` của ba hub. Loại bỏ dependency này bằng interface phát sự kiện realtime do Application sở hữu.

Api triển khai adapter SignalR:

- Giữ đúng hub đích.
- Giữ group theo conversation.
- Giữ event name, số lượng argument, payload và cancellation semantics.

Giữ lifetime và synchronization đúng:

- Không inject scoped DbContext vào singleton.
- Giữ cơ chế factory/context theo operation khi cần.
- Giữ write lock và synchronization hiện có.
- Không thay đổi debounce, turn ordering, transcript merging hoặc session lifecycle ngoài nhu cầu tách dependency.

## 6. Các hợp đồng phải bảo toàn

- Toàn bộ REST routes, HTTP methods, status codes và JSON fields.
- Ba hub: `/hubs/tutor`, `/hubs/transcribe`, `/hubs/translate`.
- Public hub method names và SignalR event contracts.
- Request defaults, `default-learner`, timezone và CEFR behavior.
- Table/column names, types, precision, lengths, nullability, indexes, relationships và delete behavior.
- Migration IDs, migration history và dữ liệu hiện có.
- Configuration keys, CORS, startup migration/seeding behavior.
- Daily plan, carry-over, review scheduling, pronunciation scoring và conversation history behavior.

Không sửa frontend, không reset database, không chạy migration/update database hoặc seed trên database đang sử dụng.

Nếu đổi namespace entity/context, xử lý migration metadata và model snapshot tương ứng để việc đổi cấu trúc không sinh thay đổi schema ngoài ý muốn.

## 7. Thực hiện theo từng bước

1. Ghi lại baseline: routes, hub contracts, EF model/migrations và build.
2. Tách Domain và persistence mappings.
3. Tách Application và các use case CQRS.
4. Tách Infrastructure, DI và integrations.
5. Rút gọn API/Program.cs.
6. Build sau mỗi bước có thay đổi dependency.
7. Kiểm tra namespace/usings, compile globs và DI lifetimes.

Nếu ứng dụng đang chạy làm khóa output, dùng configuration hoặc output riêng để build; không tự dừng ứng dụng của người dùng.

## 8. Kiểm chứng và bàn giao

- Build đủ bốn project.
- Kiểm tra dependency direction và không có vòng tham chiếu.
- Kiểm tra DI registration và singleton/scoped dependencies.
- So sánh REST/SignalR contracts trước và sau.
- Kiểm tra EF model/schema compatibility offline.
- Phân biệt schema drift đã tồn tại ở baseline với drift phát sinh do refactor.
- Kiểm chứng tập trung các quy tắc nghiệp vụ vừa di chuyển bằng test phù hợp nếu cần.
- Không chạy test frontend hoặc suite ngoài phạm vi.
- Chạy `git diff --check`.

Viết `docs/architecture.md`:

- Trách nhiệm từng project.
- Feature và aggregate boundaries.
- Dependency diagram.
- Luồng command/query.
- Các lựa chọn pragmatic.
- Lệnh build, run và EF tooling sau refactor.
- Các phần còn giữ compatibility và giới hạn kiểm chứng.

Báo cáo cuối bằng tiếng Việt:

- Những gì đã refactor.
- File/project chính.
- Lệnh kiểm chứng và kết quả thực tế.
- Các thay đổi hành vi nếu có.
- Phần chưa kiểm chứng với SQL Server, Google AI hoặc realtime live.

Không báo “Completed & Verified” chỉ dựa trên build nếu chưa kiểm chứng runtime tương ứng.

## Tài liệu tham chiếu

- [Common web application architectures — Microsoft Learn](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures).
- [CQRS pattern — Microsoft Learn](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs).
