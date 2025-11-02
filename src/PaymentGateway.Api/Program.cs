using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using FluentValidation;

using PaymentGateway.Api.Clients.Mountebank;
using PaymentGateway.Api.Config;
using PaymentGateway.Api.Filters;
using PaymentGateway.Api.Identity;
using PaymentGateway.Core.Commands;
using PaymentGateway.Core.Identity;
using PaymentGateway.Core.Repositories;

var builder = WebApplication.CreateBuilder(args);


// For FluentValidator, make sure {PropertyName} at least nearly matches the property name (still need to do camelCase)
// Without this, it'll report 'Expiry Month' rather than engineer helpful 'ExpiryMonth'.
ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) =>
    ValidatorOptions.Global.PropertyNameResolver(type, memberInfo, expression);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IMerchant, BearerMerchant>();
builder.Services.AddSingleton<Func<IMerchant>>((st) =>
{
    // ReSharper disable once ConvertClosureToMethodGroup
    return () => st.GetRequiredService<IMerchant>();
});
builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();

builder.Services.AddControllers(opts =>
    {
        // this exists because we value 1) great developer docs, and 2) really clear validation responses
        // see comment below about data annotations.
        opts.Filters.Add<RequireBodyFilter>();
    })
    .AddJsonOptions(static options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    // You may have noticed we've used a bunch of data annotation attributes (e.g., [Required]) on our API contracts
    // These attributes are great for informing the OpenAPI / Swagger generator how to describe the schema, but
    // we have requirements that are more strict than just what can be expressed in attributes. So, we'll handle
    // those using FluentValidation rather than let .NET try to enforce the validation rules.
    .ConfigureApiBehaviorOptions(static options => options.SuppressModelStateInvalidFilter = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiDocServices();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.RegisterServicesFromAssemblyContaining<AddPaymentRequestHandler>();
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddHttpClient<IMountebankClient, MountebankClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Mountebank:BaseAddress"]!);
    // TODO: add resilience but only retry connection errors
});

builder.Services.RegisterMappings();

builder.Services.AddProblemDetails();


var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseApiDocs();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program
{
}