using System;
using System.Threading.Tasks;
namespace DnDCharacterManager.Services.Import;

public interface IRulebookImporter
{
    Task ImportBasicRulesAsync(string pdfPath, IProgress<string>? progress = null);
    Task ImportMonsterManualAsync(string pdfPath, IProgress<string>? progress = null);

    // The 2e book is validated/extracted for reference only; it is not imported into 5e tables.
    Task ImportPhb2eAsync(string pdfPath, IProgress<string>? progress = null);
}
