using CLP.Core;
using CLP.Packager;
using CLP.SystemIntegration;
using System;
using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml;

namespace CLP.CLI;

public class UpdateCommand
{
    private static readonly HttpClient client = new HttpClient();
    private List<string> serversList = MirrorsReader.Read();
    HttpResponseMessage response = null;
    string serverDbContent = null;
    private string? currentEdition = null;
    private string? currentArch = null;
    string? activeServer = null;

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
                activeServer = server;
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
					// We need to map the .NET architecture names to the Linux ones
                    currentArch = RuntimeInformation.ProcessArchitecture.ToString();
                    switch (currentArch)
                    {
                        case "X64":
                            currentArch = "x86_64";
                            break;
                        case "Arm64":
                            currentArch = "aarch64";
                            break;
                        default:
                            currentArch = currentArch.ToLower();
                            break;
                    }
                }
                // Backup the local database before overwriting
                var backupPath = $"/etc/CLP/CLP.db.bak";
                File.WriteAllText(backupPath, localDbContent);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(File.ReadAllText(localDbPath));
                var packageNodes = xmlDoc.SelectNodes("//Package"); // will search in all structure independent of XML file
                if (packageNodes == null || packageNodes.Count == 0)
                {
                    Console.WriteLine("No patches listed in Core LivePatch database.");
                    return 0;
                }
                else
                {
                    foreach (XmlNode node in packageNodes)
                    {
						ClpFile clpFile = new ClpFile();
                        if (string.IsNullOrWhiteSpace(clpFile.Name)) continue;
                        if (!string.Equals(clpFile.Version, currentEdition, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Skipping {clpFile.Name}: designed for '{clpFile.Version}' but current edition is '{currentEdition ?? "unknown"}'");
                            continue;
                        }
                        if (!string.Equals(clpFile.Architecture, currentArch, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Skipping {clpFile.Name}: designed for '{clpFile.Architecture}' but current architecture is '{currentArch ?? "unknown"}'");
                            continue;
                        }

                        var tmpDir = Directory.CreateTempSubdirectory("clp-update-").FullName;
                        var patchPath = Path.Combine(tmpDir, clpFile.Name + ".clp");
                        var patchUrl = $"{activeServer}{clpFile.Name}.clp";

                        HttpResponseMessage patchResponse = await client.GetAsync(patchUrl);
                        if (!patchResponse.IsSuccessStatusCode)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Error.WriteLine($"[ERROR] Failed to download patch {clpFile.Name} from servers.");
                            Console.ResetColor();
                            continue;
                        }
                        var patchData = await patchResponse.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(patchPath, patchData);

                        // Use InstallPatch from InstallCommand to install the patch
                        Console.WriteLine($"Downloaded patch: {clpFile.Name}");
                        var installPatch = new InstallCommand();
                        installPatch.InstallPatch(patchPath);
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
