using CLP.Packager;
using System;
using System.CommandLine;
using System.Diagnostics;

namespace CLP.CLI;

public class  ListCommand
{
    public void ListInstalledPatches()
    {
        Console.WriteLine("Installed patches:");
        // Get the list of installed patches in the /opt/CLP directory and return as a list
        var patchesDirectory = Directory.GetDirectories("/opt/CLP");
        foreach (var patch in patchesDirectory)
        {
            Console.WriteLine(patch);
        }
    }
}