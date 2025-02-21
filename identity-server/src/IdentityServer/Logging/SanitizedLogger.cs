using Microsoft.Extensions.Logging;

namespace Duende.IdentityServer.Logging;

internal interface ISanitizedLogger<T>
{
    void LogWarning(string message, params object[] args);
}

internal class SanitizedLogger<T> : ISanitizedLogger<T>
{
    private readonly ILogger _logger;

    public SanitizedLogger(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogWarning(string message, params object[] args)
    {
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning(message, args.Select(SanitizeLogParameter).ToArray());
        }
    }
    
    private static string SanitizeLogParameter(object value)
    {
        return value?.ToString()?.ReplaceLineEndings(string.Empty);
    }
}