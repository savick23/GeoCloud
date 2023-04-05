//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TType – Типы.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    /// <summary> Типы.</summary>
    public static class TType
    {
        /// <summary> Конфигурация.</summary>
        public static readonly long Solution = 1;
        /// <summary> Компоненты.</summary>
        public static readonly long Component = 2;
        /// <summary> Объекты конфигурации (метаданных).</summary>
        public static readonly long Object = 3;
        /// <summary> Простой тип.</summary>
        public static readonly long SimpleType = 4;
        /// <summary> Обработчик.</summary>
        public static readonly long Handler = 5;
        /// <summary> Приложение.</summary>
        public static readonly long Application = 6;
        /// <summary> Модуль.</summary>
        public static readonly long Module = 7;
        /// <summary> Настройки.</summary>
        public static readonly long Settings = 8;
        /// <summary> Константа.</summary>
        public static readonly long Constant = 9;
        /// <summary> Перечисление.</summary>
        public static readonly long Enum = 10;
        /// <summary> Справочник.</summary>
        public static readonly long Catalog = 11;
        /// <summary> Документ.</summary>
        public static readonly long Document = 12;
        /// <summary> Журнал.</summary>
        public static readonly long Journal = 13;
        /// <summary> Отчёт.</summary>
        public static readonly long Report = 14;
        /// <summary> Регистр.</summary>
        public static readonly long Register = 15;
        /// <summary> Регистр учётный.</summary>
        public static readonly long Account = 16;
        /// <summary> Измерения для регистров.</summary>
        public static readonly long Dimension = 17;
        /// <summary> Табличная часть, детали.</summary>
        public static readonly long Details = 18;
        /// <summary> Запрос.</summary>
        /// <remarks> Заранее продготовленные запросы данных, представления.</remarks>
        public static readonly long Query = 19;
        /// <summary> Реквизит.</summary>
        public static readonly long Attribute = 20;
        /// <summary> Меню.</summary>
        public static readonly long Menu = 21;
        /// <summary> Панели инструментов.</summary>
        public static readonly long Tool = 22;
        /// <summary> Словарь тэгов.</summary>
        public static readonly long Tag = 32;

        public static readonly long RefType = 65;
        public static readonly long Byte = 65;
        public static readonly long Int16 = 66;
        public static readonly long Int32 = 67;
        public static readonly long Int64 = 68;
        public static readonly long Float = 69;
        public static readonly long Double = 70;
        public static readonly long Decimal = 71;
        public static readonly long Numeric = 72;
        public static readonly long Char = 73;
        public static readonly long NChar = 74;
        public static readonly long Varchar = 75;
        public static readonly long NVarchar = 76;
        public static readonly long DateTime = 77;
        public static readonly long Date = 78;
        public static readonly long Time = 79;
        public static readonly long Boolean = 80;
        public static readonly long Binary = 81;
        public static readonly long VarBinary = 82;
        public static readonly long Guid = 83;
        public static readonly long TimeStamp = 84;
        public static readonly long Array = 85;
        public static readonly long Xml = 86;
        public static readonly long Json = 87;

        /// <summary> Идентификатор новой записи.</summary>
        public static readonly long NewId = 0xFFFFFFFFFC00;
    }
}
