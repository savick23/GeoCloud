// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: PgSqlDatabase –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System;
    #endregion Using

    public sealed class PgSqlDatabase : DatabaseFactorySuite
    {
        #region Declarations

        string _connstr;

        #endregion Declarations

        #region Properties

        public override string DefaultScheme => "public";

        #endregion Properties

        public PgSqlDatabase(string connectionString)
        {
            _connstr = connectionString;
        }

        #region IDatabase implementation

        public override IDatabase Open()
        {
            return null;
        }

        #endregion IDatabase implementation
    }
}
