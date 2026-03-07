namespace CLP.Core;

public static class MirrorsReader
{
    public const string DefaultMirrorsPath = "/etc/CLP/mirrors";

    /// <summary>
    /// Returns the list of server URLs from the mirrors file. Lines starting with '#' are comments. Active servers use
    /// "Server = url".
    /// </summary>
    public static List<string> Read(string path = DefaultMirrorsPath)
    {
        if (!File.Exists(path))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] Mirror file not found at {path}.");
            Console.ResetColor();
            return new List<string> { null };
        }

        foreach (var url in File.ReadAllLines(path))
        {
            var trimmed = url.Trim();
            // Don't count lines starting with '#' as they are comments
            if (trimmed.StartsWith("#"))
            {
                continue;
            }
            if (trimmed.StartsWith("Server =", StringComparison.OrdinalIgnoreCase))
            {
                var serverUrl = trimmed.Substring("Server =".Length).Trim();
                if (!string.IsNullOrEmpty(serverUrl))
                {
                    return new List<string> { serverUrl };
                }
            }
        }
        return new List<string>();
    }
}
