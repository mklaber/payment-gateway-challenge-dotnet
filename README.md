# Payment Gateway Challenge

This is a submission of the .NET implementation of the CKO Payment Gateway Challenge. The full brief is available in [README.md][cko-readme] and the template repo's original readme is in [README.orig.md][orig-readme].

[cko-readme]: cko-brief/profile/README.md
[orig-readme]: README.orig.md

## Brief

This is a summary of my understanding of the brief:

* A merchant should be able to process a payment and receive:
  * Authorized -- the payment was auth by the acquiring bank (called the bank, they said yes)
  * Declined -- the payment was declined by the acquiring bank (called the bank, they said no)
  * Rejected -- the payment is invalid (didn't even call the bank)
* Merchant should be able to retrieve the details of a **previously made payment** (see assumption below)


- [ ] Document key design considerations
- [ ] Document assumptions
- [ ] Thorough automated test coverage
- [ ] Simple & maintainable -- no over-engineering (justify things that look like they're over-engineered)
- [ ] API design & arch should focs on meeting the functional requirements

## Assumptions

- We're going to define "previously made payment" as "previous attempt to make a payment that was not rejected."
- We won't persist or allow them to retrieve details of a rejected payment because those are client-side errors that the merchant should protect against. (They don't know that a payment will be declined, but they should know that they included a date in the past.)
- Merchants will make structurally correct API requests that abide by the basic, initial API contract. That is to say, we do not need to deal with malformed JSON.
- Our service is sitting behind an API Gateway which handles AuthN for us. All callers are authorized to call the endpoints.
- A reasonable AuthZ restriction would be that you can only retrieve details of _your_ previously made payments. Given the data storage model for this exercise, we'll skip the AuthZ component as well.
- "Ensure your submission validates against no more than 3 currency codes" means we support no more than three currencies (not the currency code should only have 3 characters)
- A payment must be at least `1` minor currency units

MAKE SURE TO MASK CC number on failure

## Design

The API contract really calls for the use of HTTP Status Codes to inform merchants of the success of payments. Traditionally, `Authorized` would likely map to `201: Created` and `Declined` might map to a `4xx` status. However, we have to consider whether "Declined" is the fault of the merchant at all (it's not, they don't know the customer has no funds). 


- Validation layer
- Data contracts separated from data models
- Upfront complexity makes future growth easy
- CQRS advantages like separate read & write, makes team coordination easier, etc. (see MS doc)
- CQRS makes test coverage easier
- Mapping makes things testable
- data annotations and other swagger crap make it easier for merchants and encourages good documentation practices from the jump

## Pre-Requisites

* [.NET 9.0][net9]
* Docker

This project was provided with a bank simulator. Start it before running the app and tests:

```bash
docker compose up
```

You can verify the simulator is running by navigating to [http://localhost:2525/][sim-lh] in your browser.

If you're trying to build and run, you may need to restore .NET tools:

```bash
dotnet tool restore
```



[net9]: https://dotnet.microsoft.com/en-us/download/dotnet/9.0
[sim-lh]: http://localhost:2525/
