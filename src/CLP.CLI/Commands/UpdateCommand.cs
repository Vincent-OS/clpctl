using CLP.Packager;
using System;
using System.CommandLine;
using System.Diagnostics;

namespace CLP.CLI;

public class UpdateCommand
{
    private static readonly HttpClient client = new HttpClient();

    public async Task UpdateDatabase()
    {
        Console.WriteLine("Updating CLP database...");

        // Get the latest version of the CLP database from the server and compare it to the local version
        // If the server version is newer, download and apply the patches
        var response = await client.GetAsync("https://repo.v38armageddon.net/vincent-os/CLP/CLP.db");
        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine("Failed to update CLP database. Make sure you have an active Internet connection.");
            return;
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

                File.WriteAllText(localDbPath, serverDbContent);
                foreach (var line in serverDbContent.Split('\n'))
                {
                    var patchName = line.Split('=')[1].Trim();
                    var patchUrl = $"https://repo.v38armageddon.net/vincent-os/CLP/{patchName}.CLP";
                    var patchResponse = await client.GetAsync(patchUrl);
                    var patchPath = $"/tmp/CLP/{patchName}.CLP";
                    if (line.StartsWith("Name="))
                    {
                        if (patchResponse.IsSuccessStatusCode)
                        {
                            var patchData = await patchResponse.Content.ReadAsByteArrayAsync();
                            File.WriteAllBytes(patchPath, patchData);
                            Console.WriteLine($"Downloaded patch: {patchName}");
                        }
                        else
                        {
                            Console.Error.WriteLine($"Failed to download patch: {patchName}");
                        }
                    }
                    // Call the packager to apply the patches
                    var packager = new ClpPackager();
                    packager.ExtractClpFile(patchPath, $"/opt/CLP/{patchName}");
                }
            }
            else
            {
                Console.WriteLine("No new patches available.");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred while updating CLP: {ex.Message}");
            return;
        }
    }
}
