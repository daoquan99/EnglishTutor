# Review và prompt refactor Identity — Pragmatic Clean Architecture + DDD + CQRS

## Phạm vi và trạng thái review

- Workspace: `D:\Projects\EnglishTutor\backend`.
- Review ngày 2026-10-15, dựa trên working tree hiện tại, không dựa trên phiên bản trong ZIP hay README cũ.
- Phạm vi chính: `src/EnglishLiveTutor.Domain/Identity`, `src/EnglishLiveTutor.Application/Features/Identity`, `src/EnglishLiveTutor.Api/Endpoints/IdentityEndpoints.cs`.
- Đã đọc thêm `Infrastructure/Identity/IdentityService.cs`, DI, `Api/Program.cs`, `HttpCurrentUser`, EF mappings, seeder, CQRS interfaces, domain tests và `docs/architecture.md` để theo dõi luồng end-to-end.
- Đây là review source và prompt thực thi cho bước sau. Chưa sửa implementation; chưa chạy build, test, API hoặc SQL Server trong lượt review này.

## Findings

### 1. [P1 — lỗi chức năng liên quan] Điều kiện JWT key đang đảo chiều

`src/EnglishLiveTutor.Api/Program.cs:33` dùng `Encoding.UTF8.GetByteCount(jwtSecret) >= 32` làm điều kiện throw, trong khi thông báo yêu cầu tối thiểu 32 byte. Vì vậy key đủ dài bị chặn ở startup; key dưới 32 byte lại đi qua kiểm tra này.

Sửa điều kiện thành `< 32`, dùng cùng một options object cho validation và issuance. Không in key hoặc tạo key thật trong source.

### 2. [P2 — kiến trúc] Identity chưa triển khai CQRS trong Application

`src/EnglishLiveTutor.Application/Features/Identity/IIdentityService.cs:11-29` gom registration, credential verification, refresh sessions, user queries và RBAC management vào một interface. Folder Identity chỉ có file này; `Application/DependencyInjection.cs` chưa đăng ký Identity command/query handlers.

`IdentityEndpoints.cs:54-169` gọi interface trực tiếp thay vì đi qua `ICommandHandler` / `IQueryHandler`, dù repo đã có CQRS contracts tại `Application/Common/Interfaces/ICqrs.cs`.

Tách từng use case, DTO và technical ports. Không tạo handler chỉ để gọi lại nguyên phương thức trên service nghiệp vụ cũ.

### 3. [P2 — kiến trúc] Endpoint đang điều phối use case và phát JWT

`IdentityEndpoints.cs:22-47` chứa key lookup, JWT claims, signing và expiry. Các luồng register/login/refresh tại `54-85` điều phối nhiều bước, tạo refresh token và ghép `AuthResult`. Admin create tại `103-115` kiểm tra role, tạo user và gán role; self-protection checks nằm tại `124-125` và `132-133`.

Chuyển use-case orchestration và self-protection sang Application handlers; JWT issuance sang Infrastructure qua Application port. Endpoint giữ binding, authorization/rate limiting metadata và HTTP result mapping.

Việc request DTO được khai báo trong API không tự nó là vi phạm Clean Architecture. Có thể giữ HTTP contract DTO riêng và map sang command/query; không buộc đưa HTTP-specific types vào Domain.

### 4. [P2 — kiến trúc] Infrastructure đang sở hữu nghiệp vụ Identity/RBAC

`Infrastructure/Identity/IdentityService.cs:32-45` quyết định registration validation và role Student mặc định. Các đoạn `140-176`, `190-223`, `226-237` quyết định bảo vệ admin cuối cùng, role/permission built-in và quyền Admin; đồng thời thực hiện EF writes.

Di chuyển quy tắc nội tại của entity sang Domain và quyết định use case/cross-aggregate sang Application. Infrastructure giữ persistence, transaction/concurrency primitives và crypto adapters.

Refresh rotation tại `75-102` có conditional update, transaction và reuse revocation; phải bảo toàn các đặc tính này khi tách, không thay bằng read rồi SaveChanges thiếu kiểm soát concurrency.

