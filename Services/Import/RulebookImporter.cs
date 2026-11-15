using UglyToad.PdfPig;
using System.IO;
namespace DnDCharacterManager.Services.Import;

public sealed record RulebookPageText(int PageNumber, string Text);

public interface IRulebookImporter
{
    Task<IReadOnlyList<RulebookPageText>> ExtractPagesAsync(
        string pdfPath,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Extracts text page-by-page. It does not yet infer race/class/stat blocks or OCR scanned pages.
/// </summary>
public sealed class RulebookImporter : IRulebookImporter
{
    public async Task<IReadOnlyList<RulebookPageText>> ExtractPagesAsync(
        string pdfPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pdfPath);
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException("The PDF file was not found.", pdfPath);

        return await Task.Run<IReadOnlyList<RulebookPageText>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            using PdfDocument document = PdfDocument.Open(pdfPath);
            var result = new List<RulebookPageText>(document.NumberOfPages);

            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                result.Add(new RulebookPageText(page.Number, page.Text ?? string.Empty));
            }

            return result;
        }, cancellationToken);
    }
}
