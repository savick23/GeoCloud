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
    [TObject("Настройки конфигурации", "config.settings", Ordinal = 1)]
    public class TSettings
    {
        [TColumn("Идентификатор", Length = 64, IsKey = true)]
        public string Id { get; set; }
        [TColumn("Группа", Length = 32, Nullable = true)]
        public string? Parent { get; set; }
        [TColumn("Тип", Length = 32, DefaultValue = "string")]
        public string Type { get; set; }
        [TColumn("Значение", Length = 4000, Nullable = true)]
        public string? Value { get; set; }
        [TColumn("Изменено")]
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
