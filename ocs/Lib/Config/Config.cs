﻿using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace ocs.Lib.Config;

public record Config
{
    public LogLevel LogLevel { get; init; } = LogLevel.Error;
    public string[] InlineCode { get; init; } = Array.Empty<string>();
    public string[] ReferenceList { get; init; } = Array.Empty<string>();
    public string[] UsingList { get; init; } = Array.Empty<string>();
    public LanguageVersion LanguageVersion { get; init; } = LanguageVersion.CSharp10;

    // ReSharper disable once EmptyConstructor
    public Config() { }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}
