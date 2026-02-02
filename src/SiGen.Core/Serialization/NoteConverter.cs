using SiGen.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SiGen.Serialization
{
    public class NoteConverter : JsonConverter<NoteAndOctave>
    {
        public override NoteAndOctave Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string str = reader.GetString() ?? throw new JsonException("Expected a string value for NoteAndOctave.");
                if (NoteAndOctave.TryParse(str, out var result))
                    return result;
                throw new JsonException($"Invalid NoteAndOctave format: {str}");
            }
            throw new JsonException("Expected a string value for NoteAndOctave.");
        }

        public override void Write(Utf8JsonWriter writer, NoteAndOctave value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToShortString());
        }
    }

    public class NullableNoteConverter : JsonConverter<NoteAndOctave?>
    {
        public override NoteAndOctave? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string str = reader.GetString() ?? throw new JsonException("Expected a string value for NoteAndOctave.");
                if (string.IsNullOrEmpty(str))
                    return null;

                if (NoteAndOctave.TryParse(str, out var result))
                    return result;
                throw new JsonException($"Invalid NoteAndOctave format: {str}");
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                string? str = reader.GetString();
                return null;
            }
            throw new JsonException("Expected a string value for NoteAndOctave.");
        }

        public override void Write(Utf8JsonWriter writer, NoteAndOctave? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToShortString());
        }
    }
}
