using RentalLawAssistant.Core.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRentalLawAssistantOptions(builder.Configuration);

var app = builder.Build();

// POST /ask is implemented in phase 2.
app.MapGet("/", () => "RentalLawAssistant.Api");

app.Run();

// Exposes the entry point to WebApplicationFactory in integration tests.
public partial class Program;
