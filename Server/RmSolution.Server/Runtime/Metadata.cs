//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: Metadata – Метаданные.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Server
{
    #region Using
    using RmSolution.Runtime;
    using System.Text.RegularExpressions;
    #endregion Using

    internal class Metadata : IMetadata
    {
        #region Declarations

        IDatabase _db;

        #endregion Declarations

        #region Properties

        public string DatabaseName => _db.DatabaseName ?? "RMGEO01";

        #endregion Properties

        public Metadata(IDatabase connection)
        {
            _db = connection;
        }

        public void Open()
        {
            _db.Open();
        }
    }
}