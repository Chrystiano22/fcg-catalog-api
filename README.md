# FCG CatalogAPI

Microsservico responsavel pelo catalogo de jogos, promocoes, biblioteca do usuario e inicio do fluxo de compra.

## Responsabilidades

- CRUD de jogos.
- Consulta de catalogo.
- Criacao e consulta de promocoes.
- Consulta da biblioteca do usuario.
- Publicacao de `OrderPlacedEvent` via log local ou RabbitMQ/MassTransit.
- Consumo de `PaymentProcessedEvent` via RabbitMQ/MassTransit quando habilitado.

## Endpoints

- `GET /jogos`
- `POST /jogos`
- `PUT /jogos/{gameId}`
- `DELETE /jogos/{gameId}`
- `GET /promocoes`
- `POST /promocoes`
- `GET /biblioteca`
- `POST /compras`
- `GET /health`

## Busca full-text (Fase 4)

`GET /jogos/busca?q=aventura&categoria=RPG&precoMin=10&precoMax=100&pagina=1&tamanho=20`

Requer JWT. Retorna `total`, `pagina`, `tamanho` e `itens`. Busca no titulo (peso 3),
descricao e categoria, com analisador portugues e tolerancia a erros (`AUTO`).
Filtros de preco sao inclusivos; somente jogos ativos aparecem. Ordenacao por relevancia e ID.
Pagina inicia em 1, tamanho entre 1 e 100 e janela maxima de 10.000 resultados.
Filtros invalidos retornam 400; Elasticsearch indisponivel ou desabilitado retorna 503.

Configuracao: `Elasticsearch__Enabled=true`, `Elasticsearch__Url=http://localhost:9200`,
`Elasticsearch__IndexName=fcg-games-v1`, `Elasticsearch__SyncIntervalSeconds=10`.
Para um cluster autenticado, configure `Elasticsearch__ApiKey` e HTTPS.

SQLite e a fonte transacional; Elasticsearch e uma projecao de consulta. Um worker reaplica
o catalogo em lotes de 500, incluindo jogos desativados, e tenta novamente apos falhas.
A busca tem consistencia eventual: aguarde o intervalo de sincronizacao e o refresh do indice.
Nao ha fallback silencioso para busca SQL. O replay completo e adequado ao volume academico;
para grande escala, substitua por outbox/CDC com checkpoints e reconstrucao via alias.
Mantenha uma replica enquanto a persistencia transacional for SQLite.

O workflow `.github/workflows/ci.yml` testa a solucao e publica a imagem em GHCR
com tag igual ao SHA do commit. Pull requests apenas testam e constroem a imagem.

## Execucao local

```powershell
dotnet restore
dotnet run --project src/Fcg.Catalog.Api --launch-profile http
```

Swagger:

- `http://localhost:5102/swagger`

Health check:

- `http://localhost:5102/health`

Observacao:

- `POST /compras` inicia a compra e publica `OrderPlacedEvent`.
- A inclusao na biblioteca ocorre no consumer de `PaymentProcessedEvent` quando RabbitMQ/MassTransit esta habilitado.

## Variaveis de ambiente

- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_URLS`
- `ConnectionStrings__CatalogDb`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SecretKey`
- `RabbitMq__Enabled`
- `RabbitMq__Host`
- `RabbitMq__VirtualHost`
- `RabbitMq__Username`
- `RabbitMq__Password`

## Mensageria

- Exchange publicado: `fcg.order-placed`
- Exchange consumido: `fcg.payment-processed`
- Fila consumidora: `catalog-payment-processed`
- Serializacao com broker: raw JSON, para interoperar com contratos duplicados entre repositorios.
- Retry do consumer: `3` tentativas com intervalo de `5` segundos.
- Para usar RabbitMQ, configure `RabbitMq__Enabled=true`.

## Validacao

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build --no-restore
```
