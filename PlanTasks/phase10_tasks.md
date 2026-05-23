# Phase 10: Audio / Storage / Gemini Live — Detailed Tasks

> **Goal:** File storage abstractions, cloud storage adapters, audio upload for speaking/vocabulary, TTS generation, Gemini Live preparation
> **Dependencies:** Phase 3 (Vocabulary, AI), Phase 4 (Speaking)
> **Estimated tasks:** 7

---

## Task 10.1: File Storage Abstractions in BuildingBlocks

**Agent:** Module Implementer

**Files to create:**

```
src/BuildingBlocks/EnglishTutor.BuildingBlocks.Infrastructure/
├── Storage/
│   ├── IFileStorageService.cs
│   ├── IAudioStorageService.cs
│   ├── StorageOptions.cs
│   ├── FileMetadata.cs
│   └── StorageProvider.cs
```

**IFileStorageService:**
```csharp
public interface IFileStorageService
{
    Task<FileMetadata> UploadAsync(Stream file, string fileName, string contentType, string? folder, CancellationToken ct);
    Task<Stream> DownloadAsync(string fileKey, CancellationToken ct);
    Task DeleteAsync(string fileKey, CancellationToken ct);
    string GeneratePresignedUrl(string fileKey, TimeSpan expiry);
    Task<bool> ExistsAsync(string fileKey, CancellationToken ct);
}
```

**IAudioStorageService : IFileStorageService** — marker interface for audio-specific DI resolution

**FileMetadata:**
- `string FileKey`, `string FileName`, `string ContentType`, `long SizeBytes`, `string Url`, `DateTime UploadedAtUtc`

**StorageOptions:**
- `StorageProvider Provider (Local/S3/R2/GCS/AzureBlob)`
- `string BucketName`, `string Region`, `string AccessKey`, `string SecretKey`, `string BaseUrl`, `string LocalPath (for dev)`

**StorageProvider enum:** `Local, S3, CloudflareR2, GoogleCloudStorage, AzureBlobStorage`

**Acceptance Criteria:**
- [x] Abstraction is cloud-agnostic
- [x] FileKey is unique path (e.g., `speaking/sessions/{sessionId}/turn-{turnId}.webm`)
- [x] Presigned URL supports time-limited access
- [x] StorageOptions configurable per environment

---

## Task 10.2: Storage Implementations

**Agent:** Module Implementer

**Files to create:**

```
src/BuildingBlocks/EnglishTutor.BuildingBlocks.Infrastructure/
├── Storage/
│   ├── Local/
│   │   └── LocalFileStorageService.cs
│   ├── S3/
│   │   └── S3FileStorageService.cs
│   └── DependencyInjection/
│       └── StorageServiceRegistration.cs
```

**LocalFileStorageService : IFileStorageService, IAudioStorageService:**
- Stores files on local disk at `StorageOptions.LocalPath`
- FileKey maps to subdirectory + filename
- `GeneratePresignedUrl` returns local file path (dev only)
- Creates directories automatically

**S3FileStorageService : IFileStorageService, IAudioStorageService:**
- Uses AWS SDK `AmazonS3Client`
- Uploads to `StorageOptions.BucketName`
- FileKey = S3 object key
- `GeneratePresignedUrl` uses `GetPreSignedURL`
- Supports Cloudflare R2 (S3-compatible endpoint)

**StorageServiceRegistration:**
```csharp
public static IServiceCollection AddFileStorage(this IServiceCollection services, StorageOptions options)
{
    return options.Provider switch
    {
        StorageProvider.Local => services.AddSingleton<IFileStorageService, LocalFileStorageService>()
                                        .AddSingleton<IAudioStorageService, LocalFileStorageService>(),
        StorageProvider.S3 or StorageProvider.CloudflareR2 =>
            services.AddSingleton<IFileStorageService, S3FileStorageService>()
                    .AddSingleton<IAudioStorageService, S3FileStorageService>(),
        _ => throw new NotSupportedException($"Storage provider {options.Provider} not supported")
    };
}
```

**Add to Directory.Packages.props:** `AWSSDK.S3`

**Acceptance Criteria:**
- [x] Local storage works for dev without any cloud config
- [x] S3 storage works with configurable endpoint (supports R2)
- [x] DI registration switches based on config
- [x] Files stored with correct content type
- [x] Presigned URLs expire correctly

---

## Task 10.3: Speaking — Audio Upload for Turns

**Agent:** Module Implementer

**Files to modify:**

