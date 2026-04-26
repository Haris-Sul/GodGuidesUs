using GodGuidesUs.Api.Models;
using GodGuidesUs.Api.Repositories;
using GodGuidesUs.Api.Services;

var builder = WebApplication.CreateBuilder(args);
const string corsPolicyName = "WebClient";

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policyBuilder =>
    {
        policyBuilder
            .WithOrigins(
                "http://localhost:5173", 
                "https://godguides.us", 
                "https://god-guides-us.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services
    .AddOptions<MongoDbSettings>()
    .Bind(builder.Configuration.GetSection(MongoDbSettings.SectionName));
builder.Services
    .AddOptions<GoogleAiSettings>()
    .Bind(builder.Configuration.GetSection(GoogleAiSettings.SectionName));
builder.Services.AddScoped<IVerseRepository, VerseRepository>();
builder.Services.AddHttpClient<IAiService, GoogleAiService>((serviceProvider, client) =>
{
    var googleAiSettings = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<GoogleAiSettings>>()
        .Value;

    client.BaseAddress = new Uri(googleAiSettings.BaseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(corsPolicyName);
app.MapControllers();

app.Run();
