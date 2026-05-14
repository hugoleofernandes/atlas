# Arquitetura Enterprise para Worker Service de Outbox Pattern Excelente projeto! Vou criar uma solução **production-ready** seguindo os padrões que você já estabeleceu (Clean Architecture, DDD, Modular Monolith). Vou focar em uma arquitetura escalável, testável e extensível. ## 📐 PARTE 1: ARQUITETURA E DESIGN ### 1. VISÃO GERAL DA ARQUITETURA
┌─────────────────────────────────────────────────────────────────┐
│                        WORKER SERVICE                            │
│  (BackgroundService - Hosted Service)                           │
└───────────────────────┬─────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────────┐
│                   OUTBOX PROCESSOR                              │
│  - Polling Strategy                                             │
│  - Batch Processing                                             │
│  - Locking Coordination                                         │
│  - Retry Orchestration                                          │
└───────────────────────┬─────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────────┐
│                  MESSAGE DISPATCHER                             │
│  - Type Resolution                                              │
│  - Handler Discovery                                            │
│  - Concurrent Execution                                         │
│  - Error Handling                                               │
└───────────────────────┬─────────────────────────────────────────┘
│
┌───────┴────────┐
▼                ▼
┌──────────────────┐  ┌──────────────────┐
│  DOMAIN HANDLER  │  │ TRANSPORT HANDLER│
│  (Business Logic)│  │ (Email/Queue/etc)│
└──────────────────┘  └──────────────────┘
--- ## 2. ESTRUTURA DE PASTAS (Clean Architecture)
Atlas.BackgroundWorkers/
├── Atlas.BackgroundWorkers.OutboxProcessor/           # Worker Service Project
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Dockerfile
│   └── Workers/
│       └── OutboxProcessorWorker.cs                   # BackgroundService
│
├── Atlas.BackgroundWorkers.Application/               # Application Layer
│   ├── Abstractions/
│   │   ├── IOutboxMessageHandler.cs                   # Handler contract
│   │   ├── IOutboxProcessor.cs                        # Processor contract
│   │   ├── IMessageDispatcher.cs                      # Dispatcher contract
│   │   └── IOutboxRepository.cs                       # Repository contract
│   │
│   ├── Configuration/
│   │   ├── OutboxProcessorOptions.cs                  # Configuration model
│   │   └── RetryPolicy.cs                             # Retry configuration
│   │
│   ├── Services/
│   │   ├── OutboxProcessor.cs                         # Main processor logic
│   │   ├── MessageDispatcher.cs                       # Message routing
│   │   └── HandlerRegistry.cs                         # Handler registration
│   │
│   └── Handlers/
│       ├── Base/
│       │   └── OutboxMessageHandler.cs                # Base handler
│       └── Examples/
│           ├── UserCreatedHandler.cs
│           └── OrderPlacedHandler.cs
│
├── Atlas.BackgroundWorkers.Infrastructure/            # Infrastructure Layer
│   ├── Persistence/
│   │   ├── OutboxRepository.cs                        # EF implementation
│   │   └── Queries/
│   │       └── OutboxQueries.cs                       # Optimized queries
│   │
│   ├── Messaging/
│   │   ├── Dispatchers/
│   │   │   ├── RabbitMqDispatcher.cs                  # Future: RabbitMQ
│   │   │   ├── EmailDispatcher.cs                     # Future: Email
│   │   │   └── WebhookDispatcher.cs                   # Future: Webhooks
│   │   │
│   │   └── Serialization/
│   │       └── MessageSerializer.cs                   # JSON serialization
│   │
│   └── DependencyInjection.cs                         # DI registration
│
└── Atlas.BackgroundWorkers.Contracts/                 # Shared contracts
└── Events/
├── IIntegrationEvent.cs
├── UserCreatedEvent.cs
└── OrderPlacedEvent.cs
--- ## 3. DECISÕES ARQUITETURAIS E TRADEOFFS ### 3.1 **Polling vs Push (Trigger-based)** **✅ ESCOLHA: Polling** **Motivos:** - ✅ Simples e confiável - ✅ Não precisa de triggers no banco - ✅ Fácil de escalar horizontalmente - ✅ Funciona bem com PostgreSQL - ✅ Testável **Tradeoffs:** - ⚠️ Latência (configurável via intervalo) - ⚠️ Carga constante no DB (mitigado com índices + batch size) **Configuração recomendada:** - **High throughput**: 1-5 segundos - **Low latency**: 500ms - 1s - **Standard**: 10-30 segundos --- ### 3.2 **Optimistic Locking vs Pessimistic Locking** **✅ ESCOLHA: Optimistic Locking com GUID** **Implementação:**
sql
-- Query com lock otimista
UPDATE outbox_messages
SET lock_id = @lockId,
locked_until = @lockedUntil
WHERE id = ANY(@ids)
AND (locked_until IS NULL OR locked_until < NOW())
AND processed_on IS NULL
AND dead_lettered_on IS NULL
RETURNING *;
**Motivos:** - ✅ Múltiplas instâncias do worker podem rodar simultaneamente - ✅ Sem bloqueios de banco - ✅ Alta performance - ✅ Baixa contenção **Alternativa rejeitada (Skip Locked):**
sql
-- NÃO USAR - cria dependência de transaction longa
SELECT * FROM outbox_messages
WHERE processed_on IS NULL
FOR UPDATE SKIP LOCKED;
--- ### 3.3 **Handler Resolution Strategy** **✅ ESCOLHA: Registry Pattern + DI** **Motivos:** - ✅ Sem reflection em runtime - ✅ Validação em startup - ✅ Fácil de testar - ✅ Type-safe - ✅ Performance previsível **Evitamos:** - ❌ Assembly.GetTypes() em runtime - ❌ Switch gigante por Type - ❌ Reflection pesada por mensagem --- ### 3.4 **Retry Strategy** **✅ ESCOLHA: Exponential Backoff com Jitter**
Retry 1: ~2 seconds
Retry 2: ~4 seconds
Retry 3: ~8 seconds
Retry 4: ~16 seconds
Retry 5: ~32 seconds (dead letter)
**Motivos:** - ✅ Evita thundering herd - ✅ Dá tempo para sistemas downstream recuperarem - ✅ Jitter evita sincronização de retries --- ### 3.5 **Batch Processing** **✅ ESCOLHA: Batch de 50-100 mensagens** **Estratégia:** 1. Buscar batch do banco (1 query) 2. Aplicar lock em batch (1 query) 3. Processar em paralelo (até MaxDegreeOfParallelism) 4. Atualizar status (bulk update) **Configuração recomendada:** - **BatchSize**: 50-100 - **MaxDegreeOfParallelism**: 4-10 (depende da carga) - **Lock Duration**: 5 minutos --- ## 4. CONTRATOS E INTERFACES Vou definir as interfaces principais: ### 4.1 **IOutboxMessageHandler**
csharp
namespace Atlas.BackgroundWorkers.Application.Abstractions;

