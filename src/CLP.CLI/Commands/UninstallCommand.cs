using KLP.Packager;
using System;
using System.CommandLine;
using System.Diagnostics;

namespace KLP.CLI;

public class UninstallCommand
{
    public void UninstallPatch(string patch)
    {
        Console.WriteLine($"Uninstalling patch {patch}...");
        Process.Start("klpctl", $"uninstall {patch}");
    }
}