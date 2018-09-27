using System;
using System.Linq;
using System.Reflection;

namespace Postulate.Lite.Core.Extensions
{
	public static class PropertyInfoExtensions
	{
		/// <summary>
		/// Returns true if the property has the specified attribute
		/// </summary>
		/// <typeparam name="T">Attribute class type</typeparam>
		public static bool HasAttribute<T>(this ICustomAttributeProvider provider, Func<T, bool> criteria = null) where T : Attribute
		{			
			return HasAttribute<T>(provider, out T result, criteria);
		}

		public static bool HasAttribute<T>(this ICustomAttributeProvider provider, out T result, Func<T, bool> criteria = null) where T : Attribute
		{
			result = GetAttribute<T>(provider);

			if (result != null)
			{
				if (criteria?.Invoke(result) ?? true) return true;
			}			

			return false;
		}

		public static T GetAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute
		{		
			var attrs = provider.GetCustomAttributes(typeof(T), true).OfType<T>();
			return (attrs?.Any() ?? false) ? attrs.FirstOrDefault() : null;
		}

		/// <summary>
		/// Gets the intended mapping type of a given type, for example removing any nullable generic or getting the underlying type of an enum
		/// </summary>
		public static Type GetMappedType(this PropertyInfo propertyInfo)
		{
			Type result = propertyInfo.PropertyType;
			if (result.IsGenericType) result = result.GenericTypeArguments[0];
			if (result.IsEnum) result = result.GetEnumUnderlyingType();
			return result;
		}
	}
}