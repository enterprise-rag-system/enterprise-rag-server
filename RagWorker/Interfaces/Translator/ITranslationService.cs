namespace RagWorker.Interfaces.Translator;

public interface ITranslationService
{
    Task<string> DetectLanguageAsync(string text);
    Task<string> TranslateToEnglishAsync(string text);
    Task<string> TranslateFromEnglishAsync(string text, string targetLanguage);
}