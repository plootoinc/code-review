using Interview.PaymentConsole.Models;

namespace Interview.PaymentConsole.Services;

public interface ITransactionProcessor
{
    Task<PaymentResult> ProcessAsync(ProcessTransactionRequest request);
}

public interface ITransactionReverter
{
    Task<PaymentResult> RevertAsync(RevertTransactionRequest request);
}

public interface IPaymentBusPublisher
{
    Task PublishProcessEventAsync(ProcessTransactionRequest request);
    Task PublishRevertEventAsync(RevertTransactionRequest request);
}

public interface ITransactionStore
{
    Task SaveAsync(string transactionId, decimal amount);
    Task<bool> ExistsAsync(string transactionId);
    Task RemoveAsync(string transactionId);
}

public interface IFraudScoreService
{
    Task<int> GetRiskScoreAsync(string transactionId);
}
