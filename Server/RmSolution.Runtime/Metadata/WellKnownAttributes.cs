//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: WellKnownAttributes – Известные наименования атрибутов (полей).
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    #region Using
    using System;
    #endregion Using

    /// <summary> Известные наименования атрибутов (полей) в базе данных.</summary>
    public static class WellKnownAttributes
    {
        public static readonly string Id = "id";
        public static readonly string Parent = "parent";
        public static readonly string Type = "type";
        public static readonly string Code = "code";
        public static readonly string Name = "name";
        public static readonly string View = "view";
        public static readonly string DatabaseId = "databaseid";
        public static readonly string DataType = "datatype";
        public static readonly string DateTime = "datetime";
        public static readonly string Description = "descript";
        public static readonly string Dimension = "dimension";
        public static readonly string Expression = "expression";
        public static readonly string Definition = "definition";
        public static readonly string Flags = "flags";
        public static readonly string Label = "label";
        public static readonly string Ordinal = "ordinal";
        public static readonly string Picket = "picket";
        public static readonly string Site = "site";
        public static readonly string Source = "source";
        public static readonly string Model = "model";
        public static readonly string State = "state";
        /// <summary> Row version.</summary>
        public static readonly string Timestamp = "version";
    }
}
