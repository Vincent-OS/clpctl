using System.Diagnostics;

namespace CLP.SystemIntegration;

public class PatchExecutor
{
    public bool Success { get; set; } = false;

    public void ApplyPatch(string scriptPath)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "pwsh",
                Arguments = scriptPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        if (process.ExitCode != 0)
            Success = false;
        Success = true;
    }
}
