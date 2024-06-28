using System.Text.Json;
using System.Text.Json.Serialization;

namespace TradesTracker.Converters
{
    internal sealed class UInt128Converter : JsonConverter<UInt128>
    {
        public override UInt128 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            return value == null ? 0 : UInt128.Parse(value);
        }

        public override void Write(Utf8JsonWriter writer, UInt128 value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
