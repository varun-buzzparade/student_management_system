using Microsoft.Extensions.Configuration;

namespace StudentManagementSystem.Configuration;

/// <summary>
/// Centralized config access. Tries environment variables first (prod-style secrets),
/// then IConfiguration (appsettings, etc.). Use config keys like "AdminCredentials:Email";
/// env vars use "__" for hierarchy, e.g. AdminCredentials__Email.
/// </summary>
public static class ConfigHelper
{
    /// <summary>
    /// Converts a config key (e.g. "AdminCredentials:Email") to the env-var form (e.g. "AdminCredentials__Email").
    /// </summary>
    public static string ToEnvVarKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return key;
        return key.Replace(":", "__");
    }

    /// <summary>
    /// Gets a config value: Environment.GetEnvironmentVariable(envKey) first, then configuration[key].
    /// Key uses ":" for hierarchy (e.g. "AdminCredentials:Email"). Env equivalent uses "__".
    /// </summary>
    public static string? GetValue(string key, IConfiguration configuration)
    {
        if (string.IsNullOrEmpty(key)) return null;

        var envKey = ToEnvVarKey(key);
        var fromEnv = Environment.GetEnvironmentVariable(envKey);
        if (!string.IsNullOrEmpty(fromEnv)) return fromEnv;

        return configuration[key];
    }

    /// <summary>
    /// Gets a connection string: ConnectionStrings:{name}. Env form is ConnectionStrings__{name}.
    /// </summary>
    public static string? GetConnectionString(string name, IConfiguration configuration)
    {
        return GetValue("ConnectionStrings:" + name, configuration);
    }
}
