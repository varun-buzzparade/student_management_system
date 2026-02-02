# Compression Architecture Review & Test Findings

## Compression Algorithms (As Discussed)

| Type | Algorithm | Settings |
|------|-----------|----------|
| **Image** | ImageSharp | Resize max 1920×1080, JPEG quality 85 / PNG BestCompression |
| **Video** | FFmpeg libx264 | CRF 23, `-c:a copy` (audio passthrough) |

## Test Results (Standalone Script)

Run: `dotnet run --project Scripts/TestCompress -- [studentId]`

### STU20260202061519744 (requested)
- **Image** (72 KB PNG): 73,869 → 74,033 bytes **(100.2%)** – no meaningful reduction
- **Video** (95 MB): **FFmpeg failed** – file reports "moov atom not found" (corrupt or incomplete)

### STU20260130071858690 (valid H.264)
- **Image** (42 KB PNG): 43,686 → 74,033 bytes – output larger (different encoding)
- **Video** (27 MB): 27 MB → **~0.76 MB (2.8%)** – strong compression as intended

## Findings

### 1. Video in STU20260202061519744 Is Corrupt
- "moov atom not found" indicates an incomplete or broken MP4
- Typical causes: interrupted upload, OneDrive placeholder, or streamed write still in progress
- FFmpeg cannot process it; compression will always fail for this file

### 2. Image Compression Behavior
- For small images (< 100 KB), PNG BestCompression often yields little or no reduction
- Resize (max 1920×1080) mainly helps larger images
- Converting PNG → JPEG would reduce size more but changes format

### 3. Architecture Flow (Verified)

```
Upload (Draft)     → SaveDraftFileAsync → NO compression queued
                    (files in uploads/images|videos/{draftId}/)

Submit with Draft  → MoveDraftFilesToStudentAsync → QueueForCompression(image), QueueForCompression(video)
                    (after folder rename to {studentId})

Submit with Form   → SaveImageAsync/SaveVideoAsync → QueueForCompression
                    (direct to {studentId})

BackgroundService  → ExecuteAsync reads Channel → CompressImageAsync / CompressVideoAsync
                    → overwrites original in place
```

### 4. DI Registration (Verified)
- `BackgroundCompressionService` – Singleton
- `IBackgroundCompressionService` – same singleton
- `AddHostedService` – same singleton
- `StudentFileUploadService` – Scoped, receives `IBackgroundCompressionService`
- Same Channel instance used for all queuing and processing

### 5. Why Compression Might Not Appear in the App
1. **Corrupt videos** – FFmpeg fails silently (logs warning, keeps original)
2. **Small images** – compression barely changes size
3. **Compression not queued** – draft path mismatch or missing `ProfileImagePath` / `ProfileVideoPath`
4. **Long processing** – large video compression takes minutes; user may check too early

## Expected Sizes After Compression

| Original | Expected After |
|----------|----------------|
| 95 MB video (if valid) | ~15–40 MB (CRF 23) |
| 27 MB H.264 video | ~0.7–3 MB (re-encode) |
| 5 MB image | ~0.5–2 MB (resize + compress) |
| 72 KB image | ~70–80 KB (little change) |

## Recommendations

1. **Validate video before queueing** – e.g. quick FFprobe check; skip queue if corrupt
2. **Optionally convert PNG → JPEG** – for images without transparency, to improve size reduction
3. **Exclude OneDrive placeholder files** – if using Files On-Demand, ensure files are fully downloaded before processing
