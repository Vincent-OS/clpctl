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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"[ERROR] Patch {patch} is not installed.");
                Console.ResetColor();
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
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                if (process.ExitCode != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"[ERROR] Error executing script: {error}");
                    Console.Error.WriteLine($"[FATAL] Manual intervention required!");
                    Console.ResetColor();
                    return;
                }
                Console.WriteLine(output);

                // At last, remove the patch directory
                Directory.Delete(patchDir, true);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"[ERROR] No Remove-Patch.ps1 script found in the patch directory. Manual intervention required!");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"[ERROR] An error occurred: {ex.Message}");
            Console.ResetColor();
        }
    }
}