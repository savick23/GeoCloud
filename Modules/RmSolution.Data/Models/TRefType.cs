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
        public static readonly TRefType Empty = new();

        public readonly long? Id;
        public readonly string? Name;

        public TRefType(long? id)
        {
            Id = id;
        }

        public TRefType(long? id, string? name)
        {
            Id = id;
            Name = name;
        }

        public static implicit operator long(TRefType v) => v.Id ?? 0;
        public static implicit operator long?(TRefType v) => v.Id;
        public static implicit operator TRefType(long? v) => new(v);

        public override string ToString() => Name?.ToString();
    }
}
