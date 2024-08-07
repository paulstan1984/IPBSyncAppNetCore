﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace IPBSyncAppNetCore.utils
{
    public class DateTimeConverter : DateTimeConverterBase
    {
        private readonly string format = "yyyy-MM-dd HH:mm:ss";

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((DateTime)value).ToString(format));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return DateTime.ParseExact((string)reader.Value, format, CultureInfo.InvariantCulture);
        }
    }
}