/// <summary>
/// Base contract for handling outbox messages.
/// Implementations should be stateless and registered as scoped services.
/// </summary>
public interface IOutboxMessageHandler
{
/// <summary>
/// Returns the event type this handler processes (e.g., "UserCreatedEvent")
/// </summary>
string EventType { get; }

    /// <summary>
    /// Handles the deserialized event.
    /// Should be idempotent - may be called multiple times for the same message.
    /// </summary>
    /// <returns>Result with success/failure indication</returns>
    Task<HandlerResult> HandleAsync(
        object eventData, 
        MessageContext context, 
        CancellationToken ct);
}
### 4.2 **IOutboxProcessor**
csharp
/// <summary>
/// Orchestrates the entire outbox processing cycle.
/// Responsible for polling, locking, dispatching, and error handling.
/// </summary>
public interface IOutboxProcessor
{
/// <summary>
/// Processes a single batch of pending messages.
/// Returns the number of messages successfully processed.
/// </summary>
Task<ProcessingResult> ProcessBatchAsync(CancellationToken ct);
}
### 4.3 **IMessageDispatcher**
csharp
/// <summary>
/// Resolves and invokes the appropriate handler for each message type.
/// Handles deserialization, error handling, and retry logic.
/// </summary>
public interface IMessageDispatcher
{
/// <summary>
/// Dispatches a single message to its handler.
/// </summary>
Task<DispatchResult> DispatchAsync(
OutboxMessage message,
CancellationToken ct);

    /// <summary>
    /// Dispatches multiple messages concurrently.
    /// </summary>
    Task<IReadOnlyList<DispatchResult>> DispatchBatchAsync(
        IReadOnlyList<OutboxMessage> messages, 
        CancellationToken ct);
}
### 4.4 **IOutboxRepository**
csharp
/// <summary>
/// Repository for outbox message persistence operations.
/// All queries should be optimized with proper indexes.
/// </summary>
public interface IOutboxRepository
{
/// <summary>
/// Fetches pending messages and locks them atomically.
/// Returns only successfully locked messages.
/// </summary>
Task<IReadOnlyList<OutboxMessage>> FetchAndLockBatchAsync(
Guid lockId,
int batchSize,
TimeSpan lockDuration,
int maxRetries,
CancellationToken ct);

