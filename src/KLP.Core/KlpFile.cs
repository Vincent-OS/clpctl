using System.Collections.Generic;
using System.IO;

namespace KLP.Core;

public class KlpFile
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Architechture { get; set; }
    public string Description { get; set; }
    public string Depends { get; set; }

    //public static KlpFile FromFile(string filePath)
    //{
    //    var klpFile = new KlpFile();
    //    foreach (var line in File.ReadLines(filePath))
    //    {
    //        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || !line.Contains("="))
    //        {
    //            continue;
    //        }
    //        var parts = line.Split("=", 2);
    //        var key = parts[0].Trim();
    //        var value = parts[1].Trim();
    //        switch (key)
    //        {
    //            case "Name":
    //                klpFile.Name = value;
    //                break;
    //            case "Version":
    //                klpFile.Version = value;
    //                break;
    //            case "Architechture":
    //                klpFile.Architechture = value;
    //                break;
    //            case "Description":
    //                klpFile.Description = value;
    //                break;
    //            case "Depends":
    //                klpFile.Depends = value;
    //                break;
    //        }
    //        return klpFile;
    //    }
    //}
    
    public void ToFile(string filePath)
    {
        var lines = new List<string>
        {
            "[Package]",
            $"Name={Name}",
            $"Version={Version}",
            $"Architechture={Architechture}",
            $"Description={Description}",
            $"Depends={Depends}"
        };
        File.WriteAllLines(filePath, lines);
    }
}
