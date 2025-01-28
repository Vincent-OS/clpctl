using System.Security.Cryptography;

namespace KLP.Core;

public static class ChecksumUtility
{
    public static string ComputeChecksum(string filePath)
    {
        using var sha256 = SHA256.Create();
        try
        {
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        catch (FileNotFoundException ex)
        {
            // Handle file not found exception
            Console.Error.WriteLine($"File not found: {ex.Message}");
            return string.Empty;
        }
        catch (UnauthorizedAccessException ex)
        {
            // Handle unauthorized access exception
            Console.Error.WriteLine($"Access denied: {ex.Message}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
            return string.Empty;
        }
    }
}