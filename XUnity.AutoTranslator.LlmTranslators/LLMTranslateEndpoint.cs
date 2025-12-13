using SimpleJSON;
using System.Net;
using XUnity.AutoTranslator.LlmTranslators.Config;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Web;

public class LLMTranslatorEndpoint : HttpEndpoint
{
    public override string Id => "LLMTranslate";
    public override string FriendlyName => "LLM Translate";
    public override int MaxTranslationsPerRequest => 1;

    // Careful not to melt machines
    public override int MaxConcurrency => 5;

    private LlmConfig _config = new();

    public override void Initialize(IInitializationContext context)
    {
        _config = Configuration.GetConfiguration("ConfigLLM.yaml");

        // Remove artificial delays
        context.SetTranslationDelay(0.1f);
        context.DisableSpamChecks();
    }

    public override void OnCreateRequest(IHttpRequestCreationContext context)
    {
        var requestData = BaseEndpointBehavior.GetRequestData(_config, context.UntranslatedText);
        var request = new XUnityWebRequest("POST", _config.Url, requestData);
        request.Headers[HttpRequestHeader.ContentType] = "application/json";

        if (!string.IsNullOrEmpty(_config.ApiKey) && _config.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            request.Headers[HttpRequestHeader.Authorization] = $"Bearer {_config.ApiKey}";

        context.Complete(request);
    }

    public override void OnExtractTranslation(IHttpTranslationExtractionContext context)
    {
        var data = context.Response.Data;
        var jsonResponse = JSON.Parse(data);

        var result = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString();
        if (string.IsNullOrEmpty(result))
            result = jsonResponse["message"]?["content"]?.ToString() ?? string.Empty;

        result = BaseEndpointBehavior.ValidateAndCleanupTranslation(context.UntranslatedText, result, _config);

        if (MaxTranslationsPerRequest == 1)
            context.Complete(result);
    }
}