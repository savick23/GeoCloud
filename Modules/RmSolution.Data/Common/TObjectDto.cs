//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TObjectDto – Класс обмена метаданными.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System.Text.Json.Serialization;
    using System.Text.Json;
    using RmSolution.DataAnnotations;
    #endregion Using

    /// <summary> Класс обмена метаданными.</summary>
    public class TObjectDto
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public TObjectFlags Flags { get; set; }

        public TAttributeDto[] Attributes {get;set;}
    }

    public class TAttributeDto
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string DisplayField { get; set; }
        public bool Visible { get; set; }
        public TAttributeFlags Flags { get; set; }
    }

    /// <summary> JsonConverters </summary>
    public class TRefTypeConverter : JsonConverter<TRefType>
    {
        public override TRefType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? view = null;
            long? id = 0;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.Number:
                        id = reader.GetInt64();
                        break;
                    case JsonTokenType.String:
                        view = reader.GetString();
                        break;
                    case JsonTokenType.EndObject:
                        return new TRefType(id, view);
                }
            }
            return TRefType.Empty;
        }

        public override void Write(Utf8JsonWriter writer, TRefType value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("value", value.Value ?? -1);
            writer.WriteString("view", value.View ?? null);
            writer.WriteEndObject();
        }
    }
}