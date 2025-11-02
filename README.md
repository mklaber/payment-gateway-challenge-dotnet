# Payment Gateway Challenge

This is a submission of the .NET implementation of the CKO Payment Gateway Challenge. The full brief is available in [README.md][cko-readme] and the template repo's original readme is in [README.orig.md][orig-readme].

[cko-readme]: https://github.com/cko-recruitment/.github/blob/main/profile/README.md
[orig-readme]: README.orig.md

> [!TIP]
> This README is long. GitHub provides a dynamic Table of Contents to help you find your way through it. It should be in the upper right corner of this document.

## Assumptions

- A payment must be at least `1` minor currency units
- The two endpoints implemented will not be the only endpoints in this service. That is to say: we need to leave code in a reasonably extensible state (with an eye toward not overengineering).
- When the upstream bank (Mountebank) fails, we can't know how far into the process the bank failed and therefore should leave the risk of retrying to the consumer (Merchant).
- We don't know the requirements of future banks so trying to come up with a general purpose "bank interface" would be premature.
- Credit cards can be processed without 3DS or other protective interstitials
- Though we're at an organisation that is willing to pay reasonable licensing costs for previously OSS libraries, we'll try to avoid using a paid-for version
- We're going to define "previously made payment" as "previous attempt to make a payment that was not rejected." This is to say that we'll store payments that were successfully transmitted to the bank, even if they declined it.
- We won't persist or allow them to retrieve details of a rejected payment because those are client-side errors that the merchant should protect against. (They don't know that a payment will be declined, but they should know that they included a date in the past.) This assumption is supported by some of the functional requirements in the brief (none ask to return a status of "rejected"), and just common sense: we should not persist rejected requests because the requests themselves may be in a state inconsistent with our data model (e.g., overflow of massively long credit card numbers).
- Our service is sitting behind an API Gateway that does some of the general purpose protection for us including DDoS, per-merchant rate limiting, and verifying credentials.
- Detailed AuthN and AuthZ is out of scope (though we've implemented enough of this to ensure Merchants can be identified)
- All authenticated merchants are authorised to call either of the two endpoints (though not authorised to retrieve other merchants' payments)
- "Ensure your submission validates against no more than 3 currency codes" means we support no more than three currencies (not the currency code should only have 3 characters). We've picked the British Pound (`GBP`), the Euro (`EUR`), and the Swiss Franc (`CHF`). 


## Design

The tl;dr of this solution's design:

- Detailed validation layer
- Data contracts separated from data models
- Upfront boilerplate complexity makes future growth easy
- Mediator has advantages like separate read & write, testable, single purpose, and makes team coordination easier, etc.
- Mapping makes things testable
- Data annotations and Swagger / OpenAPI stuff is all about the Merchants
- Don't forget auth, but also don't overcomplicate it

> [!IMPORTANT]
> **Is this over-engineered?**
> 
> In short: **no**.
> 
> There are a bunch of classes and namespaces and third-party libraries. It may look over-engineered. But, a vast majority of the code is boilerplate code that may look complicated but ultimately looks no different from most other .NET API you'll find. Other parts of code that look over-engineered aren't really engineering so much as being very particular about how it is presented to Merchants or to future engineers.
> 
> **To get a clearer idea of where the actual engineering lies, look at [`PaymentGateway.Api.Tests`][uts] and consider that (as of this writing) we've got 100% test coverage.**

First, we're maintaining separate API "contracts" (an agreement with Merchants over what our API actually does, published as OpenAPI specs) from our actual data model. This allows us to accept and project data in an API friendly manner without mingling those concerns into our data layer. The overriding principle here is: do not expose your database model to the internet, and do not let the internet's needs muddy your database model. Contracts are in the `Contracts` namespace and are either requests or data transfer objects (DTO). The data model is in the `Models` namespace. We've added [Mapster][map][^am] to facilitate mapping between the two. (This same principal is used between our logic and the "contracts" of the Bank simulator's API.)

Second, just as a user-facing app needs to be clear to an end user, so too does our API. To that extent, we're using a couple of tools:

* We're using Data Annotation attributes (e.g., `[Required]`, `[MaxLength]`, etc.) to improve the JSON Schema of our generated API spec.
* [NSwag][swag] has been configured to publish the API spec in an easy to consume format: ReDoc. We're also making thorough use of Intellisense code comments that NSwag picks up and includes in the spec.
* In the case that a consumer makes an invalid request, it's very important to be as explicit about why it was **rejected** as possible. To that end, we're using [FluentValidation][fv] to provide detailed validation along with detailed error responses. (Data Annotations can only get you so far which is why we've overridden the Data Annotation-based validation.)

The third major design decision is one that's not client-facing at all: we're using the mediator pattern to segregate reads and writes ([CQRS pattern][cqrs]), help enforce a single responsibility principle, and generally make our business logic more composable and testable. Though it may feel like over-engineering at times, it will help future developers add functionality without stepping on each other's toes. We're using the well established library [MediatR][media][^mr]. We're using it for commands, queries, and request handlers.

Finally, we're talking about some pretty sensitive information. We can't give merchants access to other merchants' payments, we can't let authenticated users charge arbitrary credit cards, and we can't print sensitive information to our logs. We've implemented a basic JWT bearer scheme that allows for authentication and merchant identification (multi-tenancy). The data model silos data across different merchants. And, we've been thoughtful about what gets logged and when.

> [!NOTE]
> There's a lot more that can be done with Authentication such as endpoint specific ACLs, more explicit claims (we're just using `name`), and OpenID Connect settings discovery. But, some of this is assumed to be handled at the API Gateway level or otherwise out of scope of this project.
> 
> If we configured an API Gateway to extract claims from tokens and put them in, say, `X-Payment-Gateway-Merchant-Id` headers, we could even consider getting rid of this app's authentication all together.

[map]: https://github.com/MapsterMapper/Mapster
[swag]: https://github.com/RicoSuter/NSwag
[fv]: https://docs.fluentvalidation.net/en/latest/
[media]: https://github.com/LuckyPennySoftware/MediatR
[cqrs]: https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs
[^am]: I'd prefer AutoMapper but the other has changed the licensing model and Mapster is good enough
[^mr]: Unlike AutoMapper, I didn't find an alternative to MediatR that was better than just pinning to a pre-paid license version


## Pre-Requisites

* [.NET 9.0][net9]
* Docker
* A JWT token

This project was provided with a bank simulator. Start it before running the app and tests:

```bash
docker compose up
```

You can verify the simulator is running by navigating to [http://localhost:2525/][sim-lh] in your browser.

This project simulates a proper authentication and identity provider by using basic JWTs provided by [`dotnet user-jwts`][user-jwts]. You'll need to create at least one token in order to make local requests and run [Smoke Tests][smoke-t].

```bash
dotnet user-jwts create --project src/PaymentGateway.Api/PaymentGateway.Api.csproj --output token --name acme-merchant
```

Copy the output of this command and save it for later. (If you forget to write this down, you can retrieve it again using the `list` and `print` commands.)

[net9]: https://dotnet.microsoft.com/en-us/download/dotnet/9.0
[sim-lh]: http://localhost:2525/
[user-jwts]: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn?view=aspnetcore-9.0&tabs=linux
[smoke-t]: test/PaymentGateway.Api.IntegrationTests/README.md
[uts]: test/PaymentGateway.Api.Tests/README.md

## Running

You can run the app in your favourite IDE; the `PaymentGateway.Api.Swagger` Launch Profile is recommended for IDE users.

Alternatively, start it from the command line:

```bash
dotnet run --launch-profile PaymentGateway.Api --project src/PaymentGateway.Api/PaymentGateway.Api.csproj
```

And then navigate to [https://localhost:7092/swagger/][lh-swag]

Click the **Authorize** button and paste in the `acme-merchant` token you generated earlier. You are now, as we say, cooking with gas[^gas].

[lh-swag]: https://localhost:7092/swagger/
[^gas]: Who even reads footnotes? You do! See [English Language & Usage](https://english.stackexchange.com/a/100588).

## Tests

This solution offers three types of tests:

* **Unit Tests** – for the [Api](test/PaymentGateway.Api.Tests)
* **Integration Tests** – for the [Api](test/PaymentGateway.Api.IntegrationTests/README.md#integration-tests) (see README)
* **Smoke Tests** – for the [Api](test/PaymentGateway.Api.IntegrationTests/README.md#smoke-tests), found and described in the Integration Test project

To run the Unit and Integration tests (make sure you've done a `docker compose up`):

```bash
dotnet test
```

> [!IMPORTANT]
> As of this writing, test coverage of this solution is 100%.

To run the Smoke Tests, see the README in the [Integration Tests](test/PaymentGateway.Api.IntegrationTests/README.md#running-the-smoke-tests) project.

## Client / Merchant Documentation

Thorough client (Merchant) documentation is available at `/redoc` and the (uglier) `/swagger`. You'll need to start the app in `Development` (default) mode.