### 5. [P2 — DDD] Domain đã độc lập dependency nhưng invariants còn thiếu

`Domain/Identity/UserEntity.cs:15-27`, `RoleEntity.cs:10-16`, `PermissionEntity.cs:10-15`, `RefreshTokenEntity.cs:14-22` chủ yếu trim/assign dữ liệu. Ví dụ Role có thể được tạo với tên rỗng dù service kiểm tra tối thiểu hai ký tự; Permission constructor không bảo vệ format code vốn được kiểm tra trong service.

Private setters và `RefreshTokenEntity.Revoke` idempotent là những phần đã phù hợp. Vấn đề là guard nằm ngoài Domain nên caller khác có thể bypass; không phải Domain đang trực tiếp tham chiếu EF hoặc ASP.NET Core.

Tập trung encapsulation và invariants thật sự. Folder phẳng, việc có join entity hoặc chưa dùng value object không đủ để kết luận sai kiến trúc. Không thêm navigation collection hoặc value object chỉ để đáp ứng hình thức DDD.

### 6. [P2 — lỗi chức năng liên quan] Admin create trả principal trước khi gán role bổ sung

`IdentityEndpoints.cs:111-115` lấy `user` từ `RegisterAsync`, sau đó gọi `SetUserRoleAsync` nhưng trả lại `user` cũ. `IdentityService.cs:43-45` dựng principal lúc mới có Student role. Khi admin tạo user với Teacher/Admin, response không phản ánh role bổ sung vừa gán.

Tạo một `CreateUserCommand` riêng, commit user và membership trong cùng transaction, rồi dựng projection theo trạng thái cuối cùng. Giữ JSON shape; ghi rõ đây là sửa giá trị response hiện đang sai.

### 7. [P2 — rủi ro consistency] Các bước ghi liên quan chưa luôn có cùng transaction

Admin create hiện tạo user và gán role qua hai operation, mỗi operation mở context riêng. `SetUserEnabledAsync` tại `152-162` cập nhật refresh token bằng `ExecuteUpdateAsync` rồi mới SaveChanges user mà không có explicit transaction bao cả hai bước.

Guard admin cuối cùng tại `140-149` là count-before-write; source chưa bảo đảm hai thao tác đồng thời không cùng vượt qua guard. Đây là rủi ro từ source, chưa được tái hiện trên SQL Server.

Khi refactor, xác định transaction boundary và concurrency strategy cho các use case này. Không tuyên bố guard an toàn đồng thời chỉ vì đã chuyển nó vào handler.

## Prompt thực thi

Bạn đang làm việc tại `D:\Projects\EnglishTutor\backend`.

Hãy refactor riêng Identity theo **Pragmatic Clean Architecture + DDD + CQRS**, dựa trên findings ở trên và source mới nhất tại thời điểm thực thi. Thực hiện thay đổi mã nguồn khi prompt này được sử dụng để triển khai; lượt review đã tạo file này chỉ là prompt authoring.

### A. Giới hạn scope

Phạm vi chính:

- `src/EnglishLiveTutor.Domain/Identity/**`.
- `src/EnglishLiveTutor.Application/Features/Identity/**`.
- `src/EnglishLiveTutor.Api/Endpoints/IdentityEndpoints.cs`.

Phạm vi phụ được sửa khi cần để nối luồng Identity:

- `src/EnglishLiveTutor.Infrastructure/Identity/**`.
- Application/Infrastructure dependency injection.
- Các đoạn authentication/Identity integration trong `src/EnglishLiveTutor.Api/Program.cs`.
- `src/EnglishLiveTutor.Api/HttpCurrentUser.cs`, hoặc adapter actor mới nếu cần.
- Identity mappings trong `Infrastructure/Persistence/AppDbContext.cs`.
- Call sites Identity trong `ApplicationCatalogSeeder.cs`.
- Identity tests và tài liệu kiến trúc.

