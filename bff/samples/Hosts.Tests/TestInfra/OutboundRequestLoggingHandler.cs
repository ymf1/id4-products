using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Hosts.Tests.TestInfra;

public class OutboundRequestLoggingHandler : DelegatingHandler
{
    private readonly ILogger<OutboundRequestLoggingHandler> _log;
    private readonly Func<HttpRequestMessage, bool> _shouldLog;

    public OutboundRequestLoggingHandler(ILogger<OutboundRequestLoggingHandler> log,
        Func<HttpRequestMessage, bool> shouldLog)
    {
        _log = log;
        _shouldLog = shouldLog;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!_shouldLog(request))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            _log.LogDebug("Started executing {method} on {url}",
                request.Method,
                request.RequestUri?.GetLeftPart(UriPartial.Scheme | UriPartial.Authority | UriPartial.Path));

            var result = await base.SendAsync(request, cancellationToken);

            LogLevel logLevel;

            if (stopwatch.Elapsed > TimeSpan.FromSeconds(1))
            {
                logLevel = LogLevel.Warning;
            }
            else if ((int)result.StatusCode >= 500)
            {
                logLevel = LogLevel.Warning;
            }
            else if ((int)result.StatusCode >= 400 || (int)result.StatusCode < 200)
            {
                logLevel = LogLevel.Information;
            }
            else
            {
                logLevel = LogLevel.Information;
            }

            _log.Log(logLevel, "Executing {method} on {url} returned {statuscode} in {ms} ms",
                request.Method,
                request.RequestUri,
                result.StatusCode,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (OperationCanceledException)
        {
            _log.LogWarning("Executing {method} on {url} was cancelled in {ms} ms",
                request.Method,
                request.RequestUri,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Exception while executing {method} on {url} in {ms} ms",
                request.Method,
                request.RequestUri,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}