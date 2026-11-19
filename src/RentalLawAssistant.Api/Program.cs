var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// POST /ask is implemented in phase 2.
app.MapGet("/", () => "RentalLawAssistant.Api");

app.Run();

// Exposes the entry point to WebApplicationFactory in integration tests.
public partial class Program;
