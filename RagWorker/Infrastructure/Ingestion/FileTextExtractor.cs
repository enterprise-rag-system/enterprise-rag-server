using System.Text;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Logging;
using RagWorker.Interfaces.Ingestion;
using UglyToad.PdfPig;

namespace RagWorker.Infrastructure.Ingestion;

public class FileTextExtractor : ITextExtractor
{
    private readonly ILogger<FileTextExtractor> _logger;

    private static readonly HashSet<string> SupportedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".txt",
            ".pdf",
            ".docx",
            ".doc"
        };

    public FileTextExtractor(ILogger<FileTextExtractor> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExtractAsync(
        string filePath,
        CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is empty");

            if (!File.Exists(filePath))
                throw new FileNotFoundException(
                    $"File not found at path: {filePath}");

            var extension = Path.GetExtension(filePath);

            if (!SupportedExtensions.Contains(extension))
                throw new NotSupportedException(
                    $"File type '{extension}' is not supported");

            return extension.ToLowerInvariant() switch
            {
                ".txt"  => await ExtractTxtAsync(filePath, ct),
                ".pdf"  => ExtractPdf(filePath),
                ".docx" => ExtractDocx(filePath),
                ".doc"  => throw new NotSupportedException(
                                ".doc is not supported. Convert to .docx"),
                _ => throw new NotSupportedException("Unsupported file type")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File extraction failed for {Path}", filePath);
            throw;
        }
    }

    // ---------- TXT ----------
    private static async Task<string> ExtractTxtAsync(
        string path,
        CancellationToken ct)
        => await File.ReadAllTextAsync(path, ct);

    // ---------- PDF ----------
    private static string ExtractPdf(string path)
    {
        var sb = new StringBuilder();

        using var document = PdfDocument.Open(path);
        foreach (var page in document.GetPages())
            sb.AppendLine(page.Text);

        return sb.ToString();
    }

    // ---------- DOCX ----------
    private static string ExtractDocx(string path)
    {
        var sb = new StringBuilder();

        using var doc = WordprocessingDocument.Open(path, false);
        var body = doc.MainDocumentPart?.Document?.Body;

        if (body == null)
            return string.Empty;

        foreach (var text in body.Descendants<
                     DocumentFormat.OpenXml.Wordprocessing.Text>())
        {
            sb.Append(text.Text);
            sb.Append(' ');
        }

        return sb.ToString();
    }
}
