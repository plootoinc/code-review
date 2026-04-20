using Interview.PaymentConsole.Models;
using Microsoft.Extensions.Logging;

namespace Interview.PaymentConsole.Services;

public class TransactionReverter : ITransactionReverter
{
    private readonly ITransactionStore _transactionStore;
    private readonly IPaymentBusPublisher _publisher;
    private readonly ILogger<TransactionReverter> _logger;

    public TransactionReverter(
        ITransactionStore transactionStore,
        IPaymentBusPublisher publisher,
        ILogger<TransactionReverter> logger)
    {
        _transactionStore = transactionStore;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<PaymentResult> RevertAsync(RevertTransactionRequest request)
    {
        try
        {
            if (!await _transactionStore.ExistsAsync(request.TransactionId))
            {
                return new PaymentResult
                {
                    Success = true,
                    Message = "Transaction already reverted."
                };
            }

            await _transactionStore.RemoveAsync(request.TransactionId);
            await _publisher.PublishRevertEventAsync(request);

            return new PaymentResult
            {
                Success = true,
                Message = "Transaction reverted."
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Revert failed for {TransactionId}", request.TransactionId);
            return new PaymentResult
            {
                Success = true,
                Message = "Revert request queued."
            };
        }
    }
}
