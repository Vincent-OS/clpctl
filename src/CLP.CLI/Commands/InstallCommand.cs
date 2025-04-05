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

            // Get on the new directory and execute the patch.sh script
            var patchDir = Path.Combine("/opt/CLP", file);
            var scriptPath = Path.Combine(patchDir, "install.sh");
            if (File.Exists(scriptPath))
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "bash",
                        Arguments = scriptPath,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    Console.Error.WriteLine($"Error executing script: {error}");
                }
                else
                {
                    Console.WriteLine($"Patch {file} installed successfully.");
                }
            }
            else
            {
                Console.Error.WriteLine($"No install.sh script found in the patch directory.");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}