using System.Text.Json;
using System.Text.Json.Serialization;
using CopyWords.Core.Models;

namespace CopyWords.Core.Converters
{
    /// <summary>
    /// Custom JSON converter for Headword that supports both "Translation" (new) and "Russian" (legacy) property names.
    /// </summary>
    public class HeadwordJsonConverter : JsonConverter<Headword>
    {
        public override Headword? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object");
            }

            string? original = null;
            string? english = null;
            string? translation = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected property name");
                }

                string propertyName = reader.GetString()!;
                reader.Read();

                switch (propertyName.ToLowerInvariant())
                {
                    case "original":
                        original = reader.GetString();
                        break;
                    case "english":
                        english = reader.GetString();
                        break;
                    case "translation":
                        // New property name takes precedence
                        translation = reader.GetString();
                        break;
                    case "russian":
                        // Legacy fallback - only use if Translation wasn't already set
                        translation ??= reader.GetString();
                        break;
                    default:
                        // Skip unknown properties
                        reader.Skip();
                        break;
                }
            }

            return new Headword(original!, english, translation);
        }

        public override void Write(Utf8JsonWriter writer, Headword value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Original", value.Original);

            if (value.English != null)
            {
                writer.WriteString("English", value.English);
            }
            else
            {
                writer.WriteNull("English");
            }

            if (value.Translation != null)
            {
                writer.WriteString("Translation", value.Translation);
            }
            else
            {
                writer.WriteNull("Translation");
            }

            writer.WriteEndObject();
        }
    }
}
