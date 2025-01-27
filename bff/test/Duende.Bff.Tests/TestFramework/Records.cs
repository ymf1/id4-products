// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Duende.Bff.Tests.TestFramework
{
    public record JsonRecord(string Type, JsonElement Value)
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = Type;

        [JsonPropertyName("value")]
        public JsonElement Value { get; init; } = Value;
    }

    public record ClaimRecord(string Type, string Value)
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = Type;

        [JsonPropertyName("value")]
        public string Value { get; init; } = Value;
    }
}
