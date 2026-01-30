namespace StudentManagementSystem.Configuration;

/// <summary>
/// Single source of truth for draft/temp upload expiry. Binds to appsettings.json "TempUpload" section.
/// Change ExpiryMinutes in appsettings.json only; no hardcoding elsewhere.
/// </summary>
public class TempUploadOptions
{
    public const string SectionName = "TempUpload";

    /// <summary>Minutes after which draft records and files are deleted if not submitted. Default 30.</summary>
    public int ExpiryMinutes { get; set; } = 30;
}
