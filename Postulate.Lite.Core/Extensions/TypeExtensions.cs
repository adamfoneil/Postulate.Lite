using Postulate.Lite.Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Postulate.Lite.Core.Extensions
{
	public static class TypeExtensions
	{
		public const string IdentityProperty = "Id";

		public static PropertyInfo GetIdentityProperty(this Type type)
		{
			try
			{
				string propertyName = type.GetCustomAttribute<IdentityAttribute>().PropertyName;
				return type.GetProperty(propertyName);
			}
			catch
			{
				try
				{
					return type.GetProperty(IdentityProperty);
				}
				catch (Exception exc)
				{
					throw new Exception($"Couldn't find identity property on model type {type.Name}: {exc.Message}");
				}
			}
		}

		public static string GetIdentityName(this Type type)
		{
			var property = GetIdentityProperty(type);
			return GetColumnName(property);
		}

		public static string TryGetIdentityName(this Type type, string defaultValue, ref bool hasIdentity)
		{
			try
			{
				string result = GetIdentityName(type);
				hasIdentity = true;
				return result;
			}
			catch
			{
				hasIdentity = false;
				return defaultValue;
			}
		}

		public static string GetColumnName(this PropertyInfo propertyInfo)
		{
			ColumnAttribute attr = propertyInfo.GetCustomAttribute<ColumnAttribute>();
			return (attr != null && !string.IsNullOrEmpty(attr.Name)) ? attr.Name : propertyInfo.Name;
		}

		public static bool IsNullable(this Type type)
		{
			return IsNullableGeneric(type) || type.Equals(typeof(string)) || type.Equals(typeof(byte[]));
		}

		public static bool IsNullableGeneric(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
	}
}