    /// <summary>
    /// Marks message as successfully processed.
    /// </summary>
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken ct);
    
    /// <summary>
    /// Increments retry count and stores error.
    /// </summary>
    Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken ct);
    
    /// <summary>
    /// Moves message to dead letter after max retries exceeded.
    /// </summary>
    Task MarkAsDeadLetteredAsync(Guid messageId, CancellationToken ct);
}
--- ## 5. MODELOS DE CONFIGURAÇÃO
csharp
namespace Atlas.BackgroundWorkers.Application.Configuration;

public sealed class OutboxProcessorOptions
{
public const string SectionName = "OutboxProcessor";

    /// <summary>
    /// Interval between polling cycles (milliseconds)
    /// Recommended: 1000-30000
    /// </summary>
    public int PollingIntervalMs { get; set; } = 10_000;
    
    /// <summary>
    /// Number of messages to fetch per batch
    /// Recommended: 50-100
    /// </summary>
    public int BatchSize { get; set; } = 50;
    
    /// <summary>
    /// Maximum concurrent message processing
    /// Recommended: 4-10
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 5;
    
    /// <summary>
    /// Duration for which a message is locked (minutes)
    /// Recommended: 5-10
    /// </summary>
    public int LockDurationMinutes { get; set; } = 5;
    
    /// <summary>
    /// Maximum retry attempts before dead lettering
    /// Recommended: 3-5
    /// </summary>
    public int MaxRetries { get; set; } = 5;
    
    /// <summary>
    /// Enable exponential backoff for retries
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
    
    /// <summary>
    /// Base delay for exponential backoff (seconds)
    /// </summary>
    public int BackoffBaseSeconds { get; set; } = 2;
    
    /// <summary>
    /// Enable processing (allows disabling without stopping service)
    /// </summary>
    public bool Enabled { get; set; } = true;
}
--- ## 6. FLUXO COMPLETO DO SISTEMA ### 6.1 **Fluxo Principal (Happy Path)**
1. [WORKER] Inicia ciclo a cada X segundos
   ↓
2. [PROCESSOR] Busca batch de mensagens pendentes
   (WHERE processed_on IS NULL AND dead_lettered_on IS NULL)
   ↓
3. [PROCESSOR] Aplica lock otimista
   (UPDATE SET lock_id = GUID, locked_until = NOW + 5min)
   ↓
4. [PROCESSOR] Envia mensagens para Dispatcher
   ↓
5. [DISPATCHER] Para cada mensagem:
   - Deserializa payload
   - Resolve handler pelo Type
   - Invoca handler
   ↓
6. [HANDLER] Executa lógica de negócio
   (Envia email, publica em fila, etc)
   ↓
7. [PROCESSOR] Marca como processada
   (UPDATE SET processed_on = NOW, lock_id = NULL)
   ↓
8. [WORKER] Aguarda próximo ciclo
### 6.2 **Fluxo de Retry (Failure)**
1. [HANDLER] Lança exceção
   ↓
2. [DISPATCHER] Captura erro
   ↓
3. [PROCESSOR] Incrementa retry_count
   Salva mensagem de erro
   Remove lock
   ↓
4. [PROCESSOR] Calcula próximo retry (exponential backoff)
   ↓
5. [WORKER] Próximo ciclo irá reprocessar
   (se retry_count < max_retries)
   ↓
6. Se max_retries atingido:
   → Marca como dead_lettered
   → Move para análise manual
### 6.3 **Fluxo Multi-Worker (Race Condition Prevention)**
Worker A                    Worker B
|                           |
├─ Busca mensagens 1-50     ├─ Busca mensagens 1-50
|                           |
├─ Tenta lock (GUID_A)      ├─ Tenta lock (GUID_B)
|                           |
├─ ✅ Lock adquirido (1-50) |
|                           ├─ ⚠️  Lock falha (já locked)
|                           |
├─ Processa mensagens       ├─ Busca próximo batch (51-100)
|                           |
|                           ├─ ✅ Lock adquirido (51-100)
|                           |
├─ Finaliza batch           ├─ Processa mensagens
--- ## 7. ESTRATÉGIAS DE ESCALABILIDADE ### 7.1 **Horizontal Scaling** ✅ **Múltiplas instâncias do worker podem rodar simultaneamente** **Como funciona:** - Cada instância gera seu próprio lockId (GUID único) - Lock otimista garante que cada mensagem é processada por apenas 1 instância - Sem coordenação externa necessária (Redis, etc) **Configuração recomendada:** - **1-3 instâncias**: Para workloads normais - **5-10 instâncias**: Para alto volume - **10+ instâncias**: Considere particionamento por TenantId ou Module ### 7.2 **Particionamento (Sharding)** Para **volumes extremos** (100k+ msgs/min), considere:
csharp
// Particionamento por Tenant
WHERE tenant_id = @specificTenant
AND processed_on IS NULL

