using CLP.Packager;
using System;
using System.CommandLine;
using System.Diagnostics;

namespace CLP.CLI;

public class UninstallCommand
{
    public int UninstallPatch(string patch)
    {
        Console.WriteLine($"Uninstalling patch {patch}...");
        try
        {
            if (!Directory.Exists($"/opt/CLP/{patch}"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"[ERROR] Patch {patch} is not installed.");
                Console.ResetColor();
                return 65;
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
                    Console.Error.WriteLine($"[FATAL] Manual intervention required!");
                    Console.ResetColor();
                    return 1;
                }
                Console.WriteLine(output);

                // At last, remove the patch directory
                Directory.Delete(patchDir, true);
                return 0;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"[ERROR] No Remove-Patch.ps1 script found in the patch directory. Manual intervention required!");
                Console.ResetColor();
                return 2;
            }
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