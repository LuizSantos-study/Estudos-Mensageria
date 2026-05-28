# 📨 Estudos Mensageria — RabbitMQ com C#

Projeto de estudo sobre sistemas de mensageria utilizando **RabbitMQ** com **.NET 10 (C#)**, explorando os conceitos de producer, consumer, exchanges, filas e bindings.

---

## 🐇 O que é o RabbitMQ?

RabbitMQ é um **message broker** open source baseado no protocolo **AMQP (Advanced Message Queuing Protocol)**. Ele atua como intermediário entre aplicações que produzem mensagens (producers) e aplicações que as consomem (consumers), garantindo entrega, persistência e roteamento das mensagens.

### Conceitos fundamentais

| Conceito | Descrição |
|---|---|
| **Producer** | Aplicação que publica mensagens no exchange |
| **Consumer** | Aplicação que consome mensagens de uma fila |
| **Exchange** | Recebe as mensagens do producer e as roteia para as filas |
| **Queue (Fila)** | Armazena as mensagens até serem consumidas |
| **Binding** | Vínculo entre um exchange e uma fila, com uma routing key |
| **Routing Key** | Chave usada pelo exchange para decidir para qual fila enviar |

---

## 🏗️ Arquitetura do Projeto

```
Produtor
    │
    ▼
[TesteEx - Exchange Direct]
    │
    ├──(routing key: pagamento)──▶ [FilaTeste]  ──▶ Consumidor 1 ──▶ /consumos/*.txt
    │
    └──(routing key: pagamento)──▶ [FilaTeste2] ──▶ Consumidor 2 ──▶ /consumos/*.txt
```

---

## 📂 Estrutura do Projeto

```
RabbitMQ/
├── Produtor/
│   ├── Produtor.cs
│   └── Produtor.csproj
├── Consumidor/
│   ├── Consumidor.cs
│   ├── Consumidor.csproj
│   └── consumos/          ← arquivos gerados pelo consumidor
└── Consumidor2/
    ├── Consumidor2.cs
    ├── Consumidor2.csproj
    └── consumos/          ← arquivos gerados pelo consumidor 2
```

---

## 🔌 Tipos de Exchange

### Direct Exchange
Roteia mensagens para filas com base na **routing key exata**. Usado neste projeto.

```
Exchange (direct)
    ├── routing key "pagamento" ──▶ FilaTeste
    └── routing key "pagamento" ──▶ FilaTeste2
```

### Fanout Exchange
Ignora a routing key e envia para **todas as filas vinculadas**.

```
Exchange (fanout)
    ├── ──▶ FilaTeste
    ├── ──▶ FilaTeste2
    └── ──▶ FilaTeste3
```

### Topic Exchange
Usa **padrões com wildcards** na routing key para roteamento flexível.

```
routing key "pagamento.cartao"  ──▶ fila que escuta "pagamento.*"
routing key "pagamento.pix"     ──▶ fila que escuta "pagamento.*"
routing key "estoque.entrada"   ──▶ fila que escuta "estoque.#"
```

| Wildcard | Significado |
|---|---|
| `*` | Substitui exatamente uma palavra |
| `#` | Substitui zero ou mais palavras |

### Headers Exchange
Roteia com base nos **headers da mensagem** em vez da routing key.

---

## ⚙️ Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [VS Code](https://code.visualstudio.com/) com extensões C# Dev Kit e Docker

---

## 🐳 Subindo o RabbitMQ com Docker

```bash
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management
```

| Porta | Uso |
|---|---|
| `5672` | Broker AMQP — comunicação das aplicações |
| `15672` | Painel de gerenciamento web |

Acesse o painel em: [http://localhost:15672](http://localhost:15672)  
Usuário padrão: `guest` / Senha: `guest`

---

## 📦 Instalação das dependências

Em cada projeto, instale o pacote RabbitMQ.Client:

```bash
dotnet add package RabbitMQ.Client --source https://api.nuget.org/v3/index.json
```

---

## 🚀 Como rodar

### 1. Suba o RabbitMQ

```bash
docker start rabbitmq
```

### 2. Configure o Exchange e Bindings no painel

- Acesse `http://localhost:15672/#/exchanges`
- Crie o exchange `TesteEx` do tipo `direct`
- Crie as filas `FilaTeste` e `FilaTeste2`
- Faça o binding de cada fila ao exchange com a routing key `pagamento`

### 3. Suba os Consumidores primeiro

```bash
# Terminal 1
cd Consumidor
dotnet run

# Terminal 2
cd Consumidor2
dotnet run
```

### 4. Suba o Produtor

```bash
# Terminal 3
cd Produtor
dotnet run
```

### 5. Encerre com `Ctrl+C` em cada terminal

---

## 💾 Salvamento de mensagens consumidas

Cada consumidor salva as mensagens recebidas como arquivos `.txt` na pasta `consumos/`, com nome baseado no timestamp:

```
consumos/
├── mensagem_20260527_110454_619.txt
├── mensagem_20260527_110455_321.txt
└── ...
```

O nome garante unicidade mesmo em alto volume de mensagens (precisão de milissegundos).

---

## 🔑 Configuração de conexão

```csharp
var factory = new ConnectionFactory()
{
    HostName = "localhost",
    Port = 5672,          // porta AMQP (não confundir com 15672 do painel)
    UserName = "guest",
    Password = "guest"
};
```

---

## 📋 Configurações da Fila

| Propriedade | Valor | Descrição |
|---|---|---|
| `durable` | `true` | Fila persiste após restart do RabbitMQ |
| `exclusive` | `false` | Fila compartilhada entre conexões |
| `autoDelete` | `false` | Fila não é deletada quando consumers desconectam |
| `autoAck` | `false` | Confirmação manual após processamento |
| `prefetchCount` | `1` | Processa 1 mensagem por vez por consumer |

---

## 🔄 Fluxo de confirmação (ACK)

O projeto usa confirmação manual (`autoAck: false`), garantindo que a mensagem só seja removida da fila após o processamento bem-sucedido:

```csharp
// Confirma o processamento
await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
```

Se o consumer falhar antes do ACK, a mensagem volta para a fila automaticamente.

---

## 🛠️ Comandos úteis

```bash
# Ver containers rodando
docker ps

# Logs do RabbitMQ
docker logs rabbitmq

# Parar o RabbitMQ
docker stop rabbitmq

# Iniciar o RabbitMQ
docker start rabbitmq

# Compilar o projeto
dotnet build

# Restaurar pacotes
dotnet restore
```

---

## 📚 Referências

- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [RabbitMQ .NET Client](https://www.rabbitmq.com/client-libraries/dotnet)
- [AMQP Protocol](https://www.amqp.org/)