Không refactor Conversations, Vocabulary, Learning, Pronunciation, AI prompts, teacher voices, realtime hubs hoặc frontend. Không sửa các base entity/aggregate dùng chung trên toàn hệ thống chỉ để phục vụ Identity.

Đọc hướng dẫn repository, kiểm tra `git status` và bảo toàn mọi thay đổi có sẵn. Source đang có nhiều file di chuyển/xóa/chưa tracked; không restore, reset hoặc coi chúng là rác cần dọn.

### B. Baseline và hợp đồng

Đọc lại toàn bộ Identity call sites; line numbers trong review chỉ định vị snapshot, không thay thế việc kiểm tra source hiện tại.

Lập contract matrix trước khi sửa, gồm request defaults, response shape, status/error mapping, Location header, authorization và rate limiting:

| Method | Route | Metadata cần bảo toàn |
|---|---|---|
| POST | `/api/auth/register` | Public, rate limit `auth` |
| POST | `/api/auth/login` | Public, rate limit `auth` |
| POST | `/api/auth/refresh` | Public, rate limit `auth` |
| POST | `/api/auth/logout` | Public, rate limit `auth` |
| GET | `/api/auth/me` | Authenticated |
| POST | `/api/admin/users` | `users.manage` |
| GET | `/api/admin/users` | `users.read` |
| PUT | `/api/admin/users/{id:guid}/enabled` | `users.manage` |
| PUT | `/api/admin/users/{id:guid}/roles/{role}` | `users.manage` |
| POST | `/api/admin/roles` | `roles.manage` |
| GET | `/api/admin/roles` | `roles.read` |
| DELETE | `/api/admin/roles/{name}` | `roles.manage` |
| POST | `/api/admin/permissions` | `roles.manage` |
| GET | `/api/admin/permissions` | `roles.read` |
| DELETE | `/api/admin/permissions/{code}` | `roles.manage` |
| PUT | `/api/admin/roles/{role}/permissions/{code}` | `roles.manage` |

Giữ `AuthResult`, `TokenPair`, principal và summary JSON fields. Không đưa PasswordHash, TokenHash hoặc domain entity trực tiếp vào HTTP responses. Giữ raw refresh token trả qua JSON như hiện tại, không tự đổi sang cookie/BFF flow.

Baseline token behavior lấy từ code/config thực tế: JWT expiry hiện đọc `Jwt:ExpiryMinutes`, không mặc định suy luận từ README nói 15 phút. Giữ configured lifetime hợp lệ; refresh hiện 14 ngày, random 64 byte, lưu SHA-256 hash. Không in giá trị secrets.

Hai thay đổi hành vi được chỉ định trong phạm vi này: sửa comparator kiểm tra JWT key và trả đúng roles sau admin create. Các cải thiện atomicity phải được ghi riêng trong báo cáo. Không tự thay đổi password policy, case sensitivity, logout access policy, token lifetimes hoặc membership semantics.

### C. Cấu trúc và dependency đích

Giữ bốn project hiện có và dependency direction trong `docs/architecture.md`. Không thêm BuildingBlocks project, MediatR, ASP.NET Identity schema hay auth provider mới.

Cấu trúc đề xuất:

```text
Domain/Identity/
  User/
    UserEntity.cs
    Entities/
      UserRoleEntity.cs
      RefreshTokenEntity.cs
  Role/
    RoleEntity.cs
    Entities/
      RolePermissionEntity.cs
  Permission/
    PermissionEntity.cs
  Policies/

Application/Features/Identity/
  Commands/<UseCase>/
  Queries/<UseCase>/
  Dtos/
  Abstractions/

Infrastructure/Identity/
  Persistence/
  Security/

Api/Endpoints/IdentityEndpoints.cs
```

Nhóm Domain theo root/ownership sau khi đánh giá cách persistence thực tế sử dụng chúng. RefreshToken có thể giữ lifecycle riêng nếu có lý do; ghi quyết định, không ép mọi quan hệ SQL thành một aggregate phải load toàn bộ. Di chuyển folder trước có thể giữ namespace hiện tại để hạn chế migration metadata churn.

