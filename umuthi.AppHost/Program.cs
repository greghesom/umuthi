var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.umuthi_ApiService>("apiservice");
var functionApp = builder.AddProject<Projects.umuthi_Functions>("functions");

builder.AddProject<Projects.umuthi_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(functionApp)
    .WaitFor(apiService);

builder.Build().Run();
