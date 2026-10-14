# Sửa lỗi build backend EnglishTutor (.NET 10 + SQL Server)

Bạn đang làm việc tại `D:\Projects\EnglishTutor`. Hãy sửa trực tiếp các lỗi build của backend `backend/EnglishLiveTutor.Api.csproj`, build lại và hoàn thành đến khi dự án biên dịch thành công. Dùng source hiện tại làm căn cứ, giữ các tính năng đang được phát triển.

## Phạm vi

- Chỉ sửa backend và cấu hình build/package liên quan trực tiếp đến lỗi. Không sửa frontend.
- Giữ `TargetFramework=net10.0` và EF Core SQL Server. Không chuyển sang SQLite hay nâng target framework lên .NET 11.
- Working tree có nhiều file đã sửa và file mới chưa tracked. Đọc `git status --short` trước khi làm; bảo toàn các thay đổi sẵn có. Không reset, checkout, clean, xóa file hoặc hoàn tác chức năng để làm build xanh.
- Không bỏ file `.cs` khỏi compilation, comment toàn bộ service/endpoint, dùng `dynamic`, tạo class rỗng hoặc thêm property giả chỉ để né lỗi compiler.
- Không chạy migration vào database, không chạy seeder hoặc gọi Google/Gemini thật trong nhiệm vụ sửa build. Không in connection string có credentials hay API key.
- Không commit, push, nâng cấp hàng loạt thư viện hoặc refactor kiến trúc ngoài phạm vi.

## Bằng chứng đã ghi nhận

Lệnh chạy từ thư mục gốc:

```powershell
dotnet build backend/EnglishLiveTutor.Api.csproj --no-restore
```

Kết quả: exit code `1`, `Build FAILED`, `1 Error(s)`:

```text
backend/Services/VocabularyLearningService.cs(26,28): error CS0246:
The type or namespace name 'AiModelCatalogEntity' could not be found
(are you missing a using directive or an assembly reference?)
```

Source tại thời điểm tạo prompt:

- `backend/Services/VocabularyLearningService.cs:26`: `GetGenerationModelsAsync` trả về `Task<List<AiModelCatalogEntity>>`, rồi query `db.AiModelCatalog` và gọi `ToListAsync(ct)`.
- `backend/Data/AppDbContext.cs:27`: `AiModelCatalog` là `DbSet<AiModelConfigEntity>`.
- `backend/Data/VocabularyDomainEntities.cs:215`: entity hiện có là `AiModelConfigEntity`, namespace `EnglishLiveTutor.Api.Data`.
- `backend/Data/DatabaseSeeder.cs:334`: seeder dùng `List<AiModelConfigEntity>`.
- `backend/Services/PronunciationAssessmentService.cs:22`: `GetAssessmentModelsAsync` đã trả về `Task<List<AiModelConfigEntity>>`.
- `PronunciationAssessmentService.cs:140-141` tham chiếu `ReplacementModelCode`, trong khi `AiModelConfigEntity` hiện khai báo `RoutedModelCode`. Đây là bất nhất phát hiện qua đọc source; chưa được báo riêng bởi lần build trên. Phải xác minh ý nghĩa routing và các nơi sử dụng trước khi sửa.

Lần build chỉ xác nhận lỗi compiler đầu tiên đang được báo. Không mặc định rằng sửa một dòng là hết lỗi: build lại để tìm các lỗi tiếp theo.

Máy đang chọn SDK `11.0.100-rc.1.26425.128`, xuất thông báo `NETSDK1057` về preview SDK. Project vẫn target `net10.0`; thông báo này không phải lỗi `CS0246`. Không đổi target framework hoặc cài lại SDK chỉ để xử lý lỗi tên entity.

## Các file cần kiểm tra

- `backend/EnglishLiveTutor.Api.csproj`
- `backend/Services/VocabularyLearningService.cs`
- `backend/Services/PronunciationAssessmentService.cs`
- `backend/Data/AppDbContext.cs`
- `backend/Data/VocabularyDomainEntities.cs`
- `backend/Data/DatabaseSeeder.cs`
- `backend/Models/VocabularyDtos.cs`
- `backend/Program.cs`
- Các service/entity/DTO khác được compiler chỉ ra sau khi build lại.

Chỉ đọc migrations trong `backend/Migrations/` khi cần xác minh mapping. Không tái tạo hoặc sửa migration history để xử lý lỗi tên type trong service.

## Cách thực hiện

