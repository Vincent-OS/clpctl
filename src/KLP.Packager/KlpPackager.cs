using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace KLP.Packager;

public class KlpPackager
{
    /// <summary>
    /// Creates a KLP file from the specified folder path and saves it to the specified output path.
    /// </summary>
    /// <param name="outputPath">The path where the KLP file will be saved.</param>
    /// <param name="folderPath">The path of the folder to be compressed into a KLP file.</param>
    public void CreateKlpFile(string outputPath, string folderPath)
    {
        using var archive = ArchiveFactory.Create(ArchiveType.GZip);
        foreach (var file in Directory.GetFiles(folderPath))
        {
            archive.AddEntry(Path.GetFileName(file), File.OpenRead(file), true);
        }

        using var stream = File.OpenWrite(outputPath);
        archive.SaveTo(stream, new WriterOptions(CompressionType.Deflate));
    }

    /// <summary>
    /// Extracts the contents of a KLP file to the specified output path.
    /// </summary>
    /// <param name="klpPath">The path of the KLP file to be extracted.</param>
    /// <param name="outputPath">The path where the contents will be extracted.</param>
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
