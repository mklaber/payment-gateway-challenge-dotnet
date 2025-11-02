# Payment Gateway Integration & Smoke Tests

We offer thorough Integration Tests and a few Smoke Tests that verify the alterations the integration test suite requires (mainly JWT authentication).

## Integration Tests

Integration tests are intended to verify the API in the context of all of its components such as dependency injected services, calls to external dependencies (in our case, the Bank Simulator), etc. They're effectively E2E tests. Our tests run against `PaymentGateway.Api` using its `appsettings.Development.json` configuration (`Development` is the default mode for the `WebApplicationFactory`).

There are two alterations these tests make relative to a normal Development configuration:

1. The text suite overrides the JWT Bearer Token authentication requirements with a `TestAuthHandler`; a technique described by Microsoft in [Integration tests in ASP.NET Core][it-core]. This is not ideal but, ultimately, is a consequence of relying on [`dotnet user-jwts`][user-jwts] rather than setting up an external auth provider like Keycloak as a local Docker container.
2. For some (but not all) tests, we register our own `IPaymentsRepository` for the purposes of seeding the in-memory "database." This, too, is not ideal but is an approach offered by this exercise's [starter repository][cko-dotnet].

To run a quick Smoke Test with the standard JWT configuration, see below.

[it-core]: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-9.0&pivots=xunit#mock-authentication
[user-jwts]: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn?view=aspnetcore-9.0&tabs=linux
[cko-dotnet]: https://github.com/cko-recruitment/payment-gateway-challenge-dotnet/blob/main/test/PaymentGateway.Api.Tests/PaymentsControllerTests.cs#L35-L36

### Integration Tests Pre-requisites

The bank simulator needs to be running. From the root of this repository:

```bash
docker compose up
```

### Running the Integration Tests

You can use your favourite test runner, or:

```bash
dotnet test
```

## Smoke Tests

Smoke Tests offer a quick "sanity check" that the whole system works without diving into testing that components are fully working together.

### Smoke Test Pre-requisites

First, follow the steps in the Integration Tests Pre-requites to get Docker running.

To run the Smoke Tests, you can use the JetBrains [HTTP Client][jb-http-plug] plugin. If you don't have a JetBrains IDE, you can use the [HTTP Client CLI][jb-http-cli]. You'll need to add a private environment file at the root of this project called `http-client.private.env.json`. It should have the following content where `<<user-jwt>>` is a JWT token retrieved from `dotnet user-jwts` (see other README docs):

```json
{
  "e2e": {
    "merchant-1-token": "<<user-jwt>>"
  }
}
```

[jb-http-plug]: https://www.jetbrains.com/help/rider/2025.2/Http_client_in__product__code_editor.html
[jb-http-cli]: https://www.jetbrains.com/help/rider/2025.2/HTTP_Client_CLI.html


### Running the Smoke Tests

If you're using the HTTP Client plugin, open `payment-gateway-api-e2e.http` and set the environment to `e2e` (from the environment file that has the `<<user-jwt>>` in it). Then, click the *Run All Requests in File* button.

If you're using the CLI, run:

```bash
ijhttp \
  --insecure=true \
  --private-env-file http-client.private.env.json \
  --env e2e \
  payment-gateway-api-e2e.http
```