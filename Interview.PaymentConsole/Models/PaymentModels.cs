namespace Interview.PaymentConsole.Models;

public class ProcessTransactionRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string SourceAccountId { get; set; } = string.Empty;
}

public class RevertTransactionRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
}
