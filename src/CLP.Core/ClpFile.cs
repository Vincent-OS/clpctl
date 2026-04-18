using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace CLP.Core;

public class ClpFile
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Architecture { get; set; }
    public string Description { get; set; }

    public static ClpFile FromFile(string filePath)
    {
        var clpFile = new ClpFile();
        var doc = XDocument.Load(filePath);
        var rootX = doc.Root;
        clpFile.Name = rootX?.Element("Name")?.Value?.Trim();
        clpFile.Version = rootX?.Element("Version")?.Value?.Trim();
        clpFile.Architecture = rootX?.Element("Architecture")?.Value?.Trim();
        clpFile.Description = rootX?.Element("Description")?.Value?.Trim();

        return clpFile;
    }
}
