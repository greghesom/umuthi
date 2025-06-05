using umuthi.Infrastructure.Configuration;
using umuthi.Application.Interfaces;
using umuthi.Application.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Infrastructure services (Entity Framework, repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add services to the container.
builder.Services.AddProblemDetails();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Umuthi API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Add Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Umuthi API V1");
    });
}

// Weather forecast endpoint (keep existing)
string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Workflow API endpoints
app.MapGet("/api/workflows", async (IWorkflowService workflowService) =>
{
    var workflows = await workflowService.GetAllWorkflowsAsync();
    return Results.Ok(workflows);
})
.WithName("GetWorkflows");

app.MapGet("/api/workflows/{id:guid}", async (Guid id, IWorkflowService workflowService) =>
{
    var workflow = await workflowService.GetWorkflowByIdAsync(id);
    return workflow != null ? Results.Ok(workflow) : Results.NotFound();
})
.WithName("GetWorkflow");

app.MapPost("/api/workflows", async (CreateWorkflowDto createWorkflowDto, IWorkflowService workflowService) =>
{
    var workflow = await workflowService.CreateWorkflowAsync(createWorkflowDto);
    return Results.Created($"/api/workflows/{workflow.Id}", workflow);
})
.WithName("CreateWorkflow");

app.MapPut("/api/workflows/{id:guid}", async (Guid id, UpdateWorkflowDto updateWorkflowDto, IWorkflowService workflowService) =>
{
    try
    {
        var workflow = await workflowService.UpdateWorkflowAsync(id, updateWorkflowDto);
        return Results.Ok(workflow);
    }
    catch (ArgumentException)
    {
        return Results.NotFound();
    }
})
.WithName("UpdateWorkflow");

app.MapDelete("/api/workflows/{id:guid}", async (Guid id, IWorkflowService workflowService) =>
{
    try
    {
        await workflowService.DeleteWorkflowAsync(id);
        return Results.NoContent();
    }
    catch (ArgumentException)
    {
        return Results.NotFound();
    }
})
.WithName("DeleteWorkflow");

app.MapPost("/api/workflows/{id:guid}/publish", async (Guid id, IWorkflowService workflowService) =>
{
    try
    {
        var workflow = await workflowService.PublishWorkflowAsync(id);
        return Results.Ok(workflow);
    }
    catch (ArgumentException)
    {
        return Results.NotFound();
    }
})
.WithName("PublishWorkflow");

app.MapPost("/api/workflows/{id:guid}/archive", async (Guid id, IWorkflowService workflowService) =>
{
    try
    {
        var workflow = await workflowService.ArchiveWorkflowAsync(id);
        return Results.Ok(workflow);
    }
    catch (ArgumentException)
    {
        return Results.NotFound();
    }
})
.WithName("ArchiveWorkflow");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