```
src/Modules/Speaking/EnglishTutor.Modules.Speaking.Application/Commands/AddTurn/
    AddSpeakingTurnCommand.cs  (add Stream? AudioFile, string? AudioContentType)
    AddSpeakingTurnCommandHandler.cs  (upload audio before correction)

src/Modules/Speaking/EnglishTutor.Modules.Speaking.Domain/Entities/
    SpeakingTurn.cs  (AudioUrl already exists, ensure it's set)
```

**Updated AddSpeakingTurnCommandHandler flow:**
1. Load session → validate Active
2. **If AudioFile provided:**
   - Generate file key: `speaking/sessions/{sessionId}/turn-{turnNumber}.webm`
   - Call `IAudioStorageService.UploadAsync(audioFile, fileName, contentType)`
   - Store `AudioUrl = fileMetadata.Url`
   - Optionally: call speech-to-text service to get `RecognizedText` from audio
   - Use `RecognizedText` as the user text for correction
3. **If text provided directly:** use text as before
4. Build correction request → call AI.Contracts → store result
5. Save turn + result + outbox message

**New field on AddSpeakingTurnCommand:**
- `Stream? AudioFile`, `string? AudioContentType`, `string? UserText`
- Validation: either AudioFile or UserText must be provided

**Acceptance Criteria:**
- [x] Audio upload stores file via storage service
- [x] AudioUrl saved on SpeakingTurn entity
- [x] Text-only turns still work without audio
- [x] File key follows consistent naming pattern
- [x] Speaking module references IAudioStorageService from BuildingBlocks only

---

## Task 10.4: Vocabulary — Pronunciation Audio Upload

**Agent:** Module Implementer

**Files to modify:**

```
src/Modules/Vocabulary/EnglishTutor.Modules.Vocabulary.Application/Commands/SubmitPronunciationAttempt/
    SubmitPronunciationAttemptCommand.cs  (add Stream AudioFile, string AudioContentType)
    SubmitPronunciationAttemptCommandHandler.cs  (upload audio, get pronunciation score)

src/Modules/Vocabulary/EnglishTutor.Modules.Vocabulary.Application/Commands/SubmitExamplePronunciation/
    SubmitExamplePronunciationCommand.cs
    SubmitExamplePronunciationCommandHandler.cs
```

**Updated SubmitPronunciationAttemptCommandHandler flow:**
1. Upload audio: `vocabulary/pronunciation/{userId}/{vocabularyItemId}/{timestamp}.webm`
2. Call AI pronunciation scoring service (via `AI.Contracts`):
   - Send audio + expected word/phonetic
   - Get back: PronunciationScore, AccuracyScore, FluencyScore, CompletenessScore, RecognizedText, Feedback
3. Create `VocabularyPronunciationAttempt` with all scores + AudioUrl
4. Update `UserVocabularyMastery.PronunciationMasteryScore`
5. Save outbox event

**Updated SubmitExamplePronunciationCommandHandler flow:**
1. Upload audio: `vocabulary/examples/{userId}/{exampleId}/{timestamp}.webm`
2. Call AI: send audio + expected sentence text
3. Get back: PronunciationScore, FluencyScore, AccuracyScore, WordLevelFeedbackJson, RecognizedText
4. Create `ExampleSentencePronunciationAttempt`
5. Update mastery + save outbox event

**Acceptance Criteria:**
- [x] Audio files uploaded with unique keys per attempt
- [x] AI pronunciation scoring returns per-word feedback for sentences
- [x] Mastery scores updated after each attempt
- [x] Both word + sentence pronunciation flows work

---

## Task 10.5: AI — TTS Integration (Gemini TTS)

**Agent:** Module Implementer

**Files to create/modify:**

```
src/Modules/AI/EnglishTutor.Modules.AI.Infrastructure/Clients/
    GeminiTtsClient.cs

src/Modules/AI/EnglishTutor.Modules.AI.Infrastructure/ContractImplementations/
    AudioGenerationService.cs

src/Modules/AI/EnglishTutor.Modules.AI.Contracts/Services/
    IAudioGenerationService.cs  (already exists, implement)
```

**IAudioGenerationService (in AI.Contracts):**
```csharp
public interface IAudioGenerationService
{
    Task<AudioGenerationResult> GenerateWordPronunciationAsync(string word, string languageCode, CancellationToken ct);
    Task<AudioGenerationResult> GenerateSentencePronunciationAsync(string sentence, string languageCode, CancellationToken ct);
}
```

**AudioGenerationResult:**
- `Stream AudioStream`, `string ContentType`, `int DurationMs`, `string LanguageCode`

