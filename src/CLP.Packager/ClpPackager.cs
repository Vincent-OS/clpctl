using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace CLP.Packager;

public class ClpPackager
{
    /// <summary>
    /// Extracts the contents of a CLP file to the specified output path.
    /// Treats .clp files as plain ZIP archives.
    /// </summary>
    /// <param name="clpPath">The path of the CLP file to be extracted.</param>
    /// <param name="outputPath">The path where the contents will be extracted.</param>
    /// <exception cref="FileNotFoundException">Thrown when the CLP file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the CLP file is not a valid ZIP archive.</exception>
    public void ExtractClpFile(string clpPath, string outputPath)
    {
        if (!File.Exists(clpPath))
        {
            throw new FileNotFoundException($"CLP file not found: {clpPath}");
        }

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        try
        {
            ZipFile.ExtractToDirectory(clpPath, outputPath, overwriteFiles: true);
        }
        catch (InvalidDataException ex)
        {
            throw new InvalidDataException($"Invalid CLP file format. The file may be corrupted or not a valid ZIP archive: {clpPath}", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException($"Insufficient permissions writing to '{outputPath}': {ex.Message}", ex);
        }
        catch (IOException ex)
        {
            throw new IOException($"Failed to extract CLP file to {outputPath}: {ex.Message}", ex);
        }
    }
}
