﻿using System.Collections;
using System.ComponentModel;

namespace Stenguage.Json
{
    public static class JsonWriter
    {
        public static string ToJson(this object obj)
        {
            if (obj is null) return "null";
            string json = "";
            Type type = obj.GetType();
            if (type == typeof(string) || type == typeof(char))
            {
                json += "\"";
                string str = obj.ToString();
                if (str == null) return "";
                foreach (char c in str)
                {
                    if (c < 32 || c == '\"' || c == '\\')
                    {
                        if ("\"\\/\n\r\t\b\f".Contains(c)) json += "\\" + "\"\\/nrtbf"["\"\\/\n\r\t\b\f".IndexOf(c)];
                        else json += "\\u" + ((int)c).ToString("D4");
                    }
                    else json += c;
                }
                json += "\"";
            }
            else if (type == typeof(byte) || type == typeof(long) || type == typeof(int) || type == typeof(float) || type == typeof(short) ||
                     type == typeof(sbyte) || type == typeof(ulong) || type == typeof(uint) || type == typeof(double) || type == typeof(ushort) || type == typeof(decimal)) json += obj.ToString();
            else if (type == typeof(bool)) json += (bool)obj ? "true" : "false";
            else if (type == typeof(DateTime) || type.IsEnum) json += $"\"{obj}\"";
            else if (obj is IList list)
            {
                json += "[";
                foreach (object item in list)
                {
                    if (json[json.Length - 1] != '[') json += ",";
                    json += item.ToJson();
                }
                json += "]";
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                if (type.GetGenericArguments()[0] != typeof(string)) return json + "{}";
                json += "{";
                foreach (DictionaryEntry item in (IDictionary)obj)
                {
                    if (json[json.Length - 1] != '{') json += ",";
                    json += $"{item.Key.ToJson()}:{item.Value?.ToJson()}";
                }
                json += "}";
            }
            else
            {
                json += "{";
                foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(obj))
                {
                    if (json[json.Length - 1] != '{') json += ",";
                    json += $"\"{prop.Name}\":{prop.GetValue(obj)?.ToJson()}";
                }
                json += "}";
            }
            return json;
        }
    }
}
