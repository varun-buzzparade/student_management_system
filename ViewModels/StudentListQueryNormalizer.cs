using StudentManagementSystem.Models;

namespace StudentManagementSystem.ViewModels;

/// <summary>
/// Shared normalization for StudentListQueryViewModel (paging, page size bounds).
/// Used by Admin list, cache key, and query service so all stay in sync.
/// </summary>
public static class StudentListQueryNormalizer
{
    public static void Normalize(StudentListQueryViewModel q)
    {
        if (q.Page < 1) q.Page = 1;
        if (q.PageSize is < 5 or > 100) q.PageSize = 20;
    }
}
