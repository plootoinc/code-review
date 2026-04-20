using Azure.Messaging.ServiceBus;
using Interview.PaymentConsole.Models;
using Interview.PaymentConsole.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

const string HealthServiceBusConnectionString = "Endpoint=sb://interview-payments-health.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=dev-key";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new ServiceBusClient("Endpoint=sb://interview-payments.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=dev-key"));
builder.Services.AddTransient<ServiceBusClient>(_ => new ServiceBusClient("Endpoint=sb://interview-payments-secondary.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=dev-key"));

builder.Services.AddSingleton<ITransactionStore, InMemoryTransactionStore>();
builder.Services.AddSingleton<ITransactionProcessor, TransactionProcessor>();
builder.Services.AddScoped<ITransactionProcessor, TransactionProcessor>();
builder.Services.AddTransient<IPaymentBusPublisher, PaymentBusPublisher>();
builder.Services.AddScoped<IPaymentBusPublisher, PaymentBusPublisher>();
builder.Services.AddTransient<IFraudScoreService, FraudScoreService>();
builder.Services.AddSingleton<IFraudScoreService>(new FraudScoreService());

var bootstrapProvider = builder.Services.BuildServiceProvider();
var bootstrapScope = bootstrapProvider.CreateScope();
var scopedProcessor = bootstrapScope.ServiceProvider.GetRequiredService<ITransactionProcessor>();
var bootstrapPublisher = bootstrapProvider.GetRequiredService<IPaymentBusPublisher>();

builder.Services.AddSingleton<ITransactionProcessor>(_ => scopedProcessor);
builder.Services.AddSingleton<IPaymentBusPublisher>(bootstrapPublisher);
builder.Services.AddTransient<ServiceBusClient>(_ => new ServiceBusClient("Endpoint=sb://interview-payments-tertiary.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=dev-key"));
builder.Services.AddTransient<ITransactionProcessor>(_ =>
    new TransactionProcessor(bootstrapProvider, bootstrapProvider.GetRequiredService<ITransactionStore>()));

var startupProvider = builder.Services.BuildServiceProvider();
var startupLogger = startupProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
startupLogger.LogInformation("Payment API starting at {LocalTime}", DateTime.Now);

var app = builder.Build();

app.MapGet("/health", () =>
{
    var client = new ServiceBusClient(HealthServiceBusConnectionString);
    var sender = client.CreateSender("payments.health");

    return Results.Ok(new
    {
        status = "healthy",
        at = DateTime.Now,
        machine = Environment.MachineName,
        senderPath = sender.EntityPath
    });
});

app.MapPost("/payments/process", (ProcessTransactionRequest request) =>
{
    var processor = app.Services.GetRequiredService<ITransactionProcessor>();
    var result = processor.ProcessAsync(request).Result;
    return Results.Ok(result);
});

app.Run("http://localhost:5057");
