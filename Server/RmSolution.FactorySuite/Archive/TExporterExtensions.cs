//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TExporterExtensions –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System;
    using System.Xml.Linq;
    using System.IO.Compression;
    using System.Text;
    using System.IO;
    using System.Linq;
    #endregion Using

    public static class TExporterExtensions
    {
        public static string FileNameTemporary =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Templates), "sm" + (new Random().NextDouble() * 1000000000).ToString("#") + ".$$$");

        public static void AddContent(this ZipArchive archive, string filename, byte[] content)
        {
            ZipArchiveEntry file = archive.CreateEntry(filename);
            file.Open().Write(content, 0, content.Length);
        }

        public static void AddContent(this ZipArchive archive, string filename, XElement content, bool formatting = false)
        {
            XDocument xdoc = new XDocument(new XDeclaration("1.0", Encoding.UTF8.WebName, "yes"), content);
            TStringWriter xfile = new TStringWriter(Encoding.UTF8);
            xdoc.Save(xfile, formatting ? SaveOptions.OmitDuplicateNamespaces : SaveOptions.DisableFormatting);
            AddContent(archive, filename, Encoding.UTF8.GetBytes(xfile.ToString()));
        }

        public static string ReadArchive(this ZipArchive arch, string name)
        {
            ZipArchiveEntry menu = arch.GetEntry(name + (string.IsNullOrEmpty(Path.GetExtension(name)) ? ".xml" : string.Empty));
            byte[] buf = new byte[menu.Length];
            menu.Open().Read(buf, 0, buf.Length);
            return Encoding.UTF8.GetString(buf[0..3].SequenceEqual(new byte[] { 239, 187, 191 }) ? buf[3..] : buf);
        }

        public static byte[] ReadArchiveBin(this ZipArchive arch, string name)
        {
            ZipArchiveEntry menu = arch.GetEntry(name + (string.IsNullOrEmpty(Path.GetExtension(name)) ? ".xml" : string.Empty));
            byte[] buf = new byte[menu.Length];
            menu.Open().Read(buf, 0, buf.Length);
            return buf;
        }
    }
}
