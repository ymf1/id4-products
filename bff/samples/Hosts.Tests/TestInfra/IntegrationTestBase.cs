using Xunit.Abstractions;
using SkipException = Xunit.Sdk.SkipException;

namespace Hosts.Tests.TestInfra;

public class IntegrationTestBase : IClassFixture<AppHostFixture>, IDisposable
{
    private readonly IDisposable _loggingScope;
    private readonly ITestOutputHelper _output;
    private readonly AppHostFixture _fixture;

    public IntegrationTestBase(ITestOutputHelper output, AppHostFixture fixture)
    {
        _output = output;
        _fixture = fixture;
        _loggingScope = fixture.ConnectLogger(output.WriteLine);
        if (_fixture.UsingAlreadyRunningInstance)
        {
            output.WriteLine("Running tests against locally running instance");
        }
        else
        {
#if DEBUG_NCRUNCH
            Skip.If(true, "already attached");
#endif

        }
    }

    public AppHostFixture Fixture => _fixture;
    public ITestOutputHelper Output => _output;

    public HttpClient CreateHttpClient(string clientName) => _fixture.CreateHttpClient(clientName);

    public void Dispose()
    {
        if (!_fixture.UsingAlreadyRunningInstance)
        {
            _output.WriteLine(Environment.NewLine);
            _output.WriteLine(Environment.NewLine);
            _output.WriteLine(Environment.NewLine);
            _output.WriteLine("*************************************************");
            _output.WriteLine("** Startup logs ***");
            _output.WriteLine("*************************************************");
            _output.WriteLine(_fixture.StartupLogs);
        }

        _loggingScope.Dispose();
    }
}