using System.Net;
using System.Text.RegularExpressions;

namespace XUnity.AutoTranslator.LlmTranslators.Config;

public static class ConfigFunctions
{
    public static string RegexModel(string body)
    {
        var m = Regex.Match(body, "\"result\"\\s*:\\s*\"(?:[^/\"]*/)?([^\"]+)\"");
        return m.Success ? m.Groups[1].Value : body;
    }
    public static void DetectModel(LlmConfig config)
    {
        var trimmedBase = BaseEndpointBehavior.GetDomain(config.Url);
        var Combine = new Func<string, string>(path => BaseEndpointBehavior.CombineUrl(trimmedBase, path));

        var variants = new List<string> { Combine("v1/model"), Combine("model"), Combine("api/v1/model") };
        foreach (var url in variants)
        {
            try
            {
                var req = WebRequest.CreateHttp(url);
                req.Method = "GET";

                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        using var reader = new StreamReader(resp.GetResponseStream());
                        var body = reader.ReadToEnd()?.Trim();
                        string model = RegexModel(body);
                        config.Model = model;

                        Console.WriteLine($"Model parameter is blank, use detected model: {model}");
                        return;
                    }
                }
            }
            catch { }
        }
        config.Model = "default";
        Console.WriteLine("Could not auto-detect model name from the provided URL. Ignore this if endpoint doesn't require it (ie: koboldcpp, textgenwebui)");
    }
    public static void FindCompatibleUrl(LlmConfig config)
    {
        string baseUrl = config.Url;
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new Exception("Config Url is empty");

        // add or interchange https or http to find valid url
        var schemesToTry = new List<string>();
        if (baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            schemesToTry.Add("https://" + baseUrl.Substring(7));
            schemesToTry.Add(baseUrl);
        }
        else if (baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            schemesToTry.Add(baseUrl);
            schemesToTry.Add("http://" + baseUrl.Substring(8));
        }
        else
        {
            schemesToTry.Add("https://" + baseUrl);
            schemesToTry.Add("http://" + baseUrl);
        }

        object lockObj = new object();
        bool found = false;

        foreach (var schemeUrl in schemesToTry)
        {
            if (found) break;

            var trimmedBase = BaseEndpointBehavior.GetDomain(schemeUrl);
            var Combine = new Func<string, string>(path => BaseEndpointBehavior.CombineUrl(trimmedBase, path));

            var variants = new List<string>
            {
            Combine("v1/chat/completions"),
            Combine("api/v1/chat/completions"),
            Combine("api/paas/v4/chat/completions")
            };


            Parallel.ForEach(variants, (url, state) =>
            {
                // Stop quickly if another thread already found a working variant
                if (Volatile.Read(ref found)) return;

                try
                {
                    // Console.WriteLine($"Checking {url}...");

                    var req = (HttpWebRequest)WebRequest.Create(url);
                    req.Method = "POST";
                    req.ContentType = "application/json";
                    req.Timeout = 2000;
                    // Only send api key on https
                    if (!string.IsNullOrWhiteSpace(config.ApiKey) && url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        req.Headers["Authorization"] = $"Bearer {config.ApiKey}";

                    using (var writer = new StreamWriter(req.GetRequestStream()))
                    {
                        writer.Write($@"
                        {{
                            ""model"": ""{config.Model}"",
                            ""max_tokens"": 1,
                            ""messages"": [
                            {{
                                ""role"": ""user"",
                                ""content"": ""Hello there.""
                            }}
                        ]
                    }}");
                    }

                    using var resp = (HttpWebResponse)req.GetResponse();

                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        lock (lockObj)
                        {
                            if (!found)
                            {
                                found = true;
                                config.Url = url;
                                Console.WriteLine($"Detected endpoint: {url}");
                                state.Stop();
                            }
                        }
                    }
                }
                catch (WebException) // ex)
                {
                    /*
                    if (ex.Response is HttpWebResponse r)
                        Console.WriteLine($"Failed {url}: {(int)r.StatusCode} {r.StatusCode}");
                    */
                }
            });
        }
        if (!found)
            throw new InvalidOperationException(
                $"Failed to connect {baseUrl}. Endpoint may not exist, or require a Model to set, or valid API key."
                );
    }
}