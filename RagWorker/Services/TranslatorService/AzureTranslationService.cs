using System.Text;
using System.Text.Json;
using RagWorker.Interfaces.Translator;

namespace RagService.Services.TranslatorService;

public class AzureTranslationService: ITranslationService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<AzureTranslationService> _logger;

    public AzureTranslationService(
        HttpClient http,
        IConfiguration config,
        ILogger<AzureTranslationService> logger)
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
            var url = $"{_config["Translator:Endpoint"]}/detect?api-version=3.0";

            var response = await SendRequest(url, text);

            using var doc = JsonDocument.Parse(response);
            var result = doc.RootElement[0].GetProperty("language").GetString();
            _logger.LogInformation($"Laguage detected: {result}");
            return result;

        }
        catch (Exception e)
        { 
            _logger.LogError($"Error Running DetectLanguageAsync {e.Message}");
            _logger.LogError(e.ToString());
            return string.Empty;
        }
        finally{
            _logger.LogInformation("Exit Method DetectLanguageAsync");
            
        }
    }

    public async Task<string> TranslateToEnglishAsync(string text)
    {
        try
        {
            _logger.LogInformation("Enter Method TranslateToEnglishAsync");
            return await TranslateAsync(text, "en");
        }
        catch (Exception e)
        {
            _logger.LogError($"Error Running TranslateToEnglishAsync {e.Message}");
            _logger.LogError(e.ToString());
            throw;
        }
        finally{
            _logger.LogInformation("Exit Method TranslateToEnglishAsync");
        }
    }

    public async Task<string> TranslateFromEnglishAsync(string text, string targetLanguage)
    {
        try
        {
            _logger.LogInformation("Enter Method TranslateFromEnglishAsync");
            return await TranslateAsync(text, targetLanguage);

        }
        catch (Exception e)
        {
            _logger.LogError($"Error Running TranslateFromEnglishAsync {e.Message}");
            _logger.LogError(e.ToString());
            throw;
        }
        finally
        {
            _logger.LogInformation("Exit Method TranslateFromEnglishAsync");
        }
    }
    
    private async Task<string> TranslateAsync(string text, string to)
    {
        try
        {
            _logger.LogInformation("Enter Method TranslateToEnglishAsync");
            var url = $"{_config["Translator:Endpoint"]}/translate?api-version=3.0&to={to}";

            var response = await SendRequest(url, text);

            using var doc = JsonDocument.Parse(response);
            return doc.RootElement[0]
                .GetProperty("translations")[0]
                .GetProperty("text")
                .GetString();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error Running TranslateToEnglishAsync {e.Message}");
            _logger.LogError(e.ToString());
            throw;
        }
        finally
        {
            _logger.LogInformation("Exit Method TranslateToEnglishAsync");
        }
    }
    
    private async Task<string> SendRequest(string url, string text)
    {
        try
        {
            _logger.LogInformation("Enter Method TranslateToEnglishAsync");
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Content = new StringContent(
                JsonSerializer.Serialize(new[] { new { Text = text } }),
                Encoding.UTF8,
                "application/json"
            );

            request.Headers.Add("Ocp-Apim-Subscription-Key", _config["Translator:ApiKey"]);
            request.Headers.Add("Ocp-Apim-Subscription-Region", _config["Translator:Region"]);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error Running TranslateToEnglishAsync {e.Message}");
            _logger.LogError(e.ToString());
            throw;
        }
        finally
        {
            _logger.LogInformation("Exit Method TranslateToEnglishAsync");
        }
    }
}