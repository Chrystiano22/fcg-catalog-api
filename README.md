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
