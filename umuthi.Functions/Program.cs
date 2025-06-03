using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using umuthi.Functions.Services;

var builder = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Register audio processing services
        services.AddScoped<IAudioConversionService, AudioConversionService>();
        services.AddScoped<ISpeechTranscriptionService, SpeechTranscriptionService>();
    });

var host = builder.Build();
await host.RunAsync();