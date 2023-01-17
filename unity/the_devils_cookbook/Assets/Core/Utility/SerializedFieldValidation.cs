using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TDC.Core.Utility
{
    public class SerializedValueRequiredAttribute : Attribute {}
    public static class SerializedFieldValidation
    {
        /// <summary>
        /// Ensures all fields marked with SerializedValueRequiredAttribute have non-null values in the given type.
        /// Logs an error if any fields are null. Returns whether all fields validated successfully.
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="instance"></param>
        /// <param name="inheritFields"></param>
        public static bool Validate(System.Type targetType, object instance, bool inheritFields)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            if (inheritFields) bindingFlags |= BindingFlags.FlattenHierarchy;
            FieldInfo[] fields = targetType.GetFields(bindingFlags)
                .Where(f => f.IsDefined(typeof(SerializedValueRequiredAttribute)) && f.GetValue(instance) == null).ToArray();
            if (fields.Length == 0) return true;
            var errorMessage = new StringBuilder($"Missing serialized values on type '{targetType.Name}': ",
                fields.Length * 30);
            foreach (FieldInfo field in fields)
            {
                errorMessage.Append($"'{field.Name}',");
            }
            errorMessage.Remove(errorMessage.Length - 1, 1);
            Debug.LogError(errorMessage);
            return false;
        }

        /// <summary>
        /// Ensures all fields marked with SerializedValueRequiredAttribute have non-null values in the given type.
        /// Logs an error if any fields are null. Returns whether all fields validated successfully.
        /// </summary>
        /// <param name="inheritFields">Whether inherited fields are considered.</param>
        public static bool Validate<T>(this T instance, bool inheritFields = true) where T : class
        {
            return Validate(instance.GetType(), instance, inheritFields);
        }
    }
}
