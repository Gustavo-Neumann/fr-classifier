using FrClassifier.Data;
using FrClassifier.Repositories;
using FrClassifier.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var connectionString = builder.Configuration.GetConnectionString("FinancialDatabase")
    ?? throw new InvalidOperationException("ConnectionStrings:FinancialDatabase is required.");
builder.Services.AddDbContext<FrClassifierDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IDocumentParser, AcdocaXlsxImporter>();
builder.Services.AddScoped<ImportDocumentService>();
builder.Services.AddScoped<ClassificationDispatchService>();
builder.Services.AddScoped<ClassificationResultService>();
builder.Services.AddSingleton<IDocumentStorage, LocalDocumentStorage>();
builder.Services.AddSingleton<IClassificationMessagePublisher, RabbitMqClassificationMessagePublisher>();
builder.Services.AddHostedService<RabbitMqClassificationResultConsumer>();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FrClassifier API", Version = "v1" }));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FrClassifier API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
