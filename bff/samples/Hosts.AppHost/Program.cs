var builder = DistributedApplication.CreateBuilder(args);

var idServer = builder.AddProject<Projects.IdentityServer>("identity-server");

var api = builder.AddProject<Projects.Api>("api");
var isolatedApi = builder.AddProject<Projects.Api_Isolated>("api-isolated");

builder.AddProject<Projects.Bff>("bff")
    .WithExternalHttpEndpoints()
    .WithReference(idServer)
    .WithReference(isolatedApi)
    .WithReference(api);

builder.AddProject<Projects.Bff_EF>("bff-ef")
    .WithExternalHttpEndpoints()
    .WithReference(idServer)
    .WithReference(isolatedApi)
    .WithReference(api);

builder.AddProject<Projects.WebAssembly>("bff-webassembly-per-component")
    .WithExternalHttpEndpoints()
    .WithReference(idServer)
    .WithReference(isolatedApi)
    .WithReference(api);



builder.AddProject<Projects.PerComponent>("bff-blazor-per-component")
    .WithExternalHttpEndpoints()
    .WithReference(idServer)
    .WithReference(isolatedApi)
    .WithReference(api);

var apiDPop = builder.AddProject<Projects.Api_DPoP>("api-dpop");

builder.AddProject<Projects.Bff_DPoP>("bff-dpop")
    .WithExternalHttpEndpoints()
    .WithReference(idServer)
    .WithReference(apiDPop);

builder.AddProject<Projects.UserSessionDb>("migrations");

builder.Build().Run();
