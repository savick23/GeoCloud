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
        [Column("Группа", "group nvarchar(32) NULL")]
        public string Parent { get; set; }
        [Column("Значение", "value nvarchar(4000) NULL")]
        public string Value { get; set; }
        [Column("Значение", "modified datetime NOT NULL")]
        public DateTime Modified { get; set; }
    }

    internal class TSettingsWrapper : DynamicObject
    {
        readonly List<TSettings> _settings;

        public TSettingsWrapper(IEnumerable<TSettings>? settings)
        {
            _settings = settings?.ToList() ?? new();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            result = _settings.FirstOrDefault(s => s.Id.Equals(binder.Name))?.Value;
            return true;
        }
    }
}
#pragma warning restore CS8618
