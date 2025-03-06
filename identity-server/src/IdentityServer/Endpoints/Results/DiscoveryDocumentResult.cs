// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Duende.IdentityServer.Hosting;

namespace Duende.IdentityServer.Endpoints.Results;

/// <summary>
/// Result for a discovery document
/// </summary>
/// <seealso cref="IEndpointResult" />
public class DiscoveryDocumentResult : EndpointResult<DiscoveryDocumentResult>
{
    /// <summary>
    /// Gets the maximum age.
    /// </summary>
    /// <value>
    /// The maximum age.
    /// </value>
    public int? MaxAge { get; }

    /// <summary>
    /// Gets or sets the JSON representation of the entries in the discovery document.
    /// </summary>
    /// <value>
    /// A JSON string that represents the entries.
    /// </value>
    public string Json { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoveryDocumentResult" /> class.
    /// </summary>
    /// <param name="entries">The entries.</param>
    /// <param name="maxAge">The maximum age.</param>
    /// <exception cref="System.ArgumentNullException">entries</exception>
    public DiscoveryDocumentResult(Dictionary<string, object> entries, int? maxAge = null)
    {
        MaxAge = maxAge;

        // serialize entries ahead of time
        Json = ObjectSerializer.ToString(entries);
    }

    /// <summary>
    /// Represents a result for a discovery document, implementing <see cref="IEndpointResult"/>.
    /// </summary>
    public DiscoveryDocumentResult(string json, int? maxAge = null)
    {
        MaxAge = maxAge;
        Json = json;
    }
}

class DiscoveryDocumentHttpWriter : IHttpResponseWriter<DiscoveryDocumentResult>
{
    /// <inheritdoc/>
    public Task WriteHttpResponse(DiscoveryDocumentResult result, HttpContext context)
    {
        if (result.MaxAge.HasValue && result.MaxAge.Value >= 0)
        {
            context.Response.SetCache(result.MaxAge.Value, "Origin");
        }

        return context.Response.WriteJsonAsync(result.Json);
    }
}