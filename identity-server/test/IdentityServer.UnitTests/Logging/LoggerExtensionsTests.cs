using Duende.IdentityServer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace IdentityServer.UnitTests.Logging;

public class LoggerExtensionsTests
{
    private readonly FakeLogger _logger = new();

    [Fact]
    public void LogSanitizedDebugWithSingleParam_does_not_log_message_when_debug_not_enabled()
    {
        _logger.ControlLevel(LogLevel.Debug, false);
        
        _logger.LogSanitizedDebug("This should not log anything {input}", "nope");
        
        _logger.Collector.Count.ShouldBe(0);
    }

    [Fact]
    public void LogSanitizedDebugWithSingleParam_logs_sanitized_message_when_debug_enabled()
    {
        _logger.ControlLevel(LogLevel.Debug, true);
        
        _logger.LogSanitizedDebug("This should not have newlines {input}", $"testing{Environment.NewLine} newlines");

        _logger.LatestRecord.Message.ShouldBe("This should not have newlines testing newlines");
    }
    
    [Fact]
    public void LogSanitizedDebugWithTwoParams_does_not_log_message_when_debug_not_enabled()
    {
        _logger.ControlLevel(LogLevel.Debug, false);
        
        _logger.LogSanitizedDebug("This should not log anything {first}:{second}", "no", "logs");
        
        _logger.Collector.Count.ShouldBe(0);
    }

    [Fact]
    public void LogSanitizedDebugWithTwoParams_logs_sanitized_message_when_debug_enabled()
    {
        _logger.ControlLevel(LogLevel.Debug, true);
        
        _logger.LogSanitizedDebug("This should not have newlines {first}:{second}", $"testing{Environment.NewLine} newlines", $"and more{Environment.NewLine} newlines");

        _logger.LatestRecord.Message.ShouldBe("This should not have newlines testing newlines:and more newlines");
    }
    
    [Fact]
    public void LogSanitizedWarningWithSingleParam_does_not_log_message_when_debug_not_enabled()
    {
        _logger.ControlLevel(LogLevel.Warning, false);
        
        _logger.LogSanitizedWarning("This should not log anything {input}", "nope");
        
        _logger.Collector.Count.ShouldBe(0);
    }

    [Fact]
    public void LogSanitizedWarningWithSingleParam_logs_sanitized_message_when_debug_enabled()
    {
        _logger.ControlLevel(LogLevel.Warning, true);
        
        _logger.LogSanitizedWarning("This should not have newlines {input}", $"testing{Environment.NewLine} newlines");

        _logger.LatestRecord.Message.ShouldBe("This should not have newlines testing newlines");
    }
    
    [Fact]
    public void LogSanitizedErrorWithTwoParams_does_not_log_message_when_debug_not_enabled()
    {
        _logger.ControlLevel(LogLevel.Error, false);
        
        _logger.LogSanitizedError("This should not log anything {first}:{second}", "no", "logs");
        
        _logger.Collector.Count.ShouldBe(0);
    }

    [Fact]
    public void LogSanitizedErrorWithTwoParams_logs_sanitized_message_when_debug_enabled()
    {
        _logger.ControlLevel(LogLevel.Error, true);
        
        _logger.LogSanitizedError("This should not have newlines {first}:{second}", $"testing{Environment.NewLine} newlines", $"and more{Environment.NewLine} newlines");

        _logger.LatestRecord.Message.ShouldBe("This should not have newlines testing newlines:and more newlines");
    }
}