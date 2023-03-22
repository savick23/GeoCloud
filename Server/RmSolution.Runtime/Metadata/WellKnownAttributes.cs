//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: WellKnownAttributes – Известные наименования атрибутов (полей).
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    #region Using
    using System;
    #endregion Using

    /// <summary> Известные наименования атрибутов (полей).</summary>
    public static class WellKnownAttributes
    {
        public static readonly string Code = "Code";
        public static readonly string View = "View";
        public static readonly string DatabaseId = "DatabaseId";
        public static readonly string DataType = "DataType";
        public static readonly string DateTime = "Datetime";
        public static readonly string Description = "Descript";
        public static readonly string Dimension = "Dimension";
        public static readonly string Expression = "Expression";
        public static readonly string Definition = "Definition";
        public static readonly string Flags = "Flags";
        public static readonly string Id = "Id";
        public static readonly string Name = "Name";
        public static readonly string Label = "Label";
        public static readonly string Ordinal = "Ordinal";
        public static readonly string Parent = "Parent";
        public static readonly string Picket = "Picket";
        public static readonly string Site = "Site";
        public static readonly string Source = "Source";
        public static readonly string Model = "Model";
        public static readonly string State = "State";
        public static readonly string Type = "Type";
        /// <summary> Row version.</summary>
        public static readonly string Timestamp = "Version";
    }
}
