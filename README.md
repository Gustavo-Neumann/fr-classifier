# FR Classifier

API ASP.NET Core 10 para importar documentos financeiros, persistir contas e
encaminha-las por RabbitMQ a um classificador externo. O dominio usa os conceitos
genericos `Document`, `Account`, `AccountClassification` e `Category`; nao depende
de SAP nem referencia a biblioteca Python/Laya.

## Formatos de documento

`ImportDocumentService` seleciona uma implementacao de `IDocumentParser`. A
implementacao inicial, `AcdocaXlsxImporter`, e especifica para uma exportacao SAP
ACDOCA em XLSX. Ela converte as colunas de origem para o modelo canonico de
conta; nomes SAP nao viram colunas obrigatorias no dominio. Os campos de origem
que nao cabem no modelo comum ficam em `SourceReference` e `DimensionsJson`.

O importador atual exige `RLDNR`, `RBUKRS`, `GJAHR`, `BELNR`, `DOCLN`, `RACCT`,
`BUDAT`, `WSL` e `RWCUR`. Le opcionalmente `BLDAT`, `HSL`, `RHCUR`, `TXT50`,
`SGTXT`, `PRCTR`, `RCNTR` e `SEGMENT`. Um novo formato pode adicionar outro
`IDocumentParser` sem mudar as entidades ou o contrato do classificador.

O arquivo original e armazenado por padrao em `App_Data/financial-documents`,
fora de `wwwroot`; cada conta guarda documento, localizacao da linha e hash para
rastreabilidade. `Documents:StoragePath` pode apontar para outro caminho. O
adaptador atual usa disco local, adequado para desenvolvimento e instancia
unica; producao distribuida deve usar storage compartilhado.

## Swagger e API

Com a API em Development, a documentacao interativa fica em
`http://localhost:5118/swagger`.

- `POST /api/documents` recebe multipart com o campo `file` e um `.xlsx`.
- `GET /api/documents/{id}` consulta status, hash e total de contas.
- `GET /api/documents/{id}/accounts?page=1&pageSize=100` lista contas paginadas.
- `GET /api/documents/{id}/content` recupera o arquivo original.
- `GET /api/accounts/{id}` consulta uma conta e sua classificacao mais recente.
- `GET /api/ifrs18-categories` lista os codigos de categoria aceitos.
- `POST /api/classifications/dispatch?limit=100` publica contas pendentes (maximo 500).

## Banco de dados

As migrations ficam em `Data/Migrations/`. O EF Core normalmente agrupa em cada
migration uma alteracao coesa do modelo completo, em vez de criar uma migration
por entidade; a primeira migration cria as tabelas `documents`, `accounts` e
`account_classifications` juntas. Alteracoes futuras geram migrations incrementais.

```sh
cp .env.example .env
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update
dotnet run
```

O arquivo `.env` guarda os valores locais de desenvolvimento e não deve ser
versionado. Configure `DATABASE_HOST`, `DATABASE_PORT`, `DATABASE_NAME`,
`DATABASE_USER`, `DATABASE_PASSWORD`, `RABBITMQ_HOST`, `RABBITMQ_PORT`,
`RABBITMQ_USER` e `RABBITMQ_PASSWORD`; a aplicação monta a connection string
PostgreSQL e a URI AMQP a partir desses campos. O Compose define as mesmas
variáveis explicitamente para usar os nomes DNS `db` e `rabbitmq` na rede Docker.
`Documents__StoragePath` e `Documents__MaxUploadBytes` continuam configuráveis
por ambiente/appsettings.

## Contrato RabbitMQ

O ASP.NET publica JSON persistente na fila `financial.classification.requests`.
A mensagem usa o contrato canonico (`requestId`, `accountId`, `documentId`,
`entityCode`, `accountCode`, `description`, `amount`, `currencyCode` e dimensoes).
O servico externo responde na fila `financial.classification.results` com
`requestId`, `accountId`, `category`, `confidence`, `rationale`,
`classifierName` e `modelVersion`. Os codigos aceitos sao `operating`,
`investing`, `financing`, `income_taxes` e `discontinued_operations`.

O resultado so recebe ack depois de persistido. Mensagens invalidas ou de
requisicoes antigas vao para a fila dead-letter; falhas transitorias de banco
sao reencaminhadas. Esta primeira versao nao usa outbox transacional.