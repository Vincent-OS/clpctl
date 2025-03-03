using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace CLP.Packager;

public class ClpPackager
{
    /// <summary>
    /// Creates a CLP file from the specified folder path and saves it to the specified output path.
    /// </summary>
    /// <param name="outputPath">The path where the CLP file will be saved.</param>
    /// <param name="folderPath">The path of the folder to be compressed into a CLP file.</param>
    public void CreateClpFile(string outputPath, string folderPath)
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
    /// Extracts the contents of a CLP file to the specified output path.
    /// </summary>
    /// <param name="clpPath">The path of the CLP file to be extracted.</param>
    /// <param name="outputPath">The path where the contents will be extracted.</param>
    public void ExtractClpFile(string clpPath, string outputPath)
    {
        using var archive = ArchiveFactory.Open(clpPath);
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
