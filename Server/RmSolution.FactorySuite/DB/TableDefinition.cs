//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TableDefinition – Определение таблицы в БД.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System.Collections.Generic;
    #endregion Using

    /// <summary> Определение таблицы в БД.</summary>
    internal class TableDefinition
    {
        public string Name;
        public List<string> Columns;
        public List<string> Constraints;

        public TableDefinition(string tableName)
        {
            Name = tableName;
            Columns = new List<string>();
            Constraints = new List<string>();
        }
    }
}
