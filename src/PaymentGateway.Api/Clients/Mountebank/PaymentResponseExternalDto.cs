namespace PaymentGateway.Api.Clients.Mountebank;

public record PaymentResponseExternalDto(bool Authorized, string? AuthorizationCode);
