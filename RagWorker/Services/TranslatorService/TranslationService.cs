using System.Text;
using System.Text.Json;
using RagWorker.Interfaces.Translator;

namespace RagService.Services.TranslatorService;

public class TranslationService : ITranslationService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<TranslationService> _logger;

    private string targetLang = "en";

    public TranslationService(
        HttpClient http,
        IConfiguration config,
        ILogger<TranslationService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<string> DetectLanguageAsync(string text)
    {
        try
        {
            _logger.LogInformation("Enter Method DetectLanguageAsync");

            var url = $"{_config["Translator:Endpoint"]}/detect";

            // LibreTranslate /detect expects { q: "text" }
            var payload = JsonSerializer.Serialize(new { q = text });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            // Returns array: [{ language: "en", confidence: 0.9 }, ...]
            _logger.LogInformation($"Language detected object: {JsonSerializer.Serialize(doc)}");
            var result = doc.RootElement[0].GetProperty("language").GetString();
            
            _logger.LogInformation("Language detected: {Language}", result);
            
            targetLang = result ?? string.Empty;
            return targetLang;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error running DetectLanguageAsync: {Message}", e.Message);
            return string.Empty;
        }
        finally
        {
            _logger.LogInformation("Exit Method DetectLanguageAsync");
        }
    }

    public async Task<string> TranslateToEnglishAsync(string text)
    {
        try
        {
            _logger.LogInformation("Enter Method TranslateToEnglishAsync");
            return await TranslateAsync(text, targetLang, "en");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error running TranslateToEnglishAsync: {Message}", e.Message);
            throw;
        }
        finally
        {
            _logger.LogInformation("Exit Method TranslateToEnglishAsync");
        }
    }

    public async Task<string> TranslateFromEnglishAsync(string text, string targetLanguage)
    {
        try
        {
            _logger.LogInformation("Enter Method TranslateFromEnglishAsync");
            return await TranslateAsync(text, "en", targetLanguage);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error running TranslateFromEnglishAsync: {Message}", e.Message);
            throw;
        }
        finally
        {
            _logger.LogInformation("Exit Method TranslateFromEnglishAsync");
        }
    }

    // ----------------------------------------------------------------
    // Private Helpers
    // ----------------------------------------------------------------

    private async Task<string> TranslateAsync(string text, string src, string to)
    {
        try
        {
            _logger.LogInformation("Translating from {Source} to {Target}", src, to);

            var url = $"{_config["Translator:Endpoint"]}/translate";

            // LibreTranslate /translate payload
            var payload = JsonSerializer.Serialize(new
            {
                q = text,
                source = src,  // use "auto" to skip manual detect
                target = to,
                format = "text",
                api_key = _config["Translator:ApiKey"] ?? string.Empty  // empty for self-hosted
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            // Returns: { translatedText: "..." }
            _logger.LogInformation("Translated to english {res}", JsonSerializer.Serialize(doc.RootElement));
            return doc.RootElement.GetProperty("translatedText").GetString() ?? string.Empty;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error running TranslateAsync: {Message}", e.Message);
            throw;
        }
        finally
        {
            _logger.LogInformation("Exit Method TranslateAsync");
        }
    }
}