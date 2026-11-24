var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Ticket API is running!");
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        service = "TicketApi",
        status = "Healthy",
        time = DateTime.UtcNow
    });
});
app.Run();
