using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.SaveSystem
{
    public class Vector2JsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            Vector2 vector = (Vector2)value!;
            
            float[] values = new float[2];
            values[0] = vector.x;
            values[1] = vector.y;
            serializer.Serialize(writer, values);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            float[] values = serializer.Deserialize<float[]>(reader)!;

            return new Vector2(values[0], values[1]);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector2);
        }
    }
}