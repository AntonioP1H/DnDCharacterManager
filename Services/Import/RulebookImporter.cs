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

/// <summary>Extracts selectable text from a PDF. OCR and semantic rule parsing are separate future steps.</summary>
public sealed class RulebookImporter : IRulebookImporter
{
    public Task<IReadOnlyList<RulebookPageText>> ExtractPagesAsync(
        string pdfPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pdfPath))
            throw new ArgumentException("A PDF path is required.", nameof(pdfPath));
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException("The rulebook PDF was not found.", pdfPath);

        return Task.Run<IReadOnlyList<RulebookPageText>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var document = PdfDocument.Open(pdfPath);
            var pages = new List<RulebookPageText>();

            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                pages.Add(new RulebookPageText(page.Number, page.Text ?? string.Empty));
            }

            return pages;
        }, cancellationToken);
    }
}
