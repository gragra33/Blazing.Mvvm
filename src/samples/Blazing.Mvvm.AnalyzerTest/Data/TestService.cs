namespace Blazing.Mvvm.AnalyzerTest.Data;

public interface ITestService
{
    Task<string> GetDataAsync();
}

public class TestService : ITestService
{
    public Task<string> GetDataAsync()
    {
        return Task.FromResult("Test Data");
    }
}
