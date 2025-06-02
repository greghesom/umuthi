var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.umuthi_ApiService>("apiservice");

builder.AddProject<Projects.umuthi_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
