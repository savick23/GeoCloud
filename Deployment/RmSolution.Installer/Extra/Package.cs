//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RmPackage – Создание пакета для установки и развёртывания.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Deployment
{
    #region Using
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml.Linq;
    #endregion Using

    internal class RmPackage
    {
        public void CreateCabinetFile()
        {
            XDocument xdoc;
            using (var ms = Assembly.GetExecutingAssembly().GetManifestResourceStream("RmSolution.Deployment.Properties.package.xml"))
                xdoc = XDocument.Load(ms);

            XDocument content = new XDocument(new XElement("RmSolution", new XElement("applications")));
            using (var output = new FileStream(xdoc.Root.Attribute("filename").Value, FileMode.Create))
            using (var cab = new ZipArchive(output, ZipArchiveMode.Update))
            {
                foreach (var app in xdoc.Root.Element("applications").Elements())
                {
                    var appid = app.Attribute("id").Value;
                    var config = app.Attribute("config")?.Value;
                    var folder = app.Attribute("publish").Value;
                    if (!folder.EndsWith("\\")) folder += "\\";

                    var appsect = new XElement("application",
                        new XAttribute("id", appid),
                        new XAttribute("path", app.Attribute("target").Value),
                        XElement.Parse(app.Element("service").ToString()));

                    if (config != null) appsect.Add(new XAttribute("config", config));

                    content.Root.Elements().First().Add(appsect);

                    foreach (var filename in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
                    {
                        var fn = Path.Combine(appid, filename.Replace(folder, string.Empty));
                        AddContent(cab, fn, File.ReadAllBytes(filename));
                    }
                }
                using (var writer = new StringWriter())
                {
                    content.Save(writer);
                    AddContent(cab, "[Content_Types].xml", Encoding.UTF8.GetBytes(writer.ToString()));
                }
            }
        }

        static void AddContent(ZipArchive archive, string filename, byte[] content)
        {
            ZipArchiveEntry file = archive.CreateEntry(filename);
            file.Open().Write(content, 0, content.Length);
        }
    }
}
