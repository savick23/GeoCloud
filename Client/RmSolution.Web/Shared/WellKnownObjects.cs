namespace RmSolution.Web
{
    using System;

    public static class WellKnownObjects
    {
        public static readonly string Role = "config.roles";
        public static readonly string User = "config.users";
        public static readonly string EquipmentType = "equiptypes";
        public static readonly string Equipment = "equipments";

        public static class Api
        {
            public static readonly string GetObjects = "objects";
            public static readonly string GetObject = "object/";
            public static readonly string GetData = "data/";
            public static readonly string GetDataTable = "rows/";
            public static readonly string GetReference = "reference/";
            public static readonly string PostNewItem = "new";
            public static readonly string PostUpdateItem = "update";
        }
    }
}
