using CLP.Core;
using CLP.Packager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLP.CLI
{
    public class CreateCommand
    {
        public void CreatePatch(string path, string name)
        {
            // Check if the specified path exists
            if (!Directory.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"[ERROR] The specified path {path} does not exist.");
                Console.ResetColor();
                return;
            }
            // Check if in the path, there is required files
            // Like PKGINFO.meta, Install-Patch.ps1, etc.
            var requiredFiles = new string[] { "PKGINFO.meta", "Install-Patch.ps1", "Remove-Patch.ps1" };
            foreach (var file in requiredFiles)
            {
                if (!File.Exists(Path.Combine(path, file)))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"[ERROR] The file {file} is missing in the specified path.");
                    Console.ResetColor();
                    return;
                }
            }

            // At last, create the patch
            ClpFile clpFile = ClpFile.FromFile(path, path);
            ClpPackager clpPackager = new ClpPackager();
            clpPackager.CreateClpFile($"{name}.clp", path);
        }
    }
}
