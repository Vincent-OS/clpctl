using SharpCompress.Archives;
using SharpCompress.Common;

namespace KLP.Packager;

public class KlpPackager
{
    public void CreateKlpFile(string outputPath, string folderPath)
    {
        using var archive = ArchiveFactory.Create(ArchiveType.Zip);
        foreach (var file in Directory.GetFiles(folderPath))
        {
            archive.AddEntry(Path.GetFileName(file), File.OpenRead(file));
        }

        using var stream = File.OpenWrite(outputPath);
        archive.SaveTo(stream, new WriterOptions(CompressionType.Deflate));
    }

    public void ExtractKlpFile(string klpPath, string outputPath)
    {
        using var archive = ArchiveFactory.Open(klpPath);
        foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
        {
            entry.WriteToDirectory(outputPath, new ExtractionOptions
            {
                ExtractFullPath = true,
                Overwrite = true
            });
        }
    }
}