### D. Domain bảo vệ invariants

- User bảo vệ email/display name và normalization phù hợp với contract hiện tại.
- Role bảo vệ tên/description; Permission bảo vệ code format/description và normalization.
- RefreshToken mô tả trạng thái expiry/revocation/replacement và giữ `Revoke` idempotent; nhận thời gian từ caller, không tự đọc clock hệ thống.
- UserRole/RolePermission bảo vệ định danh hợp lệ khi tạo mới; duplicate membership vẫn cần persistence unique constraints.
- Đặt built-in role/permission identifiers và domain policies ở một nơi, tránh lặp literals giữa handler, store và seeder.
- Quy tắc cần biết actor hoặc trạng thái nhiều user như self-disable/last active admin không được nhét vào entity bằng cách inject repository. Application lấy dữ liệu và áp dụng policy; Infrastructure bảo đảm consistency khi ghi.
- Plaintext password validation ở Application; hashing/verifying/random generation/JWT ở Infrastructure. Không đưa `IPasswordHasher`, `ClaimsPrincipal`, `IConfiguration`, `DbContext` hoặc token library vào Domain.
- Cập nhật seeder và creation flow tương ứng nếu thay constructor/factory; không làm hỏng bước khởi tạo user rồi hash password bằng adapter hiện tại.

### E. Application CQRS thực chất

Dùng `ICommand`, `ICommandHandler`, `IQuery`, `IQueryHandler` hiện có tại `Common/Interfaces/ICqrs.cs`; dùng `HandleAsync` và CancellationToken theo convention repo.

Commands:

- Register, Login, RefreshToken, Logout.
- CreateUser dành cho admin, tách khỏi public Register.
- SetUserEnabled, SetUserRole.
- CreateRole, DeleteRole.
- CreatePermission, DeletePermission.
- SetRolePermission.

Queries:

- GetCurrentUser.
- GetIdentityPrincipal theo user ID, dùng cho token validation.
- ListUsers, ListRoles, ListPermissions.

Login là command vì tạo session/refresh token. Query không phát token hoặc ghi dữ liệu.

Mỗi use case có command/query và handler riêng trong folder tương ứng; DTO tách khỏi file interface. Đăng ký handlers trong `Application/DependencyInjection.cs` theo convention existing handlers.

Handlers sở hữu orchestration và transaction boundary. Chuyển nghiệp vụ khỏi `Infrastructure/Identity/IdentityService.cs`, không giữ service này như nơi quyết định mọi use case rồi tạo handler facade ở ngoài.

Định nghĩa các ports đủ nhỏ theo nhu cầu, ví dụ user/role/permission persistence, principal reader, refresh-session store, password adapter, access-token issuer và unit of work. Đây là nhóm trách nhiệm gợi ý; tránh tạo interface cho từng dòng code hoặc một generic repository bao toàn hệ thống.

DTO/result của handler không phụ thuộc `IResult` hoặc HTTP status. Dùng cách phân biệt duplicate, invalid input, unauthorized và not found phù hợp repo; endpoint map đúng behavior hiện tại. Không dùng bool/null để gom các failure cần trả HTTP khác nhau.

### F. Infrastructure và tính nguyên tử

