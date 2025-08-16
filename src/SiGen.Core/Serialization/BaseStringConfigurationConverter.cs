using SiGen.Layouts.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SiGen.Serialization
{
    public class BaseStringConfigurationConverter : JsonConverter<BaseStringConfiguration>
    {
        public override BaseStringConfiguration? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var typeName = doc.RootElement.GetProperty("$type").GetString();

            Type? targetType = typeName switch
            {
                "Single" => typeof(SingleStringConfiguration),
                "Group" => typeof(StringGroupConfiguration),
                _ => throw new JsonException($"Unknown type: {typeName}")
            };

            var json = doc.RootElement.GetRawText();
            return (BaseStringConfiguration?)JsonSerializer.Deserialize(json, targetType, options);
        }

        public override void Write(Utf8JsonWriter writer, BaseStringConfiguration value, JsonSerializerOptions options)
        {
            var typeName = value.GetType().Name;
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(value, value.GetType(), options));
            writer.WriteStartObject();

            if (value is SingleStringConfiguration)
                writer.WriteString("$type", "Single");
            else if (value is StringGroupConfiguration)
                writer.WriteString("$type", "Group");
            else
                throw new JsonException($"Unknown type: {typeName}");

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                prop.WriteTo(writer);
            }
            writer.WriteEndObject();
        }
    }
}
