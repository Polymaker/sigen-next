using SiGen.Measuring;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SiGen.Serialization
{
    public class MeasureConverter : JsonConverter<Measure>
    {
        public override Measure? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (MeasureParser.TryParse(str, out var measure))
                return measure;
            return null;
        }

        public override void Write(Utf8JsonWriter writer, Measure value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToStringFormatted(CultureInfo.InvariantCulture, true));
        }
    }
}
