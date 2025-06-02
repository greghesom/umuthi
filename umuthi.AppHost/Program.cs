var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.umuthi_ApiService>("apiservice");
var functionApp = builder.AddProject<Projects.umuthi_Functions>("functions").WithExternalHttpEndpoints();

builder.AddProject<Projects.umuthi_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService)
     .WithReference(functionApp)
     .WaitFor(functionApp);

builder.Build().Run();
