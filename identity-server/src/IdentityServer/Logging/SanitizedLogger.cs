// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Microsoft.Extensions.Logging;

namespace Duende.IdentityServer.Logging;

internal interface ISanitizedLogger
{
    void LogTrace(string message, params object[] args);
    void LogDebug(string message, params object[] args);
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogCritical(Exception exception, string message, params object[] args);
    ILogger Logger { get; }
}

internal interface ISanitizedLogger<T> : ISanitizedLogger
{
}

internal class SanitizedLogger<T> : ISanitizedLogger<T>
{
    public SanitizedLogger(ILogger<T> logger)
    {
        Logger = logger;
    }
    
    public SanitizedLogger(ILogger logger)
    {
        Logger = logger;
    }
    
    public void LogTrace(string message, params object[] args)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace(message, args.Select(SanitizeLogParameter).ToArray());
        }
    }
    
    public void LogDebug(string message, params object[] args)
    {
        if (Logger.IsEnabled(LogLevel.Debug))
        {
            LoggerExtensions.LogDebug(Logger, message, args.Select(SanitizeLogParameter).ToArray());
        }
    }
    
    public void LogInformation(string message, params object[] args)
    {
        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation(message, args.Select(SanitizeLogParameter).ToArray());
        }
    }

    public void LogWarning(string message, params object[] args)
    {
        if (Logger.IsEnabled(LogLevel.Warning))
        {
            Logger.LogWarning(message, args.Select(SanitizeLogParameter).ToArray());
        }
    }
    
    public void LogError(string message, params object[] args)
    {
        if (Logger.IsEnabled(LogLevel.Error))
        {
            Logger.LogError(message, args.Select(SanitizeLogParameter).ToArray());
        }
    }

    public void LogCritical(Exception exception, string message, params object[] args)
    {
        if (Logger.IsEnabled(LogLevel.Critical))
        {
            Logger.LogCritical(exception, message, args.Select(SanitizeLogParameter).ToArray());
        }
    }

    public ILogger Logger { get; }

    private static object SanitizeLogParameter(object value)
    {
        return value?.GetType() == typeof(string) ? value.ToString()?.ReplaceLineEndings(string.Empty) : value;
    }
}