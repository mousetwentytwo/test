using System;
using System.Text.RegularExpressions;
using System.Linq;
using Neurotoxin.Godspeed.Core.Attributes;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    /// <summary>
    /// Enum helper class works with enums that use StringValue attribute on their fields.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Gets the value from the instance of an enum based on it's StringValue attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enum">The @enum.</param>
        /// <returns></returns>
        public static string GetStringValue<T>(T @enum)
        {
            var enumField = @enum.GetType().GetField(@enum.ToString());
            var stringValueAttribute = (StringValueAttribute)enumField.GetCustomAttributes(typeof(StringValueAttribute), false).FirstOrDefault();
            return stringValueAttribute == null ? @enum.ToString() : stringValueAttribute.Value;
        }

        /// <summary>
        /// Gets the field based on the string value and type of enum. It will try to match value against 
        /// StringValue attribute, if it doesn't find a match, it will look for a field with the name
        /// specified as a string. Before it does comparison, it strips whitespaces from the string, and 
        /// converts it to lowercase.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stringValue">The string value.</param>
        /// <returns></returns>
        public static T GetField<T>(string stringValue)
        {
            var type = typeof(T);
            var fields = type.GetFields();

            foreach (var fieldInfo in fields)
            {
                var stringValueAttribute =
                    (StringValueAttribute)
                    fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false).FirstOrDefault();

                if (stringValueAttribute != null && stringValueAttribute.Value == stringValue)
                {
                    return (T)fieldInfo.GetValue(type);
                }

                if (fieldInfo.Name.ToLower().Equals(SanitiseString(stringValue).ToLower()))
                {
                    return (T)fieldInfo.GetValue(type);
                }
            }
            var exceptionString = string.Format("None of the fields in '{0}' match search value '{1}'", type.FullName,
                                                stringValue);
            throw new ArgumentException(exceptionString);
        }

        private static string SanitiseString(string text)
        {
            var returnText = new Regex("[^a-zA-Z0-9]+").Replace(text, "");
            return returnText;
        }
    }
}