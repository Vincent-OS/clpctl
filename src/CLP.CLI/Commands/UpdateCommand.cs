using CLP.Core;
using CLP.Packager;
using CLP.SystemIntegration;
using System;
using System.CommandLine;
using System.Diagnostics;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CLP.CLI;

public class UpdateCommand
{
    private static readonly HttpClient client = new HttpClient();

    public async Task UpdateDatabase()
    {
        Console.WriteLine("Updating CLP database...");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("clpctl/2.0 (Core LivePatch; Vincent OS)");
        
        // Get the latest version of the CLP database from the server and compare it to the local version
        // If the server version is newer, download and apply the patches
        var response = await client.GetAsync("https://repo.v38armageddon.net/vincent-os/CLP/CLP.db");
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Failed to update Core LivePatch database. Make sure you have an active Internet connection.");
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
                xmlDoc.LoadXml(localDbPath);
                var root = xmlDoc.DocumentElement;
                foreach (XmlNode node in root.SelectSingleNode("Name"))
                {
                    var patchName = node.InnerText.Trim();
                    var patchUrl = $"https://repo.v38armageddon.net/vincent-os/CLP/{patchName}.CLP";
                    var patchResponse = await client.GetAsync(patchUrl);
                    var patchPath = $"/tmp/CLP/{patchName}.CLP";
                    if (!Directory.Exists("/tmp/CLP"))
                    {
                        Directory.CreateDirectory("/tmp/CLP");
                    }
                    if (patchResponse.IsSuccessStatusCode)
                    {
                        var patchData = await patchResponse.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(patchPath, patchData);

                        // Ensure the patch has not been compromised
                        ChecksumUtility.ComputeChecksum(patchPath);

                        // Prepare the folder to /opt/CLP for extraction
                        // Should be handled by ClpPackager, .NET somethimes can be weird
                        if (!Directory.Exists($"/opt/CLP/{patchName}"))
                        {
                            Directory.CreateDirectory($"/opt/CLP/{patchName}");
                        }

                        // Call the packager to apply the patches
                        var packager = new ClpPackager();
                        packager.ExtractClpFile(patchPath, $"/opt/CLP/{patchName}");
                        Console.WriteLine($"Downloaded patch: {patchName}");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine($"[ERROR] Failed to download patch: {patchName}");
                        Console.ResetColor();
                    }
                    continue;
                }

                // Execute the installation scripts for each patch
                var patchesDirectory = Directory.GetDirectories("/opt/CLP");
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
                            Console.Error.WriteLine($"[ERROR] Error reverting patch {installScriptPath}.");
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
            }
            else
            {
                Console.WriteLine("No new patches available.");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"[ERROR] An error occurred while updating CLP: {ex.Message}");
            Console.ResetColor();
            return;
        }
    }
}
