using System.Collections.Generic;
using System.IO;

namespace CLP.Core;

public class ClpFile
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Architechture { get; set; }
    public string Description { get; set; }
    public string Depends { get; set; }

    //public static ClpFile FromFile(string filePath)
    //{
    //    var clpFile = new ClpFile();
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
    //                clpFile.Name = value;
    //                break;
    //            case "Version":
    //                clpFile.Version = value;
    //                break;
    //            case "Architechture":
    //                clpFile.Architechture = value;
    //                break;
    //            case "Description":
    //                clpFile.Description = value;
    //                break;
    //            case "Depends":
    //                clpFile.Depends = value;
    //                break;
    //        }
    //        return clpFile;
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
