//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: CabReader –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Deployment
{
    #region Using
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    #endregion Using

    internal class CabReader : IDisposable
    {
        static CabReader _instance;
        ZipArchive _arch;

        public CabReader(Stream cab)
        {
            _instance = this;
            _arch = new ZipArchive(cab);
        }

        public string[] GetFiles(string path)
        {
            return _arch.Entries
                .Where(f => Path.GetDirectoryName(f.FullName).StartsWith(path))
                .Select(f => f.FullName).ToArray();
        }

        public byte[] ReadAllBytes(string filename) =>
            ReadArchiveBin(_arch, filename);

        public string ReadAllText(string filename) =>
            ReadArchive(_arch, filename);

        public void Dispose()
        {
            _instance = null;
            _arch.Dispose();
            GC.SuppressFinalize(this);
        }

        public static byte[] Read(string archiveName, string filename)
        {
            if (_instance != null)
                return _instance.ReadAllBytes(filename);

            using (var fs = new FileStream(archiveName, FileMode.Open))
            using (var arch = new ZipArchive(fs))
                foreach (var entry in arch.Entries)
                    if (entry.FullName.StartsWith(filename))
                        return ReadArchiveBin(arch, entry.FullName);

            return null;
        }

        static string ReadArchive(ZipArchive arch, string name)
        {
            ZipArchiveEntry menu = arch.GetEntry(name);
            byte[] buf = new byte[menu.Length];
            menu.Open().Read(buf, 0, buf.Length);
            byte[] buf1 = new byte[3];
            Array.Copy(buf, 0, buf1, 0, buf1.Length);
            byte[] buf2 = new byte[buf.Length - 3];
            Array.Copy(buf, 3, buf2, 0, buf2.Length);
            return Encoding.UTF8.GetString(buf1.SequenceEqual(new byte[] { 239, 187, 191 }) ? buf2 : buf);
        }

        static byte[] ReadArchiveBin(ZipArchive arch, string name)
        {
            ZipArchiveEntry menu = arch.GetEntry(name);
            byte[] buf = new byte[menu.Length];
            menu.Open().Read(buf, 0, buf.Length);
            return buf;
        }
    }
}
