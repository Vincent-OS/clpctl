using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.Text;

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
        using var tarArchive = TarArchive.Create();
        foreach (var dir in Directory.GetDirectories(folderPath))
        {
            tarArchive.AddAllFromDirectory(dir);
        }
        foreach (var file in Directory.GetFiles(folderPath))
        {
            tarArchive.AddEntry(Path.GetFileName(file), file);
        }
        var writerOptions = new WriterOptions(CompressionType.GZip)
        {
            ArchiveEncoding = new ArchiveEncoding(Encoding.UTF8, Encoding.UTF8)
        };
        tarArchive.SaveTo(outputPath, writerOptions);
    }

    /// <summary>
    /// Extracts the contents of a CLP file to the specified output path.
    /// </summary>
    /// <param name="clpPath">The path of the CLP file to be extracted.</param>
    /// <param name="outputPath">The path where the contents will be extracted.</param>
    public void ExtractClpFile(string clpPath, string outputPath)
    {
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        // HACK: Use 'tar' command line tool to extract the CLP file
        // Because SharpCompress gets weird and shits a lot of errors
        // When using .CLP files even if it is a tar file.
        // And yes, I know this is bad because if someone accidentally
        // remove 'tar' this fucked up the system.
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "tar",
                Arguments = $"-xzf \"{clpPath}\" -C \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
    }
}
