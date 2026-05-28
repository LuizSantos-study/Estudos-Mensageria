using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "FilaTeste",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("\n[!] Encerrando produtor...");
};

Console.WriteLine("Produtor iniciado! Pressione Ctrl+C para parar.");

int contador = 1;
while (!cts.Token.IsCancellationRequested)
{
    var mensagem = $"Hello World#{contador++} - {DateTime.Now:HH:mm:ss}";
    var body = Encoding.UTF8.GetBytes(mensagem);

    await channel.BasicPublishAsync(
        exchange: "TesteEx",
        routingKey: "pagamento",
        body: body
    );

    Console.WriteLine($"[->] Enviada: {mensagem}");

    try { await Task.Delay(1000, cts.Token); }
    catch (TaskCanceledException) { break; }
}

Console.WriteLine("[OK] Produtor encerrado.");
