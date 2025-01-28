using KLP.Packager;
using System;
using System.CommandLine;
using System.Diagnostics;

namespace KLP.CLI;

public class InstallCommand
{
    public void InstallPatch(string file)
    {
        Console.WriteLine($"Installing patch {file}...");
        var packager = new KlpPackager();
        packager.ExtractKlpFile(file, "/tmp/klp");
    }
}