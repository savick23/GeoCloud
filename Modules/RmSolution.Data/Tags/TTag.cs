//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TagBase – Базовый тэг данных.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using RmSolution.DataAnnotations;
    #endregion Using

    /// <summary> Базовый тип тэга. </summary>
    public abstract class TTag : TEntity
    {
        /// <summary> Короткий 32-разрядный ИД.</summary>
        public int TagId => (int)Id;
        /// <summary> Площадка. Вычисляемое поле</summary>
        public long Site { get; set; }
        /// <summary> Код типа сообщения из справочника.</summary>
        public long EventType { get; set; }
        public long DataType { get; set; }
        /// <summary> Источник данных. Оборудование. OPC Сервер.</summary>
        public long Provider { get; set; }
        /// <summary> Способ сохранения истории значений тэгов.</summary>
        public StorageMethod Storage { get; set; }
        /// <summary> Канал ввода/вывода, сигнал.</summary>
        public int Channel { get; set; }

        /// <summary> Отключение сигнализации. По умолчанию включена.</summary>
        public bool AlarmEnable { get; set; } = true;
        /// <summary> Список тревог.</summary>
        public TAlarmTag[] Alarms { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Hash(long kind, int channel = 0) => (kind << 8) + channel;

        /// <summary> Тип данных C#.</summary>
        public Type CType =>
            DataType switch
            {
                TType.Int16 => typeof(short),
                TType.Int32 => typeof(int),
                TType.Int64 => typeof(long),
                TType.Boolean => typeof(bool),
                TType.Float => typeof(float),
                TType.Double => typeof(double),
                TType.DateTime => typeof(DateTime),
                TType.Varchar => typeof(string),
                _ => typeof(long)
            };

        /// <summary> Преобразует в строку описания для сохранения в БД (поле Definition).</summary>
        public string ToDefinition() =>
            $"{{\"Id\":{Id},\"Parent\":{Parent},\"Type\":{(long)Type},\"Provider\":{Provider},\"Channel\":\"{Channel}\",\"Code\":\"{Code}\",\"Name\":\"{Name}\""
            + (string.IsNullOrWhiteSpace(Description) ? string.Empty : $",\"Description\":\"{Description}\"")
            + "}";
    }

    public sealed class TAnalogTag : TTag
    {
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public double? Deadband { get; set; }
    }

    public sealed class TDiscreteTag : TTag
    {
    }

    public sealed class TStringTag : TTag
    {
    }

    public sealed class TEventTag : TTag
    {
    }

    public sealed class TAlarmTag : ICloneable
    {
        public int TagId { get; set; }
        public DateTime DateTime { get; set; }
        public AlarmTagType Type { get; set; }
        public long Class { get; set; }
        public int Priority { get; set; }
        public double Value { get; set; }
        public string Message { get; set; }

        public long? Location { get; set; }
        public string Picket { get; set; }
        public long? Pass { get; set; }
        public long? Provider { get; set; }

        readonly static MethodInfo _memberwiseClone = typeof(TAlarmTag).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
        public object Clone() => _memberwiseClone.Invoke(this, null);
    }

    public enum TagEventType : long
    {
        /// <summary> Превышено минимальное значение.</summary>
        Minimum = 1,
        /// <summary> Возврат к нормальному значению.</summary>
        Normal = 2,
        /// <summary> Превышено максимальное значение.</summary>
        Maximum = 3
    }

    /// <summary> Способы сохранения истории значений тэгов.</summary>
    public enum StorageMethod
    {
        /// <summary> Не сохранять значения.</summary>
        NotStored = 0,
        /// <summary> Циклическая запись с заданным интервалом.</summary>
        Cyclic = 1,
        /// <summary> Сохранение изменений от предыдущего значения.</summary>
        Delta = 2,
        /// <summary> Сохранение всех изменений значений.</summary>
        Forced = 3
    }

    [JsonConverter(typeof(JsonAlarmTagTypeConverter))]
    public enum AlarmTagType
    {
        Normal = 0,
        LoLo = 10, Low = 11, HiHi = 12, High = 13, // для аналоговых тэгов
        On = 20, Off = 21,                         // для дискретных тэгов
        Action = 30, Close = 31, Ack = 32          // для событийных тэгов
    }

    sealed class JsonAlarmTagTypeConverter : JsonConverter<AlarmTagType>
    {
        public override AlarmTagType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return (AlarmTagType)Enum.Parse(typeof(AlarmTagType), reader.GetString(), true);

            return (AlarmTagType)reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, AlarmTagType value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}