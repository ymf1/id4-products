// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Duende.IdentityServer.Configuration;

using System;

/// <summary>
/// Provides configuration options for enabling and managing preview features in IdentityServer.
/// </summary>
public class PreviewFeatureOptions
{
    /// <summary>
    /// Enables Caching of Discovery Document based on ResponseCaching Interval 
    /// </summary>
    public bool EnableDiscoveryDocumentCache { get; set; } = false;

    /// <summary>
    /// DiscoveryDocument Cache Duration
    /// </summary>
    [Experimental("DUENDEPREVIEW001", UrlFormat = "https://duende.link/previewfeatures?id={0}")]
    public TimeSpan DiscoveryDocumentCacheDuration{ get; set; } = TimeSpan.FromMinutes(1);
}