1. Đọc hướng dẫn `AGENTS.md` nếu có, kiểm tra trạng thái Git và project/package hiện tại. Source có thể đã thay đổi sau khi prompt được tạo.
2. Restore và build backend để lấy baseline mới. Giữ nguyên mã lỗi, tên file, vị trí và exit code; phân biệt lỗi compiler, cảnh báo NuGet và lỗi truy cập nguồn package.
3. Tìm tất cả chỗ tham chiếu `AiModelCatalogEntity` và `AiModelConfigEntity`. Đồng bộ service với entity đã được DbContext và seeder sử dụng. Với source hiện tại, hướng sửa tối thiểu là dùng `AiModelConfigEntity` cho kiểu trả về của `GetGenerationModelsAsync`; không tạo entity thứ hai cùng mô tả bảng `AiModelCatalog`.
4. Build lại sau sửa đầu tiên. Xử lý tất cả lỗi compiler tiếp theo của backend bằng cách đối chiếu entity, DTO, service, mapping và constructor thực tế. Không thêm namespace/package theo phỏng đoán.
5. Với bất nhất `ReplacementModelCode`/`RoutedModelCode`, đọc metadata và các luồng chọn model. Dùng contract có thật nếu phù hợp, giữ ý nghĩa định tuyến model và kiểm tra capability. Không tự bịa model hoặc khẳng định provider hỗ trợ tính năng chưa kiểm chứng. Nếu cần đổi schema mới giải quyết được, nêu bằng chứng cụ thể thay vì tự chạy cập nhật database.
6. Bảo toàn các endpoint, SignalR hubs, conversation modes, vocabulary, daily learning và pronunciation assessment. Các sửa đổi phải khôi phục tính nhất quán của source, không loại bỏ tính năng đang gây lỗi.

## Package và cảnh báo NuGet

Source project ở thời điểm tạo prompt đang ghi `Microsoft.AspNetCore.OpenApi` và các package EF Core/Tools/Design là `10.0.0`. Những lượt trước đã từng nâng lên `10.0.13`; phải kiểm tra lại source và kết quả restore, không dựa vào cache `obj` để kết luận phiên bản đang dùng.

Nếu restore tái hiện cảnh báo bảo mật `NU1902`/`NU1903` liên quan các dependency đã thảo luận:

- Dùng bản vá ổn định tương thích .NET 10; đồng bộ EF Core SqlServer/Tools/Design khi cần nâng nhóm EF. `10.0.13` là lựa chọn đã được xác minh trong phiên làm việc trước, cần kiểm tra nguồn package hiện tại trước khi áp dụng.
- `Microsoft.AspNetCore.OpenApi 10.0.13` trước đó đã resolve `Microsoft.OpenApi 2.12.2`. Hai package có chu kỳ version riêng. Không ép `Microsoft.OpenApi` thành version 10 hoặc thêm lại pin `2.7.5` làm hạ dependency mới hơn.
- `System.Security.Cryptography.Pkcs 10.0.13` là bản vá nhánh 10.x đã được kiểm tra; nếu SqlClient vẫn kéo bản bị cảnh báo, có thể thêm reference trực tiếp bản vá này. Xác minh dependency thực tế sau restore.
- Không đặt `NuGetAudit=false`, thêm `NoWarn` hoặc hạ `ErrorActionPreference` để giấu cảnh báo. Báo rõ nếu nguồn package/network ngăn xác minh.

Chỉ sửa các package liên quan trực tiếp đến vấn đề được tái hiện; không mở rộng thành cập nhật mọi dependency.

## Kiểm chứng bắt buộc

Chạy từ `D:\Projects\EnglishTutor`:

```powershell
dotnet restore backend/EnglishLiveTutor.Api.csproj
dotnet build backend/EnglishLiveTutor.Api.csproj --no-restore
git diff --check
```

Sau khi build thành công, đọc diff các file đã sửa để xác nhận thay đổi tối thiểu và không làm mất tính năng. Không chạy toàn bộ test suite, test frontend, database update hay AI requests. Nếu lỗi đòi hỏi kiểm tra bổ sung, chỉ kiểm tra phần backend liên quan và giải thích mục đích.

## Tiêu chí hoàn thành và báo cáo

- Backend restore và build thành công, exit code `0`, không còn lỗi compiler.
- `GetGenerationModelsAsync` nhất quán với DbContext và entity hiện có.
- Mọi lỗi phát sinh tiếp theo trong build đã được xử lý; không dừng ở việc chỉ đưa đề xuất.
- Các cảnh báo còn lại được ghi rõ cùng nguyên nhân; không gọi warning là build error và không che warning.
- Diff chỉ chứa sửa đổi cần thiết trong phạm vi backend; giữ .NET 10, SQL Server và các thay đổi sẵn có.
- Báo cáo bằng tiếng Việt: nguyên nhân gốc, các file/symbol đã sửa, lệnh kiểm chứng và kết quả thực tế, cảnh báo hoặc phần chưa xác minh.
- Build thành công chỉ chứng minh biên dịch; không tuyên bố database, migrations, seeder hoặc Gemini runtime đã hoạt động khi chưa chạy kiểm chứng tương ứng.
