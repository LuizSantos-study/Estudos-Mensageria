using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

// Declara a mesma fila do produtor
await channel.QueueDeclareAsync(
    queue: "FilaTeste2",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

// Processa 1 mensagem por vez
await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

var pastaConsumos = @"C:\Users\luiz.santos\Desktop\Luiz Santos\Dev\RabbitMQ\Consumidor2\consumos";

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var mensagem = Encoding.UTF8.GetString(body);

    Console.WriteLine($"[←] Recebida: {mensagem}");

    var nomeArquivo = $"mensagem_{DateTime.Now:yyyyMMdd_HHmmss_fff}.txt";
    var caminhoArquivo = Path.Combine(pastaConsumos, nomeArquivo);

    await File.WriteAllTextAsync(caminhoArquivo, mensagem);
    Console.WriteLine($"[] Salvo em: {caminhoArquivo}");

    // Simula processamento
    await Task.Delay(500);

    // Confirma que processou
    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
};

await channel.BasicConsumeAsync(
    queue: "FilaTeste2",
    autoAck: false,   // confirma manualmente
    consumer: consumer
);

// Mantém rodando até Ctrl+C
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("\n[!] Encerrando consumidor...");
};

Console.WriteLine("Consumidor aguardando mensagens... Pressione Ctrl+C para parar.\n");

try { await Task.Delay(Timeout.Infinite, cts.Token); }
catch (TaskCanceledException) { }

Console.WriteLine("[OK] Consumidor encerrado.");