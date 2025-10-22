using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace TedToolkit.Grasshopper.SourceGenerator;

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
internal static class ResxManager
{
    public static void Generate(string filePath, Dictionary<string, string> data)
    {
        if (File.Exists(filePath))
            File.SetAttributes(filePath, File.GetAttributes(filePath) & ~FileAttributes.ReadOnly);

        using var writer = XmlWriter.Create(filePath, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "\t"
        });
        writer.WriteStartDocument();
        writer.WriteStartElement("root");

        // resmimetype header
        writer.WriteStartElement("resheader");
        writer.WriteAttributeString("name", "resmimetype");
        writer.WriteElementString("value", "text/microsoft-resx");
        writer.WriteEndElement();

        // version header
        writer.WriteStartElement("resheader");
        writer.WriteAttributeString("name", "version");
        writer.WriteElementString("value", "1.3");
        writer.WriteEndElement();

        // reader header
        writer.WriteStartElement("resheader");
        writer.WriteAttributeString("name", "reader");
        writer.WriteElementString("value",
            "System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        writer.WriteEndElement();

        // writer header
        writer.WriteStartElement("resheader");
        writer.WriteAttributeString("name", "writer");
        writer.WriteElementString("value",
            "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        writer.WriteEndElement();

        // data element
        foreach (var item in data)
        {
            writer.WriteStartElement("data");
            writer.WriteAttributeString("name", item.Key);
            writer.WriteAttributeString("space", "http://www.w3.org/XML/1998/namespace", "preserve");
            writer.WriteElementString("value", item.Value);
            writer.WriteEndElement();
        }

        writer.WriteEndElement(); // root
        writer.WriteEndDocument();

        File.SetAttributes(filePath, FileAttributes.ReadOnly | File.GetAttributes(filePath));
    }
}