using CLP.Core;
using CLP.Packager;
using System;
using System.CommandLine;
using System.Diagnostics;

namespace CLP.CLI;

public class InstallCommand
{
    public int InstallPatch(string file)
    {
        // Required checks
        if (!file.EndsWith(".clp", StringComparison.OrdinalIgnoreCase))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("[ERROR] Invalid patch format. Please provide a '.clp' file.");
            Console.ResetColor();
            return 8;
        }
        if (!File.Exists(file))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"[ERROR] The specified patch file {file} does not exist.");
            Console.ResetColor();
            return 2;
        }

        ChecksumUtility.ComputeChecksum(file);
        var fileName = Path.GetFileName(file);
        Console.WriteLine($"Installing patch {fileName}...");
        var packager = new ClpPackager();
        try
        {
            if (Directory.Exists($"/opt/CLP/{fileName}"))
            {
                Console.WriteLine($"Patch {fileName} is already installed.");
                return 17;
            }
            Directory.CreateDirectory($"/opt/CLP/{fileName}");
            packager.ExtractClpFile(file, $"/opt/CLP/{fileName}");

            // Get on the new directory and execute the Install-Patch.ps1 script
            var patchDir = Path.Combine("/opt/CLP", fileName);
            var scriptPath = Path.Combine(patchDir, "Install-Patch.ps1");
            if (File.Exists(scriptPath))
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "pwsh",
                        Arguments = $"-File {scriptPath}",
                        WorkingDirectory = patchDir,
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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"[ERROR] Error executing script: {error}");
                    Console.ResetColor();
                }
                Console.WriteLine(output);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"[ERROR] No Install-Patch.ps1 script found in the patch directory.");
                Console.ResetColor();
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"[ERROR] An error occurred: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }
}