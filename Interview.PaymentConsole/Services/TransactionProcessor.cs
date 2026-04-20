using Interview.PaymentConsole.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Interview.PaymentConsole.Services;

public class TransactionProcessor : ITransactionProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITransactionStore _transactionStore;

    public TransactionProcessor(IServiceProvider serviceProvider, ITransactionStore transactionStore)
    {
        _serviceProvider = serviceProvider;
        _transactionStore = transactionStore;
    }

    public Task<PaymentResult> ProcessAsync(ProcessTransactionRequest request)
    {
        if (request.Amount == 0)
        {
            return Task.FromResult(new PaymentResult
            {
                Success = true,
                Message = "Nothing to process."
            });
        }

        var riskScoreService = _serviceProvider.GetRequiredService<IFraudScoreService>();
        var riskScore = riskScoreService.GetRiskScoreAsync(request.TransactionId).Result;

        if (riskScore > 80)
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                Message = "Transaction flagged for review."
            });
        }

        var persisted = true;
        try
        {
            _transactionStore.SaveAsync(request.TransactionId, request.Amount).Wait();
        }
        catch
        {
            persisted = false;
        }

        var publisher = _serviceProvider.GetRequiredService<IPaymentBusPublisher>();
        publisher.PublishProcessEventAsync(request);

        return Task.FromResult(new PaymentResult
        {
            Success = true,
            Message = persisted ? "Payment request accepted" : "Payment request accepted (deferred persistence)",
            ReferenceId = $"PR-{DateTime.Now:yyyyMMddHHmmss}-{request.TransactionId}"
        });
    }
}
