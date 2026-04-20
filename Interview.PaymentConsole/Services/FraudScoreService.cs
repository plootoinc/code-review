namespace Interview.PaymentConsole.Services;

public class FraudScoreService : IFraudScoreService
{
    public async Task<int> GetRiskScoreAsync(string transactionId)
    {
        var random = new Random();
        await Task.Delay(5);
        return random.Next(0, 100);
    }
}
