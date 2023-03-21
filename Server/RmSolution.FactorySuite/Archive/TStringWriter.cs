//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TStringWriter – Аналог StringWriter поддерживающий кодовую страницу.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System.IO;
    using System.Text;
    #endregion Using

    /// <summary> Аналог StringWriter поддерживающий кодовую страницу.</summary>
    public class TStringWriter : StringWriter
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
