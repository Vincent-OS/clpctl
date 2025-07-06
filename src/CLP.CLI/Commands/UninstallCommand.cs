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
            // Execute the Remove-Patch.ps1 script
            var patchDir = Path.Combine("/opt/CLP", patch);
            var scriptPath = Path.Combine(patchDir, "Remove-Patch.ps1");
            if (File.Exists(scriptPath))
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "pwsh",
                        Arguments = $"{scriptPath}",
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
                    Console.Error.WriteLine($"Error executing script: {error}");
                    return;
                }
                Console.WriteLine(output);

                // At last, remove the patch directory
                Directory.Delete(patchDir, true);
            }
            else
            {
                Console.Error.WriteLine($"No Remove-Patch.ps1 script found in the patch directory. Manual intervention required!");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}