// Particionamento por Module
WHERE module = @specificModule
AND processed_on IS NULL
Cada worker processa apenas seu "shard". --- ## 8. ÍNDICES ESSENCIAIS DO BANCO
sql
-- Índice principal para busca de mensagens pendentes
CREATE INDEX ix_outbox_pending_messages
ON outbox_messages (occurred_on)
WHERE processed_on IS NULL
AND dead_lettered_on IS NULL
AND (locked_until IS NULL OR locked_until < NOW());

-- Índice para queries por tenant
CREATE INDEX ix_outbox_tenant_module
ON outbox_messages (tenant_id, module, occurred_on)
WHERE processed_on IS NULL;

-- Índice para monitoramento de dead letters
CREATE INDEX ix_outbox_dead_lettered
ON outbox_messages (dead_lettered_on)
WHERE dead_lettered_on IS NOT NULL;
--- ## 9. MONITORAMENTO E OBSERVABILIDADE ### Métricas importantes:
csharp
// Prometheus/OpenTelemetry metrics
- outbox_messages_processed_total (counter)
- outbox_messages_failed_total (counter)
- outbox_messages_dead_lettered_total (counter)
- outbox_processing_duration_seconds (histogram)
- outbox_batch_size (gauge)
- outbox_pending_messages (gauge)
- outbox_retry_count_distribution (histogram)
### Logs estruturados:
csharp
_logger.LogInformation(
"Outbox batch processed. " +
"Processed={Processed}, Failed={Failed}, Duration={Duration}ms",
result.Processed,
result.Failed,
result.Duration);
--- ## 10. RESUMO DE BOAS PRÁTICAS ✅ **DO:** - Use optimistic locking com GUID único por worker - Processe em batches (50-100 mensagens) - Use exponential backoff com jitter - Implemente dead letter após N tentativas - Use índices parciais no PostgreSQL - Mantenha handlers **idempotentes** - Use scoped lifetime para handlers (fresh DbContext por batch) - Implemente circuit breaker para sistemas downstream - Monitore métricas e logs estruturados ❌ **DON'T:** - Não use reflection pesada por mensagem - Não mantenha transactions longas - 


roposta de Faseamento
Aqui está uma sugestão de fases progressivas:
Fase 1: Fundação e Contratos ✅ Menor risco, base sólida
Criar estrutura de projetos (4 projetos mencionados)
Definir todas as interfaces (IOutboxMessageHandler, IOutboxProcessor, IMessageDispatcher, IOutboxRepository)
Criar modelos de configuração (OutboxProcessorOptions)
Criar modelos de resultado (HandlerResult, ProcessingResult, DispatchResult)
Entregável: Contratos compilando, sem implementação
Fase 2: Infraestrutura de Persistência 🔧
Implementar OutboxRepository com Entity Framework
Criar queries otimizadas (FetchAndLockBatchAsync)
Adicionar índices no banco (migration)
Testar locking otimista isoladamente
Entregável: Repository funcional com testes unitários
Fase 3: Message Dispatcher e Handler Registry 🎯
Implementar HandlerRegistry (resolução de handlers)
Implementar MessageDispatcher (deserialização + invocação)
Criar um handler de exemplo simples (UserCreatedHandler)
Entregável: Dispatch funcionando para 1 tipo de evento
Fase 4: Outbox Processor (Lógica Principal) ⚙️
Implementar OutboxProcessor
Adicionar lógica de retry com exponential backoff
Implementar dead letter
Adicionar tratamento de erros
Entregável: Processamento completo de batch
Fase 5: Worker Service 🚀
Criar BackgroundService (OutboxProcessorWorker)
Configurar DI completo
Adicionar configurações (appsettings.json)
Entregável: Worker rodando end-to-end
Fase 6: Observabilidade e Refinamentos 📊
Adicionar logs estruturados
Adicionar métricas (opcional: OpenTelemetry)
Testes de integração
Dockerfile
Entregável: Sistema production-ready

🎯 Recomendação
Começar pela Fase 1 porque:
✅ Sem dependências externas
✅ Valida a estrutura geral
✅ Fácil de revisar
✅ Base para as próximas fases

❓ Qual caminho você prefere?
Faseado (recomendado): Começo pela Fase 1 agora mesmo
Fases customizadas: Você quer ajustar as fases primeiro
All-in: Quer que eu peça para executar tudo de uma vez (mais arriscado)