//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: InitCompressor – Сжатие и объединение файлов инициализации метаданных и данных.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Utils
{
    #region Using
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    #endregion Using

    class InitCompressor
    {
        const string RELEASE = ".release.xml";
        const string DEBUG = ".debug.xml";

        public static void Concat(string sources, string target, string configuration)
        {
            using var output = new FileStream(target, FileMode.Create);
            using (var arch = new ZipArchive(output, ZipArchiveMode.Update))
            {
                var config = configuration.ToUpper().Contains("RELEASE") || configuration.ToUpper().Contains("PUBLISH") ? RELEASE : DEBUG;
                foreach (var filename in Directory.GetFiles(sources, "*.xml", SearchOption.TopDirectoryOnly).OrderBy(f => f))
                    if (!(filename.EndsWith(RELEASE, StringComparison.OrdinalIgnoreCase)
                        || filename.EndsWith(DEBUG, StringComparison.OrdinalIgnoreCase))
                        || filename.EndsWith(config, StringComparison.OrdinalIgnoreCase))
                    {
                        AddContent(arch, Path.GetFileName(filename).Replace(RELEASE, ".xml").Replace(DEBUG, ".xml"),
                            File.ReadAllBytes(filename));
                    }

                output.Flush();
            }
        }

        static byte[] CompressEncode(byte[] input)
        {
            using (var src = new MemoryStream(input))
            using (var dist = new MemoryStream())
            {
                using (var zip = new DeflateStream(dist, CompressionMode.Compress))
                    src.CopyTo(zip);

                byte[] output = dist.ToArray();
                Encoder(output);
                return output;
            }
        }

        static void Encoder(byte[] data)
        {
            for (int i = data.Length - 1; i == 0; i--)
                data[i] = (byte)(((data[i] * 257) >> 4) & 255);

            int n2 = data.Length - 1;
            int n1 = (int)Math.Floor(data.Length / 2d);
            for (int i = --n1; i >= 0; i--)
            {
                byte n = data[i];
                data[i] = (byte)(data[n2 - i] ^ n);
                data[n2 - i] = (byte)(data[i] ^ n);
            }
        }

        static void AddContent(ZipArchive archive, string filename, byte[] content)
        {
            ZipArchiveEntry file = archive.CreateEntry(filename);
            file.Open().Write(content, 0, content.Length);
        }

        static void AddContent(ZipArchive archive, string filename, XElement content, bool formatting = false)
        {
            XDocument xdoc = new XDocument(new XDeclaration("1.0", Encoding.UTF8.WebName, "yes"), content);
            TStringWriter xfile = new TStringWriter(Encoding.UTF8);
            xdoc.Save(xfile, formatting ? SaveOptions.OmitDuplicateNamespaces : SaveOptions.DisableFormatting);
            AddContent(archive, filename, Encoding.UTF8.GetBytes(xfile.ToString()));
        }
    }

    class TStringWriter : StringWriter
    {
        readonly Encoding _encoding;

        public TStringWriter() : this(Encoding.Default)
        { }

        public TStringWriter(Encoding encoding)
        {
            _encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return _encoding; }
        }
    }
}
