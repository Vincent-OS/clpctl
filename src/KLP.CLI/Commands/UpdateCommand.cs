using KLP.Packager;
using System;
using System.CommandLine;
using System.Diagnostics;

namespace KLP.CLI;

public class UpdateCommand
{
    public async void UpdateDatabase()
    {
        Console.WriteLine("Updating KLP database...");

        // Get the latest version of the klp database from the server and compare it to the local version
        // If the server version is newer, download and apply the patches
        HttpClient client = new HttpClient();
        var response = await client.GetAsync("https://repo.v38armageddon.net/vincent-os/KLP/klp.db");
        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine("Failed to update KLP database. Make sure you have an active Internet connection.");
            return;
        }

        var serverDbContent = await response.Content.ReadAsStringAsync();

        // Read the local klp database
        var localDbPath = File.ReadAllText("/etc/klp/klp.db");
        var localDbContent = File.Exists(localDbPath) ? File.ReadAllText(localDbPath) : string.Empty;

        if (serverDbContent != localDbContent)
        {
            Console.WriteLine("New patches available. Downloading and applying...");

            File.WriteAllText(localDbPath, serverDbContent);
            foreach (var line in serverDbContent.Split('\n'))
            {
                if (line.StartsWith("Name="))
                {
                    var patchName = line.Split('=')[1].Trim();
                    var patchUrl = $"https://repo.v38armageddon.net/vincent-os/KLP/{patchName}.klp";
                    var patchResponse = await client.GetAsync(patchUrl);

                    if (patchResponse.IsSuccessStatusCode)
                    {
                        var patchData = await patchResponse.Content.ReadAsByteArrayAsync();
                        var patchPath = $"/tmp/klp/{patchName}.klp";
                        File.WriteAllBytes(patchPath, patchData);
                        Console.WriteLine($"Downloaded patch: {patchName}");
                    }
                    else
                    {
                        Console.Error.WriteLine($"Failed to download patch: {patchName}");
                    }
                }

                // Call the packager to apply the patches
                var packager = new KlpPackager();
                //packager.ExtractKlpFile(patchPath, "/opt/klp");
            }
        }
        else
        {
            Console.WriteLine("No new patches available.");
        }
    }
}