- JWT implementation ở Infrastructure, được gọi qua Application port. Endpoint không giữ `IssueJwt`, `JwtSecurityToken`, `SigningCredentials` hoặc trực tiếp lấy signing key.
- Giữ password hash format hiện có; không thay hasher hoặc làm mất khả năng verify user đang tồn tại.
- Persistence ports không quyết định Student mặc định, built-in protections hoặc HTTP response.
- Một Identity command dùng một unit of work/context nhất quán khi cần ghi nhiều bảng. Không dùng các repository tự mở context rồi commit độc lập trong cùng use case.
- Register giữ Student membership mặc định. Admin CreateUser giữ semantics Student cộng role bổ sung, không chuyển thành replace-role ngoài ý muốn.
- User + memberships trong CreateUser phải atomic; response principal được dựng sau thay đổi và phản ánh đầy đủ role cuối cùng.
- Disable user + revoke refresh sessions có cùng transaction.
- Last-active-admin check và mutation phải có concurrency strategy rõ ràng ở database để tránh count-before-write race. Không giải quyết bằng lock in-memory nếu database có thể được dùng bởi nhiều process.
- Refresh rotation giữ conditional consume, replacement insertion và transaction; một token chỉ được rotate thành công một lần. Reuse detection vẫn revoke active sessions theo semantics hiện tại.
- Không thay conditional SQL update bằng tracked read/write thiếu CAS/transaction. Domain state methods không tự thay thế được database concurrency control.
- Bảo toàn explicit timestamp/audit handling khi dùng bulk SQL vì `ExecuteUpdateAsync` không đi qua SaveChanges interceptor.
- Giữ soft deletion và EF query filters trên User/Role/Permission. Không physical-delete roots, không tự biến membership/session thành soft-delete aggregate.

### G. API và authentication integration

- Endpoint chỉ bind request/route, map sang command/query, gọi handler, map HTTP result và gắn metadata.
- Request DTO có thể ở API hoặc shared application contracts phù hợp; giữ property names/defaults.
- Đưa self-disable và self-remove-Admin checks vào handler để caller ngoài HTTP không bypass. Actor phải lấy từ adapter đáng tin cậy, không từ body client.
- `ICurrentUser.UserId` hiện có fallback `system` phục vụ audit. Không coi `system` là authenticated actor; parse/validate GUID hoặc dùng actor abstraction riêng trong phạm vi Identity.
- JWT bearer validation, middleware, rate limiter và policies vẫn là host concerns. Không chuyển `AddJwtBearer` hoặc `ClaimsPrincipal` vào Application/Domain.
- `OnTokenValidated` trong Program hiện gọi `IIdentityService.FindUserAsync`. Thay bằng principal query/read port phù hợp, giữ việc reject disabled/missing users và stale role/permission claim sets.
- Principal lookup cho token validation nhận user ID trực tiếp; không phụ thuộc GetCurrentUser từ token đã validated, không tạo vòng gọi authentication.
- Options cho issuer/audience/key/expiry dùng chung giữa signing và bearer validation; options type đặt nơi không khiến Infrastructure tham chiếu Api. Sửa key validation thành tối thiểu 32 UTF-8 byte; validate expiry dương mà không thay giá trị hợp lệ đang cấu hình.
- Giữ claim names, algorithm, issuer/audience defaults và clock skew hiện tại. Không in key, password, token hoặc connection string.
- Loại bỏ `IIdentityService` sau khi chuyển đủ callers; nếu cần adapter chuyển tiếp, không giữ nghiệp vụ trong adapter và giải thích lý do/thời hạn.

### H. Persistence compatibility

Giữ nguyên schema sáu bảng Identity: `Users`, `Roles`, `Permissions`, `UserRoles`, `RolePermissions`, `RefreshTokens`.

Bảo toàn keys, unique indexes, string lengths, FK/delete behavior, audit fields, soft-delete filters và migration IDs. Không thêm schema chỉ vì đổi folder/namespace.

Nếu đổi CLR namespace, cập nhật EF metadata/snapshot tương ứng và kiểm tra relational model diff. So sánh với baseline trước refactor; không nhận mọi pending model changes sẵn có là do refactor Identity.

Không chạy API Development để kiểm chứng tự động: startup hiện gọi `MigrateAsync` và seeding. Không chạy `database update`, seed hoặc migration trên database thật.

### I. Thứ tự thực hiện và kiểm chứng

1. Ghi baseline contracts/dependencies và chụp trạng thái EF model offline.
2. Tách DTO/ports, thiết kế handler signatures và result mapping.
3. Di chuyển domain invariants/policies và cập nhật seeder call sites.
4. Tách Infrastructure adapters/persistence; giữ transaction và concurrency guarantees.
5. Triển khai handlers, DI, thin endpoints và token-validation caller.
6. Xóa service/call sites cũ khi hết tham chiếu; cập nhật docs.

