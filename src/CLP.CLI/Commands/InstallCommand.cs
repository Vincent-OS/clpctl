using CLP.Core;
using CLP.Packager;
using System;
using System.CommandLine;
using System.Diagnostics;

namespace CLP.CLI;

public class InstallCommand
{
    public void InstallPatch(string file)
    {
        // Check if we are installing a valid patch
        if (!file.EndsWith(".CLP"))
        {
            Console.Error.WriteLine("Invalid patch format. Please provide a .CLP file.");
            return;
        }

        // Validate the patch file checksum
        ChecksumUtility.ComputeChecksum(file);

        // Extract the patch file
        Console.WriteLine($"Installing patch {file}...");
        var packager = new ClpPackager();
        try
        {
            if (Directory.Exists($"/opt/CLP/{file}"))
            {
                Console.WriteLine($"Patch {file} is already installed.");
                return;
            }
            packager.ExtractClpFile(file, $"/opt/CLP/{file}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}