using FrClassifier.Data;
using FrClassifier.Repositories;
using FrClassifier.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Npgsql;

if (File.Exists(".env"))
{
    DotNetEnv.Env.NoClobber().Load(".env");
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins(
                "http://localhost:4200",
                "http://127.0.0.1:4200",
                "http://localhost:5173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()));
var databaseConnection = new NpgsqlConnectionStringBuilder
{
    Host = RequiredSetting(builder.Configuration, "DATABASE_HOST"),
    Port = builder.Configuration.GetValue("DATABASE_PORT", 5432),
    Database = RequiredSetting(builder.Configuration, "DATABASE_NAME"),
    Username = RequiredSetting(builder.Configuration, "DATABASE_USER"),
    Password = RequiredSetting(builder.Configuration, "DATABASE_PASSWORD")
};
var connectionString = databaseConnection.ConnectionString;

var rabbitMqUri = new UriBuilder(
    "amqp",
    RequiredSetting(builder.Configuration, "RABBITMQ_HOST"),
    builder.Configuration.GetValue("RABBITMQ_PORT", 5672))
{
    UserName = RequiredSetting(builder.Configuration, "RABBITMQ_USER"),
    Password = RequiredSetting(builder.Configuration, "RABBITMQ_PASSWORD"),
    Path = "/"
};
builder.Configuration["RabbitMQ:Uri"] = rabbitMqUri.Uri.ToString();

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

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FrClassifierDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FrClassifier API v1"));
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseCors("Frontend");
}

app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

static string RequiredSetting(IConfiguration configuration, string name) =>
    configuration[name] is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException($"Environment variable {name} is required.");