Lệnh build chạy từ backend:

```powershell
dotnet build src/EnglishLiveTutor.Api/EnglishLiveTutor.Api.csproj
git diff --check
rg -n "IIdentityService|IssueJwt" src
```

Kết quả rg không có match sau khi bỏ symbol cũ là kỳ vọng, exit code 1 của rg không phải build failure. Nếu giữ temporary adapter, ghi rõ mỗi match còn lại.

Chỉ thêm/chạy test liên quan Identity khi cần kiểm chứng thay đổi; không chạy full solution test hoặc test frontend. Có thể chạy test Identity hiện có bằng:

```powershell
dotnet test tests/EnglishLiveTutor.Domain.Tests/EnglishLiveTutor.Domain.Tests.csproj --filter "FullyQualifiedName~IdentityAndCatalogDomainTests.IdentityAggregates_HaveAuditAndLogicalDeletion|FullyQualifiedName~IdentityAndCatalogDomainTests.RefreshToken_Revoke_IsIdempotent"
```

Nếu test mới được tách class/project, báo đúng path/filter thực tế, không tự mở rộng sang Prompt/TeacherVoice tests chung file.

Các kiểm chứng có giá trị cần hoàn thành hoặc ghi rõ chưa làm:

- Domain invariants và revoke idempotence/expiry với thời gian xác định.
- Register/Login/Refresh/Logout trả đúng contracts và kết quả lỗi.
- JWT key dưới 32 byte bị từ chối; từ 32 byte trở lên không bị comparator chặn; configured expiry được giữ.
- Admin CreateUser trả roles sau gán và rollback khi membership write thất bại.
- Self-disable/self-remove protection nằm trong handlers; last-admin concurrency không bị bỏ qua.
- Rotation chỉ có một lần thành công, reuse/expiry/disabled user đúng behavior.
- Bearer validation vẫn reject stale roles/permissions.
- DI resolve các Identity handlers/adapters và không có singleton capture scoped dependencies.
- REST metadata và offline EF schema không đổi ngoài các bug fixes đã nêu.

Không dùng EF InMemory hoặc mock để tuyên bố đã chứng minh SQL transaction/CAS/concurrency. Nếu không có SQL Server dùng riêng cho test, ghi các gate này chưa được chạy; không sử dụng database của người dùng thay thế.

Nếu process đang khóa output build, dùng output/configuration riêng; không tự kill process. Không ghi secrets thật vào source để làm test hoặc build chạy được.

### J. Acceptance criteria và báo cáo

- Identity đi qua handlers trong Application; không còn business orchestration trong endpoint hoặc technical IdentityService.
- Domain bảo vệ invariants; dependency vẫn đúng và không mở rộng refactor base classes ngoài scope.
- Infrastructure sở hữu crypto, EF và implementation của atomic persistence operations.
- Public routes/JSON/policies/rate limiting tương thích; JWT comparator và admin-create projection được sửa có chủ đích.
- Call sites Program/DI/seeder được cập nhật; refresh-token concurrency và last-admin protections không bị suy yếu.
- Không thay schema/database/FE; không hoàn tác thay đổi người dùng.
- Báo cáo tiếng Việt: file chính, luồng trước/sau, quyết định pragmatic, command output, behavioral fixes và runtime/SQL gates chưa kiểm chứng.
- Không báo “Completed & Verified” chỉ dựa trên build, source review hoặc domain unit tests.

## Tham chiếu

- Kiến trúc repo: `backend/docs/architecture.md`; CQRS contracts: `backend/src/EnglishLiveTutor.Application/Common/Interfaces/ICqrs.cs`.
- [Application layer và command handlers — Microsoft Learn](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/microservice-application-layer-implementation-web-api).
- [DDD domain layer boundaries — Microsoft Learn](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/ddd-oriented-microservice).
- [JWT bearer validation — Microsoft Learn](https://learn.microsoft.com/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0).

Các tài liệu về microservice ở đây chỉ tham chiếu trách nhiệm các layer; prompt không yêu cầu chuyển backend thành microservices.
