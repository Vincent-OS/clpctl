using CLP.Core;
using CLP.Packager;
using CLP.SystemIntegration;
using System;
using System.CommandLine;
using System.Diagnostics;
using System.Xml;

namespace CLP.CLI;

public class UpdateCommand
{
    private static readonly HttpClient client = new HttpClient();

    private List<string> serversList = new List<string>
    {
        "https://repo.v38armageddon.net/vincent-os/CLP/",
        "https://repo-fallback.v38armageddon.net/vincent-os/CLP/"
    };

    public async Task<int> UpdateDatabase()
    {
        Console.WriteLine("Updating CLP database...");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("clpctl/2.1 (Core LivePatch; Vincent OS)");
        
        // Get the latest version of the CLP database from the server and compare it to the local version
        // If the server version is newer, download and apply the patches
        var response = await client.GetAsync($"{serversList[0]}CLP.db");
        if (!response.IsSuccessStatusCode)
        {
            // Fallback to secondary server
            response = await client.GetAsync($"{serversList[1]}CLP.db");
            if (!response.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("[ERROR] Failed to fetch CLP database from both primary and fallback servers.");
                Console.ResetColor();
                return 101;
            }
        }
        var serverDbContent = await response.Content.ReadAsStringAsync();

        // Read the local CLP database
        var localDbPath = "/etc/CLP/CLP.db";
        var localDbContent = File.Exists(localDbPath) ? File.ReadAllText(localDbPath) : string.Empty;
        try
        {
            // Verification part
            if (!File.Exists(localDbPath))
            {
                Console.WriteLine("Local CLP database not found. Getting from server...");
                Directory.CreateDirectory(Path.GetDirectoryName(localDbPath));
                File.WriteAllText(localDbPath, serverDbContent);
            }
            if (serverDbContent != localDbContent)
            {
                Console.WriteLine("New patches available. Downloading and applying...");
                // Backup the local database before overwriting
                var backupPath = $"/etc/CLP/CLP.db.bak";
                File.WriteAllText(backupPath, localDbContent);
                // Overwrite the local database with the server version
                File.WriteAllText(localDbPath, serverDbContent);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(File.ReadAllText(localDbPath));
                var packageNodes = xmlDoc.SelectNodes("//Package");
                if (packageNodes == null || packageNodes.Count == 0)
                {
                    Console.WriteLine("No patches listed in CLP database.");
                }
                else
                {
                    foreach (XmlNode node in packageNodes)
                    {
                        var patchName = node.SelectSingleNode("Name")?.InnerText?.Trim();
                        if (string.IsNullOrWhiteSpace(patchName))
                        {
                            Console.WriteLine("Skipping package with empty name.");
                            continue;
                        }

                        var tmpDir = "/tmp/CLP";
                        if (!Directory.Exists(tmpDir))
                        {
                            Directory.CreateDirectory(tmpDir);
                        }

                        var patchPath = Path.Combine(tmpDir, patchName + ".clp");

                        // Try primary then fallback server for each patch
                        var patchUrlPrimary = $"{serversList[0]}{patchName}.clp";
                        var patchUrlFallback = $"{serversList[1]}{patchName}.clp";

                        HttpResponseMessage patchResponse = await client.GetAsync(patchUrlPrimary);
                        if (!patchResponse.IsSuccessStatusCode)
                        {
                            patchResponse = await client.GetAsync(patchUrlFallback);
                            if (!patchResponse.IsSuccessStatusCode)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Error.WriteLine($"[ERROR] Failed to download patch {patchName} from both primary and fallback servers.");
                                Console.ResetColor();
                                continue;
                            }
                        }
                        var patchData = await patchResponse.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(patchPath, patchData);

                        // Use InstallPatch from InstallCommand to install the patch
                        Console.WriteLine($"Downloaded patch: {patchName}");
                        var installPatch = new InstallCommand();
                        installPatch.InstallPatch(patchPath);
                    }
                }

                // Execute the installation scripts for each patch
                var patchesDirectory = Directory.GetDirectories("/opt/CLP");
                if (patchesDirectory.Length == 0)
                {
                    Console.WriteLine("No patches were downloaded; nothing to apply.");
                    return 0;
                }

                foreach (var patchDir in patchesDirectory)
                {
                    var installScriptPath = Path.Combine(patchDir, "Install-Patch.ps1");
                    if (File.Exists(installScriptPath))
                    {
                        PatchExecutor patchExecutor = new PatchExecutor();
                        patchExecutor.ApplyPatch(installScriptPath);
                        if (!patchExecutor.Success)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Error.WriteLine($"[ERROR] Error applying patch {installScriptPath}. Reverting...");
                            Console.ResetColor();
                            var revertScriptPath = Path.Combine(patchDir, "Remove-Patch.ps1");
                            if (File.Exists(revertScriptPath))
                            {
                                PatchExecutor revertExecutor = new PatchExecutor();
                                revertExecutor.ApplyPatch(revertScriptPath);
                                if (!revertExecutor.Success)
                                {
                                    throw new InvalidOperationException($"Failed to revert patch {revertScriptPath}. Manual intervention required!");
                                }
                            }
                            else
                            {
                                throw new FileNotFoundException($"No Remove-Patch.ps1 script found in {patchDir}. Manual intervention required!");
                            }
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException($"No Install-Patch.ps1 script found in {patchDir}. Manual intervention required!");
                    }
                }
                return 0;
            }
            else
            {
                Console.WriteLine("No new patches available.");
                return 0;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"[ERROR] An error occurred while updating CLP: {ex.Message}");
            Console.ResetColor();
            return 95;
        }
    }
}
