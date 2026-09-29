# FR Classifier

API ASP.NET Core 10 para importar linhas financeiras de uma exportacao XLSX do
SAP ACDOCA, manter a origem e encaminhar contas para um classificador externo por
RabbitMQ. A API nao referencia nem executa Laya; o classificador Python e outro
servico.

## Modelo financeiro

O importador procura os cabecalhos tecnicos SAP nas primeiras 50 linhas de cada
aba. Campos obrigatorios:

| ACDOCA | Uso |
| --- | --- |
| `RLDNR`, `RBUKRS`, `GJAHR`, `BELNR`, `DOCLN` | Identidade do ledger, empresa, exercicio, documento e linha |
| `RACCT` | Conta contabil |
| `BUDAT` | Data de lancamento |
| `WSL`, `RWCUR` | Valor e moeda da transacao |

Campos opcionais suportados: `BLDAT` (data do documento), `HSL`/`RHCUR`
(valor/moeda da empresa), `TXT50` (descricao da conta), `SGTXT` (texto da
linha), `PRCTR` (centro de lucro), `RCNTR` (centro de custo) e `SEGMENT`.
Outros campos ACDOCA nao sao persistidos. Cada linha tambem guarda documento de
origem, nome da aba, numero da linha e hash do conteudo; o XLSX original fica em
`App_Data/financial-documents` por padrao, fora de `wwwroot`.

## API

- `POST /api/documents` recebe multipart com o campo `file` e um `.xlsx`.
- `GET /api/documents/{id}` consulta status, hash e total de contas.
- `GET /api/documents/{id}/accounts?page=1&pageSize=100` lista contas paginadas.
- `GET /api/documents/{id}/content` recupera o arquivo original.
- `GET /api/accounts/{id}` consulta uma conta e sua classificacao mais recente.
- `GET /api/ifrs18-categories` lista os codigos de categoria aceitos.
- `POST /api/classifications/dispatch?limit=100` publica contas pendentes (limite maximo 500).

A migracao inicial esta em `Migrations/`. Aplique-a antes de usar os endpoints
que acessam o banco:

```sh
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update
dotnet run
```

Configure `ConnectionStrings__FinancialDatabase`, `RabbitMQ__Uri`,
`FinancialDocuments__StoragePath` e `FinancialDocuments__MaxUploadBytes` por
ambiente. Os valores de `appsettings.Development.json` sao apenas defaults
locais. Em producao, use credenciais gerenciadas e armazenamento compartilhado
duravel; o storage local serve para desenvolvimento e instancia unica.

## Contrato RabbitMQ

O ASP.NET publica JSON persistente na fila `financial.classification.requests`.
Cada mensagem inclui `requestId`, `financialAccountId`, `financialDocumentId`,
identificadores SAP, descricao, datas, valores, moedas e dimensoes de relatorio.
O servico Python deve responder na fila `financial.classification.results` com
`requestId`, `financialAccountId`, `category`, `confidence`, `rationale`,
`classifierName` e `modelVersion`. Codigos aceitos: `operating`, `investing`,
`financing`, `income_taxes` e `discontinued_operations`.

O resultado so recebe ack depois de persistido. Respostas invalidas ou para
requisicoes antigas vao para `financial.classification.results.dead`; falhas
transitorias de banco sao reencaminhadas. `RequestId` permite idempotencia. Esta
primeira versao nao usa outbox transacional: uma interrupcao entre marcar uma
conta como `Queued` e publicar pode exigir reconciliacao operacional.

As categorias sao as cinco categorias da demonstracao do resultado do IFRS 18.
O enquadramento real pode depender das atividades principais da entidade e de
outros fatos; a API preserva a classificacao e confianca recebidas, mas nao
substitui julgamento contabil.