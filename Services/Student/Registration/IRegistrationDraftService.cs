using StudentManagementSystem.Models;

namespace StudentManagementSystem.Services.Student.Registration;

public interface IRegistrationDraftService
{
    Task<Guid> CreateDraftAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateFieldAsync(Guid draftId, string fieldName, string? value, CancellationToken cancellationToken = default);
    Task<RegistrationDraft?> GetDraftAsync(Guid draftId, CancellationToken cancellationToken = default);
    Task DeleteDraftAsync(Guid draftId, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredDraftsAsync(int expiryMinutes, CancellationToken cancellationToken = default);
}
