using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services;

/// <summary>
/// Caches the admin student list (per query). Uses a version key; when InvalidateAsync is called,
/// the version is incremented so all cached list entries are effectively invalidated.
/// </summary>
public sealed class StudentListCacheService : IStudentListCacheService
{
    private const string CacheKeyPrefix = "AdminStudentList";
    private const string VersionKey = "AdminStudentListVersion";
    private static readonly TimeSpan VersionTtl = TimeSpan.FromHours(24);
    private static readonly TimeSpan SlidingTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan AbsoluteTtl = TimeSpan.FromMinutes(30);

    private readonly IDistributedCache _cache;

    public StudentListCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<AdminStudentListViewModel> GetOrAddAsync(
        StudentListQueryViewModel query,
        Func<StudentListQueryViewModel, CancellationToken, Task<AdminStudentListViewModel>> factory,
        CancellationToken cancellationToken = default)
    {
        var version = await GetOrInitVersionAsync(cancellationToken);
        var cacheKey = BuildKey(version, query);

        var bytes = await _cache.GetAsync(cacheKey, cancellationToken);
        if (bytes != null)
        {
            var cached = JsonSerializer.Deserialize<AdminStudentListViewModel>(bytes);
            if (cached != null)
                return cached;
        }

        var model = await factory(query, cancellationToken);
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = SlidingTtl,
            AbsoluteExpirationRelativeToNow = AbsoluteTtl
        };
        await _cache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(model), options, cancellationToken);
        return model;
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        // Bump version so existing list cache keys (which include version) are no longer hit
        var version = await GetOrInitVersionAsync(cancellationToken);
        await _cache.SetAsync(
            VersionKey,
            JsonSerializer.SerializeToUtf8Bytes(version + 1),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = VersionTtl },
            cancellationToken);
    }

    private async Task<int> GetOrInitVersionAsync(CancellationToken cancellationToken)
    {
        var bytes = await _cache.GetAsync(VersionKey, cancellationToken);
        if (bytes != null)
            return JsonSerializer.Deserialize<int>(bytes);

        const int initial = 1;
        await _cache.SetAsync(
            VersionKey,
            JsonSerializer.SerializeToUtf8Bytes(initial),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = VersionTtl },
            cancellationToken);
        return initial;
    }

    private static string BuildKey(int version, StudentListQueryViewModel q) =>
        $"{CacheKeyPrefix}_v{version}_{q.Page}_{q.PageSize}_{q.Name}_{q.Age}_{q.Gender}_{q.MobileNumber}_{q.Email}";

}
