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
        private readonly long? _value;

        public TRefType(long? value)
        {
            _value = value;
        }

        public static implicit operator long(TRefType v) => v._value ?? 0;
        public static implicit operator long?(TRefType v) => v._value;
        public static implicit operator TRefType(long? v) => new(v);

        public override string ToString() => "#" + _value.ToString();
    }
}
