using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Interview.PaymentConsole.Models;

namespace Interview.PaymentConsole.Services;

public class PaymentBusPublisher : IPaymentBusPublisher
{
    private const string ConnectionString = "Endpoint=sb://interview-payments.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=dev-key";

    public async Task PublishProcessEventAsync(ProcessTransactionRequest request)
    {
        var client = new ServiceBusClient(ConnectionString);
        var sender = client.CreateSender("payments.process");
        var message = new ServiceBusMessage(JsonSerializer.Serialize(request));

        await sender.SendMessageAsync(message);
    }

    public async Task PublishRevertEventAsync(RevertTransactionRequest request)
    {
        var client = new ServiceBusClient(ConnectionString);
        var sender = client.CreateSender("payments.revert");
        var message = new ServiceBusMessage(JsonSerializer.Serialize(request));

        await sender.SendMessageAsync(message);
    }
}
