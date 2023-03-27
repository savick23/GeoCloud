//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TRefType – Ссылочный тип данных, 64-разрядный.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System;
    #endregion Using

    /// <summary> Ссылочный тип данных, 64-разрядный.</summary>
    public readonly struct TRefType
    {
        public readonly long? Value;

        public TRefType(long? value)
        {
            Value = value;
        }

        public static implicit operator long(TRefType v) => v.Value ?? 0;
        public static implicit operator long?(TRefType v) => v.Value;
        public static implicit operator TRefType(long? v) => new(v);

        public override string ToString() => "#" + Value.ToString();
    }

    /// <summary> Ссылочный тип данных, 64-разрядный.</summary>
    public readonly struct TRefType<T>
    {
        public readonly long? Value;

        public TRefType(long? value)
        {
            Value = value;
        }

        public static implicit operator long(TRefType<T> v) => v.Value ?? 0;
        public static implicit operator long?(TRefType<T> v) => v.Value;
        public static implicit operator TRefType<T>(long? v) => new(v);

        public override string ToString() => "#" + Value.ToString();
    }
}
