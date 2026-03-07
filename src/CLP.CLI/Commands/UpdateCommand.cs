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
    private List<string> serversList = MirrorsReader.Read();
    HttpResponseMessage response = null;
    string serverDbContent = null;
    private string? currentEdition = null;

    public async Task<int> UpdateDatabase()
    {
        Console.WriteLine("Updating Core LivePatch database...");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("clpctl/2.1 (Core LivePatch; Vincent OS)");
        if (serversList == null || serversList.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("[ERROR] No Core LivePatch servers configured. Please check your configuration.");
            Console.ResetColor();
            return 2;
        }
        // Get the latest version of the CLP database from the server and compare it to the local version
        // If the server version is newer, download and apply the patches
        foreach (var server in serversList)
        {
            response = await client.GetAsync($"{server}CLP.db");
            if (response.IsSuccessStatusCode)
            {
                serverDbContent = await response.Content.ReadAsStringAsync();
                break;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"[ERROR] Failed to fetch Core LivePatch database from {string.Join(", ", serversList)}.");
                Console.ResetColor();
                return 101;
            }
        }

        // Read the local CLP database
        var localDbPath = "/etc/CLP/CLP.db";
        var localDbContent = File.Exists(localDbPath) ? File.ReadAllText(localDbPath) : string.Empty;
        try
        {
            // Verification part
            if (!File.Exists(localDbPath))
            {
                Console.WriteLine("Local Core LivePatch database not found. Getting from server...");
                Directory.CreateDirectory(Path.GetDirectoryName(localDbPath));
                File.WriteAllText(localDbPath, serverDbContent);
            }
            if (serverDbContent != localDbContent)
            {
                Console.WriteLine("New patches available. Downloading and applying...");
                if (File.Exists("/etc/os-release"))
                {
                    currentEdition = File.ReadAllLines("/etc/os-release")
                        .FirstOrDefault(line => line.StartsWith("VARIANT_ID="))?
                        .Split('=')?
                        .ElementAtOrDefault(1)?
                        .Trim('"');
                }
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
                    Console.WriteLine("No patches listed in Core LivePatch database.");
                    return 0;
                }
                else
                {
                    foreach (XmlNode node in packageNodes)
                    {
                        var patchName = node.SelectSingleNode("Name")?.InnerText?.Trim();
                        var patchVersion = node.SelectSingleNode("Version")?.InnerText?.Trim();
                        if (string.IsNullOrWhiteSpace(patchName)) continue;
                        if (!string.Equals(patchVersion, currentEdition, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Skipping {patchName}: designed for '{patchVersion}' but current edition is '{currentEdition ?? "unknown"}'");
                            continue;
                        }

                        var tmpDir = Directory.CreateTempSubdirectory("clp-update-").FullName;
                        var patchPath = Path.Combine(tmpDir, patchName + ".clp");
                        var patchUrl = $"{serversList}{patchName}.clp";

                        HttpResponseMessage patchResponse = await client.GetAsync(patchUrl);
                        if (!patchResponse.IsSuccessStatusCode)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Error.WriteLine($"[ERROR] Failed to download patch {patchName} from servers.");
                            Console.ResetColor();
                            continue;
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
            Console.Error.WriteLine($"[ERROR] An error occurred while updating Core LivePatch: {ex.Message}");
            Console.ResetColor();
            return 95;
        }
    }
}
