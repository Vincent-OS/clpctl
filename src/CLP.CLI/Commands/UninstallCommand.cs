using CLP.Packager;
using System;
using System.CommandLine;
using System.Diagnostics;

namespace CLP.CLI;

public class UninstallCommand
{
    public void UninstallPatch(string patch)
    {
        Console.WriteLine($"Uninstalling patch {patch}...");
        try
        {
            if (!Directory.Exists($"/opt/CLP/{patch}"))
            {
                Console.Error.WriteLine($"Patch {patch} is not installed.");
                return;
            }
            // Get all the diff files between the original files and the patched files
            var diffFiles = Directory.GetFiles($"/opt/CLP/{patch}", "*.orig", SearchOption.AllDirectories);
            foreach (var diffFile in diffFiles)
            {
                var originalFile = diffFile.Replace(".orig", "");
                File.Copy(diffFile, originalFile, true);
                File.Delete(diffFile);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}