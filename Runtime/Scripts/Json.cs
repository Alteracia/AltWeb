using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Alteracia.Web
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class JsonPropertiesAttribute : Attribute
    {
        private string _jsonName;

        /// <summary>
        ///   <para></para>
        /// </summary>
        /// <param name="jsonName">The name of the field before renaming.</param>
        public JsonPropertiesAttribute(string jsonName) => this._jsonName = jsonName;

        /// <summary>
        ///   <para>The name of the field before the rename.</para>
        /// </summary>
        public string jsonName => this._jsonName;

        // Added by Nordup Ondar
        public static string TryGetAttributeName(System.Reflection.FieldInfo fieldInfo)
        {
            var attr = (JsonPropertiesAttribute) fieldInfo.GetCustomAttribute(typeof(JsonPropertiesAttribute), false);
            return attr == null ? "" : attr.jsonName;
        }
    }
    
    public class JsonStringArray { public string[] list; }
    public class JsonArray<T> { public T[] list; }
    
    public static class AltJson
    {
        // TODO
        // Write Comments
        // Divide into clear independent Methods use as: json.FormatIfJsonArray().FormatIfArrayInArray();
        // Rid of regex?
        
        // Added by Nordup Ondar
        public static bool IsSimple(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(type.GetGenericArguments()[0]);
            }
            return type.IsPrimitive 
                   || type.IsEnum
                   || type.Equals(typeof(string))
                   || type.Equals(typeof(decimal));
        }

        public static string FormatJsonText(this string json, Type systemType)
        {
            return FormatJson(json, systemType);
        }
        
        // Edit by Nordup Ondar
        public static string FormatJson(string json, Type systemType)
        {
            if (systemType == null) return json;
            json = json.FixOneFieldJson(systemType);
            
            var fields = GetFieldsRecursively(systemType);
            json = fields.Aggregate(json, (current, fieldInfo) => current.ReplaceFieldByName(fieldInfo));

            // If json is array wrap to JsonArray
            return json.FormatIfJsonArray().FormatIfArrayInArray();
        }

        // Added by Nordup Ondar
        public static List<FieldInfo> GetFieldsRecursively(Type type)
        {
            var fields = new List<FieldInfo>();
            if (type == null) return fields.ToList();

            var notChecked = new Queue<FieldInfo>(type.GetFields()); // TODO Check for DateTime What difference ?

            while (notChecked.Count > 0)
            {
                var current = notChecked.Dequeue();
                if (current == null) continue;
                
                // add current to list
                fields.Add(current);

                // if current is simple: continue
                var fType = current.FieldType.IsArray ? current.FieldType.GetElementType() : current.FieldType;
                if (fType == null || IsSimple(fType)) continue;

                // add to notChecked fields
                foreach (var fieldInfo in fType.GetFields())
                {
                    notChecked.Enqueue(fieldInfo);
                }
            }
            return fields;
        }
        
        // Added by Nordup Ondar
        public static string ReplaceFieldByName(this string json, System.Reflection.MemberInfo memberInfo)
        {
            Alteracia.Web.JsonPropertiesAttribute[] attrs =
                (Alteracia.Web.JsonPropertiesAttribute[]) memberInfo.GetCustomAttributes
                    (typeof(Alteracia.Web.JsonPropertiesAttribute), false);
            foreach (var attr in attrs)
            {
                json = Regex.Replace(json, $"\"({attr.jsonName}.*?)\"", "\"" + memberInfo.Name + "\"");
            }

            return json;
        }

        /*
         * Fix json from: "object". To: {"object"}
         */
        public static string FixOneFieldJson(this string json, System.Type systemType)
        {
            if (systemType.GetFields().Length != 1)
                return json;
            if (!json.StartsWith("\"") && !json.EndsWith("\""))
                return json;

            // Get Field
            var fieldName = (systemType.GetFields())[0].Name;
            // Return valid json
            return "{\"" + fieldName + "\":" + json + "}";
        }

        public static string FormatIfJsonArray(this string json)
        {
            return json.StartsWith("[") ? "{ \"list\": " + json + "}" : json;
        }

        public static string FormatIfArrayInArray(this string json)
        {
            // Line
            json = Regex.Replace(json, @"[\r*\n*]", "");
            // Rid of spaces
            json = Regex.Replace(json, @"\[(\s*?)\[", "[[");
            json = Regex.Replace(json, @"\](\s*?)\]", "]]");
            // TODO Rid of regex? OR change below code to regex
            json = json.Replace("[[", "{-->");
            json = json.Replace("]]", "<--}");
            string[] parts = json.Split(new string[] {"-->"}, StringSplitOptions.None);
            
            if (parts.Length == 1) return json;
            
            json = "";
            foreach (var part in parts)
            {
                string[] sub = part.Split(new string[] {"<--"}, StringSplitOptions.None);
                if (sub.Length < 2)
                {
                    json += part;
                    continue;
                }
                string newPart = "";
                string[] arrays = sub[0].Split(']');
                foreach (var array in arrays)
                {
                    string newArray = "";
                    string[] clean = array.Split('[');
                    string clear = clean[clean.Length > 1 ? 1 : 0];
                    clear = clear.Replace(',', ':');
                    newArray = clean.Length > 1 ? clean[0] + clear : clear;
                    newPart += newArray;
                }
                json += newPart + sub[1];
            }
            return json;
        }
    }
}
