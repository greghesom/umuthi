using Microsoft.Extensions.Hosting;
using umuthi.Functions;

var host = Startup.ConfigureHost().Build();
await host.RunAsync();