//using System.Text.Json;
//using System.Text.Json.Serialization;
//using WarehouseAssistant.WebUI.Components;

//namespace WarehouseAssistant.WebUI;

//internal class TypeMappingConverter<TType, TImplementation> : JsonConverter<TType>
//    where TImplementation : TType
//{
//    public override TType? Read(
//        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options)
//    {
//        Console.WriteLine("TypeMappingConverter Read");
//        return JsonSerializer.Deserialize<TImplementation>(ref reader, options);
//    }

//    public override void Write(
//        Utf8JsonWriter writer, TType value, JsonSerializerOptions options)
//    {
//        Console.WriteLine("TypeMappingConverter Write");
//        JsonSerializer.Serialize(writer, (TImplementation)value!, options);
//    }
//}

//public class ObjectConverter : JsonConverter<object?>
//{
//    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        switch (reader.TokenType)
//        {
//            case JsonTokenType.String:
//                return reader.GetString();
//            case JsonTokenType.Number:
//                if (reader.TryGetInt32(out int intValue))
//                    return intValue;
//                if (reader.TryGetDouble(out double doubleValue))
//                    return doubleValue;
//                throw new JsonException("Number type is not supported.");
//            case JsonTokenType.Null:
//                return null;
//            case JsonTokenType.True:
//                return true;
//            case JsonTokenType.False:
//                return false;
//            default:
//                throw new JsonException($"Unexpected token type: {reader.TokenType}");
//        }
//    }

//    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
//    {
//        if (value == null)
//        {
//            writer.WriteNullValue();
//        }
//        else if (value is string stringValue)
//        {
//            writer.WriteStringValue(stringValue);
//        }
//        else if (value is int intValue)
//        {
//            writer.WriteNumberValue(intValue);
//        }
//        else if (value is double doubleValue)
//        {
//            writer.WriteNumberValue(doubleValue);
//        }
//        else if (value is bool boolValue)
//        {
//            writer.WriteBooleanValue(boolValue);
//        }
//        else
//        {
//            throw new JsonException($"Unexpected value type: {value.GetType()}");
//        }
//    }
//}

//internal class TypeMappingDummy : JsonConverter<FilterDataWrapper>
//{
//    public override FilterDataWrapper? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        FilterDataWrapper result = JsonSerializer.Deserialize<FilterDataWrapper>(ref reader, options);

//        JsonElement? element = result.Value as JsonElement?;

//        if (element.Value.ValueKind == JsonValueKind.Number)
//        {
//            result.Value = element.Value.GetInt32();
//        }

//        //while (reader.Read())
//        //{
//        //    Console.WriteLine(reader.TokenType);
//        //}

//        return result;
//    }

//    public override void Write(Utf8JsonWriter writer, FilterDataWrapper value, JsonSerializerOptions options)
//    {
//        Console.WriteLine(value.Value == null);

//        JsonSerializer.Serialize(writer, value!, options);
//    }
//}