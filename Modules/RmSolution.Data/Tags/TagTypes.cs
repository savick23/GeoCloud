//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TagBase – Базовый тэг данных.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System;
    #endregion Using

    public interface ITagValue
    {
        public int Id { get; set; }
        public DateTime Datetime { get; set; }
        public object Value { get; set; }
    }

    public interface ITagValue<T>
    {
        public int Id { get; set; }
        DateTime Datetime { get; set; }
        T Value { get; set; }
    }

    public struct AnalogTagInt32 : ITagValue<int?>, ITagValue
    {
        public int Id { get; set; }
        public DateTime Datetime { get; set; }
        public int? Value { get; set; }
        object ITagValue.Value { get => Value; set => Value = (int?)value; }
    }

    public struct AnalogTagInt64 : ITagValue<long?>, ITagValue
    {
        public int Id { get; set; }
        public DateTime Datetime { get; set; }
        public long? Value { get; set; }
        object ITagValue.Value { get => Value; set => Value = (long?)value; }
    }

    public struct AnalogTagFloat : ITagValue<float?>, ITagValue
    {
        public int Id { get; set; }
        public DateTime Datetime { get; set; }
        public float? Value { get; set; }
        object ITagValue.Value { get => Value; set => Value = (float?)value; }
    }

    public struct AnalogTagDouble : ITagValue<double?>, ITagValue
    {
        public int Id { get; set; }
        public DateTime Datetime { get; set; }
        public double? Value { get; set; }
        object ITagValue.Value { get => Value; set => Value = (double?)value; }
    }

    public struct AnalogTagDecimal : ITagValue<decimal?>, ITagValue
    {
        public int Id { get; set; }
        public DateTime Datetime { get; set; }
        public decimal? Value { get; set; }
        object ITagValue.Value { get => Value; set => Value = (decimal?)value; }
    }

    public struct DicreteTag : ITagValue<bool?>, ITagValue
    {
        public int Id { get; set; }
        public DateTime Datetime { get; set; }
        public bool? Value { get; set; }
        object ITagValue.Value { get => Value; set => Value = (bool?)value; }
    }

    public struct StringTag : ITagValue<string>, ITagValue
    {
        public int Id { get; set; }
        public DateTime Datetime { get; set; }
        public string Value { get; set; }
        object ITagValue.Value { get => Value; set => Value = (string)value; }
    }

    public struct EventTag : ITagValue<int?>, ITagValue
    {
        public int Id { get; set; }
        public DateTime Datetime { get; set; }
        public int? Value { get; set; }
        object ITagValue.Value { get => Value; set => Value = (int?)value; }
    }
}
