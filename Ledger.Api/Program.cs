using System.Text.Json.Serialization;
using Ledger.Api;
using FluentValidation;
using Ledger.Core;
using Scalar.AspNetCore;
using Ledger.Core.Services;
using Ledger.Core.Validators;
using Ledger.Core.Models.Dtos;
using Ledger.Core.Repositories;
using Ledger.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<TransactionLimitsOptions>(
    builder.Configuration.GetSection("TransactionLimits"));

builder.Services.AddScoped<IValidator<CreateAccountRequest>, CreateAccountRequestValidator>();
builder.Services.AddScoped<IValidator<CreateTransactionRequest>, CreateTransactionRequestValidator>();

builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
builder.Services.AddSingleton<ITransactionRepository, InMemoryTransactionRepository>();
builder.Services.AddScoped<ILedgerService, LedgerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.AddLedgerEndpoints();

app.Run();