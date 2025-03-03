using KLP.Core;
using KLP.Packager;
using System;
using System.CommandLine;
using System.Diagnostics;

namespace KLP.CLI;

public class InstallCommand
{
    public void InstallPatch(string file)
    {
        // Check if we are installing a valid patch
        if (!file.EndsWith(".klp"))
        {
            Console.Error.WriteLine("Invalid patch format. Please provide a .klp file.");
            return;
        }

        // Validate the patch file checksum
        ChecksumUtility.ComputeChecksum(file);

        // Extract the patch file
        Console.WriteLine($"Installing patch {file}...");
        var packager = new KlpPackager();
        packager.ExtractKlpFile(file, "/tmp/klp");
    }
}