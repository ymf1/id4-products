var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.IdentityServer>("identity-server");
builder.AddProject<Projects.Bff>("bff");
builder.AddProject<Projects.Api>("api");

builder.Build().Run();