**GeminiTtsClient:**
- Calls Gemini TTS API with text + language
- Returns audio stream (MP3/WAV)
- Configurable voice settings

**AudioGenerationService : IAudioGenerationService:**
1. Call `GeminiTtsClient` to generate audio
2. Upload to storage via `IAudioStorageService`: `tts/{languageCode}/{hash}.mp3`
3. Log request in `AiRequestLog`
4. Return audio URL

**Use case: Generate sample pronunciation for vocabulary items:**
- Admin or background job triggers TTS generation for vocabulary words + example sentences
- Generated AudioUrl stored on `VocabularyItem` or `VocabularyExample`

**Acceptance Criteria:**
- [x] TTS generation works for single words and sentences
- [ ] Generated audio stored in cloud storage
- [ ] AI request logged with token/cost metrics
- [x] Audio cached by text hash (don't regenerate same text)

---

## Task 10.6: AI — Gemini Live Preparation

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/AI/EnglishTutor.Modules.AI.Infrastructure/Clients/
    GeminiLiveClient.cs
    GeminiLiveOptions.cs

src/Modules/AI/EnglishTutor.Modules.AI.Contracts/Services/
    IRealtimeVoiceService.cs

src/Modules/AI/EnglishTutor.Modules.AI.Contracts/DTOs/
    RealtimeSessionConfig.cs
    RealtimeMessage.cs
```

**IRealtimeVoiceService (in AI.Contracts):**
```csharp
public interface IRealtimeVoiceService
{
    Task<RealtimeSessionInfo> StartSessionAsync(RealtimeSessionConfig config, CancellationToken ct);
    Task SendAudioChunkAsync(Guid sessionId, byte[] audioChunk, CancellationToken ct);
    Task<RealtimeMessage> ReceiveMessageAsync(Guid sessionId, CancellationToken ct);
    Task EndSessionAsync(Guid sessionId, CancellationToken ct);
}
```

**RealtimeSessionConfig:**
- `string TargetLanguageCode`, `string UserLevel`, `string Topic`, `string SystemInstruction`

**GeminiLiveClient:**
- WebSocket connection to Gemini Live API
- Manages session lifecycle
- Streams audio chunks bidirectionally
- Handles connection errors + reconnection

**Note:** This is preparation/foundation only. Full real-time voice speaking sessions will require WebSocket endpoints in the API host and significant frontend work. Mark as future enhancement.

**Acceptance Criteria:**
- [ ] GeminiLiveClient can establish WebSocket connection
- [x] Session config passes language context
- [x] Abstraction in Contracts allows Speaking module to use later
- [x] Connection error handling + graceful shutdown

---

## Task 10.7: Tests

**Agent:** Test Writer

**Files to create:**

```
tests/EnglishTutor.BuildingBlocks.Infrastructure.Tests/
├── Storage/
│   ├── LocalFileStorageServiceTests.cs
│   └── S3FileStorageServiceTests.cs

tests/EnglishTutor.Modules.Speaking.UnitTests/
└── Application/
    └── AddSpeakingTurnWithAudioTests.cs

tests/EnglishTutor.Modules.Vocabulary.UnitTests/
└── Application/
    └── SubmitPronunciationWithAudioTests.cs

tests/EnglishTutor.Modules.AI.UnitTests/
└── Services/
    └── AudioGenerationServiceTests.cs
```

**Key test cases:**
- LocalFileStorage: upload creates file, download returns content, delete removes file
- S3FileStorage: upload with mock S3 client, presigned URL generation
- Speaking turn with audio: uploads file, stores URL, still calls AI correction
- Speaking turn text-only: works without audio
- Vocabulary pronunciation: uploads audio, AI returns scores, mastery updated
- TTS generation: caches by text hash, doesn't regenerate
- Gemini Live client: session start/end lifecycle

**Target:** At least 12 tests

**Acceptance Criteria:**
- [x] Storage tests use real local filesystem (integration)
- [x] S3 tests mock AWS client
- [ ] Audio upload flows tested end-to-end with mock storage
- [x] All tests pass

---

## Phase 10 Definition of Done

- [x] File storage abstraction supports Local + S3/R2
- [ ] Speaking turn accepts audio upload → stores → processes
- [ ] Vocabulary pronunciation accepts audio → AI scores → mastery updated
- [x] TTS generates sample audio for vocabulary words/sentences
- [ ] Gemini Live client can establish WebSocket connection
- [x] Storage provider switchable via configuration
- [x] `dotnet build && dotnet test` passes
- [x] At least 12 tests pass
- [x] No storage implementation leaked outside BuildingBlocks.Infrastructure
