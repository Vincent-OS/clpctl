using KLP.Packager;
using System;
using System.CommandLine;
using System.Diagnostics;

namespace KLP.CLI;

public class  ListCommand
{
    public void ListInstalledPatches()
    {
        Console.WriteLine("Installed patches:");
        // Get the list of installed patches in the /opt/klp directory and return as a list
        var patchesDirectory = Directory.GetDirectories("/opt/klp");
        foreach (var patch in patchesDirectory)
        {
            Console.WriteLine(patch);
        }
    }
}