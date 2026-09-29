using FrClassifier.Data;
using FrClassifier.Repositories;
using FrClassifier.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
var connectionString = builder.Configuration.GetConnectionString("FinancialDatabase")
    ?? throw new InvalidOperationException("ConnectionStrings:FinancialDatabase is required.");
builder.Services.AddDbContext<FrClassifierDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IFinancialDocumentRepository, FinancialDocumentRepository>();
builder.Services.AddScoped<IFinancialAccountRepository, FinancialAccountRepository>();
builder.Services.AddScoped<IFinancialDocumentParser, XlsxFinancialDocumentParser>();
builder.Services.AddScoped<FinancialDocumentImportService>();
builder.Services.AddScoped<ClassificationDispatchService>();
builder.Services.AddScoped<ClassificationResultService>();
builder.Services.AddSingleton<IFinancialDocumentStorage, FinancialDocumentStorage>();
builder.Services.AddSingleton<IClassificationMessagePublisher, RabbitMqClassificationMessagePublisher>();
builder.Services.AddHostedService<RabbitMqClassificationResultConsumer>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
