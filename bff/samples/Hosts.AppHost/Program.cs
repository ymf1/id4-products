var builder = DistributedApplication.CreateBuilder(args);

var idServer = builder.AddProject<Projects.IdentityServer>("identity-server");

var api = builder.AddProject<Projects.Api>("api");

builder.AddProject<Projects.Bff>("bff")
    .WithExternalHttpEndpoints()
    .WithReference(idServer)
    .WithReference(api);

var apiDPop = builder.AddProject<Projects.Api_DPoP>("api-dpop");

builder.AddProject<Projects.Bff_DPoP>("bff-dpop")
    .WithExternalHttpEndpoints()
    .WithReference(idServer)
    .WithReference(apiDPop);

builder.Build().Run();
