# SOLID Principles Review

## Summary

The codebase has been reviewed against SOLID principles. Several improvements were applied.

---

## S – Single Responsibility Principle ✓

- **Controllers**: Handle HTTP concerns (routing, model binding, validation, responses). Business logic delegated to services.
- **Services**: Focused responsibilities (Registration, Upload, Update, List, Mapping, Draft).
- **StudentRegistrationService**: Orchestrates registration; delegates email template loading, file upload, draft handling to respective services.

---

## O – Open/Closed Principle ✓

- New student operations can be added without modifying existing services.
- New upload types would require interface/service extension but no changes to existing methods.
- TempUploadOptions allows config changes without code changes.

---

## L – Liskov Substitution Principle ✓

- No inheritance hierarchies where subtypes could violate contracts.
- View models use inheritance (AdminStudentBaseViewModel, StudentFormBaseViewModel) appropriately.

---

## I – Interface Segregation Principle ✓

- Interfaces are focused: IStudentFileUploadService, IRegistrationDraftService, IStudentRegistrationService, etc.
- **IDraftWithFilePaths**: Minimal contract (Id, ProfileImagePath, ProfileVideoPath) so upload service does not depend on full RegistrationDraft.

---

## D – Dependency Inversion Principle ✓

- Controllers and services depend on abstractions (interfaces), not concretions.
- **Fixes applied**:
  1. **AccountController**: Replaced `HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>()` with constructor-injected `IWebHostEnvironment` (removed service locator).
  2. **IStudentFileUploadService.MoveDraftFilesToStudentAsync**: Now accepts `IDraftWithFilePaths` instead of `RegistrationDraft`, so the upload service depends on an abstraction.

---

## Compression Module (SOLID Refactor)

- **UploadConstants**: Single source for extensions, max sizes, subdir names (DRY).
- **IFfmpegLocator / FfmpegLocator**: FFmpeg path resolution extracted (SRP, testable).
- **IImageCompressor / ImageCompressor**: Image compression via ImageSharp (SRP).
- **IVideoCompressor / VideoCompressor**: Video compression via FFmpeg (SRP).
- **BackgroundCompressionService**: Orchestration only; delegates to compressors (DIP).
- **MoveDraftMediaAndQueue**: Shared helper for image/video move+queue (DRY).

---

## Optional Future Improvements

- **Email template**: `StudentRegistrationService.GetWelcomeEmailBodyAsync` reads file and does string replacement. Could extract to `IWelcomeEmailTemplateService` for SRP.
- **RegistrationDraftService**: Uses `ApplicationDbContext` directly. For strict DIP, could introduce `IRegistrationDraftRepository`; current approach is acceptable for project size.
