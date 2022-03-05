using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ocs.Lib.Config;

public static class ConfigFactory
{
    private const string ConfigFileName = "setting";
    private const string ConfigEnvironmentPrefix = "OCS_";

    public static Config Create()
    {
        var configDirPath = Environment.GetEnvironmentVariable(ConfigEnvironmentPrefix + "CONFIG_DIR") ??
                            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ocs");

        if (!Directory.Exists(configDirPath)) return new Config();

        var provider = new ConfigurationBuilder()
            .SetBasePath(configDirPath)
            .AddEnvironmentVariables(ConfigEnvironmentPrefix)
            .AddJsonFile(ConfigFileName + ".json")
            .Build();

        if (provider is null) return new Config();

        return new Config
        {
            InlineCode = provider.GetSection("InlineCode").Get<string[]>(),
            LogLevel = provider.GetValue("LogLevel", LogLevel.Error),
            UsingList = provider.GetSection("UsingList").Get<string[]>() ?? Array.Empty<string>(),
            ReferenceList = provider.GetSection("ReferenceList").Get<string[]>() ?? Array.Empty<string>(),
            LanguageVersion = provider.GetValue("LanguageVersion", LanguageVersion.CSharp10)
        };
    }
}
