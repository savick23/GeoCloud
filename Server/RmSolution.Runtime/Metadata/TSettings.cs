//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TSettings – Различные настройки конфигурации.
//--------------------------------------------------------------------------------------------------
#pragma warning disable CS8618
namespace RmSolution.Runtime
{
    #region Using
    using System;
    using System.Dynamic;
    using RmSolution.DataAnnotations;
    #endregion Using

    /// <summary> Различные настройки конфигурации.</summary>
    [Table("Настройки конфигурации", "config.settings", Ordinal = 1, IsSystem = true)]
    public class TSettings
    {
        [Column("Идентификатор", "id nvarchar(64) PRIMARY KEY", IsKey = true)]
        public string Id { get; set; }
        [Column("Группа", "parent nvarchar(32) NULL")]
        public string? Parent { get; set; }
        [Column("Тип", "type nvarchar(32) NOT NULL", Default = "string")]
        public string Type { get; set; }
        [Column("Значение", "value nvarchar(4000) NULL")]
        public string? Value { get; set; }
        [Column("Изменено", "modified datetime NOT NULL")]
        public DateTime Modified { get; set; }
    }

    internal class TSettingsWrapper : DynamicObject
    {
        readonly List<TSettings> _settings;

        public TSettingsWrapper(IEnumerable<TSettings>? settings)
        {
            _settings = settings?.ToList() ?? new();
        }

        public override IEnumerable<string> GetDynamicMemberNames() =>
            _settings.Select(p => string.Concat(xParent(p.Parent), p.Id)).ToList();

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            var prm = _settings.FirstOrDefault(s => s.Id.Equals(binder.Name));
            if (prm == null)
            {
                result = null;
                return false;
            }
            result = prm.Value;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            var prm = _settings.FirstOrDefault(s => s.Id.Equals(binder.Name));
            if (prm == null) return false;
            prm.Value = value?.ToString();
            return true;
        }

        string xParent(string? parent) =>
            string.IsNullOrWhiteSpace(parent)
                ? string.Empty
                : string.Concat(xParent(_settings.FirstOrDefault(p => p.Id.Equals(parent))?.Parent), parent, ".");
    }
}
#pragma warning restore CS8618
