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
            InlineCode = provider.GetSection("inline").Get<string[]>(),
            LogLevel = provider.GetValue("logLevel", LogLevel.Error),
            UsingList = provider.GetSection("using").Get<string[]>() ?? Array.Empty<string>(),
            ReferenceList = provider.GetSection("references").Get<string[]>() ?? Array.Empty<string>(),
            LanguageVersion = provider.GetValue("langVersion", LanguageVersion.CSharp10)
        };
    }
}
