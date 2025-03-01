// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using Bff.EF;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

Console.Title = "Bff.EF";

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var app = builder
    .ConfigureServices()
    .ConfigurePipeline();

app.Run();
