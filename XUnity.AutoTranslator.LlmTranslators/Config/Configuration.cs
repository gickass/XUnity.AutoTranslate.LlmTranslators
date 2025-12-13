using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace XUnity.AutoTranslator.LlmTranslators.Config;

public class LlmConfig
{
    public string? ApiKey { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public string GlossaryPrompt { get; set; } = string.Empty;
    public Dictionary<string, object> ModelParams { get; set; } = new Dictionary<string, object>();
    public List<GlossaryLine> GlossaryLines { get; set; } = new List<GlossaryLine>();
}

public static class Configuration
{
    public static LlmConfig GetConfiguration(string file)
    {
        string gameRoot = AppDomain.CurrentDomain.BaseDirectory;
        string[] foldersToCheck = {
        Path.Combine(gameRoot, "AutoTranslator"),
        Path.Combine(gameRoot, "BepInEx", "config")
        };

        foreach (var folder in foldersToCheck)
        {
            string filepath = Path.Combine(folder, file);
            if (!File.Exists(filepath)) continue;
            var yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var config = yamlDeserializer.Deserialize<LlmConfig>(File.ReadAllText(filepath, Encoding.UTF8));

            // Check endpoint url
            ConfigFunctions.FindCompatibleUrl(config);

            // When model is not set (Usually for local)
            if (string.IsNullOrWhiteSpace(config.Model))
            ConfigFunctions.DetectModel(config);

            if (config.Url == null)
                throw new Exception("No URLs configured for the endpoint.");
            
            return config;
    }
        throw new FileNotFoundException($"Could not find '{file}' in AutoTranslator or BepInEx config folder.");
    }
}