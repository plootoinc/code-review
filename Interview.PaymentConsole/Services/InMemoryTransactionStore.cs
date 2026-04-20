namespace Interview.PaymentConsole.Services;

public class InMemoryTransactionStore : ITransactionStore
{
    private static readonly Dictionary<string, decimal> Transactions = new();

    public Task SaveAsync(string transactionId, decimal amount)
    {
        if (transactionId.StartsWith("FAIL-", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Intermittent persistence timeout.");
        }

        Transactions[transactionId] = amount;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string transactionId)
    {
        return Task.FromResult(Transactions.ContainsKey(transactionId));
    }

    public Task RemoveAsync(string transactionId)
    {
        if (Transactions.ContainsKey(transactionId))
        {
            Transactions.Remove(transactionId);
        }

        return Task.CompletedTask;
    }
}
