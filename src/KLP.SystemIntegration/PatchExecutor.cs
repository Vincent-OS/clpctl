using System.Diagnostics;

namespace KLP.SystemIntegration;

public class PatchExecutor
{
    public void ApplyPatch(string scriptPath)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = scriptPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        process.Start();
        Console.WriteLine(process.StandardOutput.ReadToEnd());
        Console.Error.WriteLine(process.StandardError.ReadToEnd());
        process.WaitForExit();
    }
}
