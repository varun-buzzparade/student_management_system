using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Student.Upload;

namespace StudentManagementSystem.Services.Student.Registration;

public sealed class RegistrationDraftService : IRegistrationDraftService
{
    private readonly ApplicationDbContext _db;
    private readonly IStudentFileUploadService _uploadService;

    public RegistrationDraftService(ApplicationDbContext db, IStudentFileUploadService uploadService)
    {
        _db = db;
        _uploadService = uploadService;
    }

    public async Task<Guid> CreateDraftAsync(CancellationToken cancellationToken = default)
    {
        var draft = new RegistrationDraft { Id = Guid.NewGuid() };
        _db.RegistrationDrafts.Add(draft);
        await _db.SaveChangesAsync(cancellationToken);
        return draft.Id;
    }

    public async Task<bool> UpdateFieldAsync(Guid draftId, string fieldName, string? value, CancellationToken cancellationToken = default)
    {
        var draft = await _db.RegistrationDrafts.FindAsync(new object[] { draftId }, cancellationToken);
        if (draft == null) return false;

        switch (fieldName.ToLowerInvariant())
        {
            case "fullname":
                var fn = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                if (fn != null && fn.Length > 150) return false;
                draft.FullName = fn;
                break;
            case "dateofbirth":
                if (!DateOnly.TryParse(value, out var dob) || dob > DateOnly.FromDateTime(DateTime.UtcNow))
                    return false;
                draft.DateOfBirth = dob;
                break;
            case "heightcm":
                if (!decimal.TryParse(value, out var h) || h < 0 || h > 300) return false;
                draft.HeightCm = h;
                break;
            case "gender":
                if (!Enum.TryParse<Gender>(value, out var g) || g == Gender.Unknown) return false;
                draft.Gender = g;
                break;
            case "mobilenumber":
                var mn = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                if (mn != null && mn.Length > 20) return false;
                draft.MobileNumber = mn;
                break;
            case "email":
                var em = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                if (em != null && (em.Length > 256 || !IsValidEmail(em))) return false;
                draft.Email = em;
                break;
            case "profileimagepath":
                draft.ProfileImagePath = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                break;
            case "profilevideopath":
                draft.ProfileVideoPath = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                break;
            default:
                return false;
        }

        draft.LastUpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var at = email.IndexOf('@');
        return at > 0 && at < email.Length - 1 && email.IndexOf('@', at + 1) < 0;
    }

    public async Task<RegistrationDraft?> GetDraftAsync(Guid draftId, CancellationToken cancellationToken = default)
    {
        return await _db.RegistrationDrafts.AsNoTracking().FirstOrDefaultAsync(d => d.Id == draftId, cancellationToken);
    }

    public async Task DeleteDraftAsync(Guid draftId, CancellationToken cancellationToken = default)
    {
        var draft = await _db.RegistrationDrafts.FindAsync(new object[] { draftId }, cancellationToken);
        if (draft != null)
        {
            _uploadService.DeleteDraftFiles(draftId);
            _db.RegistrationDrafts.Remove(draft);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> DeleteExpiredDraftsAsync(int expiryMinutes, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMinutes(-expiryMinutes);
        var expired = await _db.RegistrationDrafts.Where(d => d.LastUpdatedAt < cutoff).ToListAsync(cancellationToken);
        if (expired.Count == 0) return 0;
        foreach (var d in expired)
            _uploadService.DeleteDraftFiles(d.Id);
        _db.RegistrationDrafts.RemoveRange(expired);
        await _db.SaveChangesAsync(cancellationToken);
        return expired.Count;
    }
}
