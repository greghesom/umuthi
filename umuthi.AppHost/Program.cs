var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server database
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithDataVolume(); // Persists data between container restarts
    
var umuthiDb = sqlServer.AddDatabase("umuthidb");

var apiService = builder.AddProject<Projects.umuthi_ApiService>("apiservice")
    .WithReference(umuthiDb, "DefaultConnection")
    .WaitFor(sqlServer);

var functionApp = builder.AddProject<Projects.umuthi_Functions>("functions")
    .WithExternalHttpEndpoints()
    .WithReference(umuthiDb, "DefaultConnection")
    .WaitFor(sqlServer);

builder.AddProject<Projects.umuthi_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithReference(functionApp)
    .WaitFor(functionApp)
    .WithReference(umuthiDb, "DefaultConnection")
    .WaitFor(sqlServer);

builder.Build().Run();
