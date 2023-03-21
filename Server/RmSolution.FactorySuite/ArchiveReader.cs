//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: ArchiveReader –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text.RegularExpressions;
    #endregion Using

    internal class ArchiveReader : IDisposable
    {
        static ArchiveReader _instance;
        FileStream _file;
        ZipArchive _arch;

        public ArchiveReader(string filename)
        {
            _instance = this;
            _file = new FileStream(filename, FileMode.Open);
            _arch = new ZipArchive(_file);
        }

        public string[] GetFiles(string path, string searchPattern)
        {
            searchPattern = searchPattern.Replace("*", @".*\") + "$";
            return _arch.Entries
                .Where(f => Path.GetDirectoryName(f.FullName).StartsWith(path) && Regex.IsMatch(Path.GetFileName(f.FullName), searchPattern))
                .Select(f => f.FullName).ToArray();
        }

        public byte[] ReadAllBytes(string filename) =>
            _arch.ReadArchiveBin(filename);

        public string ReadAllText(string filename) =>
            _arch.ReadArchive(filename);

        public void Dispose()
        {
            _instance = null;
            _arch.Dispose();
            _file.Close();
            _file.Dispose();
            GC.SuppressFinalize(this);
        }

        public static byte[] Read(string archiveName, string filename)
        {
            if (_instance != null)
                return _instance.ReadAllBytes(filename);

            using var fs = new FileStream(archiveName, FileMode.Open);
            using var arch = new ZipArchive(fs);
            foreach (var entry in arch.Entries)
                if (entry.FullName.StartsWith(filename))
                    return arch.ReadArchiveBin(entry.FullName);

            return null;
        }
    }
}
