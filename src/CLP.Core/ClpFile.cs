using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace CLP.Core;

public class ClpFile
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Architechture { get; set; }
    public string Description { get; set; }

    public static ClpFile FromFile(string filePath, string path)
    {
        var clpFile = new ClpFile();
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(filePath);

        var root = xmlDoc.DocumentElement;
        if (root == null)
            return clpFile;

        foreach (XmlNode node in root.ChildNodes)
        {
            switch (node.Name)
            {
                case "Name":
                    clpFile.Name = node.InnerText.Trim();
                    break;
                case "Version":
                    clpFile.Version = node.InnerText.Trim();
                    break;
                case "Architechture":
                    clpFile.Architechture = node.InnerText.Trim();
                    break;
                case "Description":
                    clpFile.Description = node.InnerText.Trim();
                    break;
            }
        }
        return clpFile;
    }
}
