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
		public static bool HasAttribute<T>(this PropertyInfo propertyInfo, Func<T, bool> criteria = null) where T : Attribute
		{			
			return HasAttribute<T>(propertyInfo, out T result, criteria);
		}

		public static bool HasAttribute<T>(this PropertyInfo propertyInfo, out T result, Func<T, bool> criteria = null) where T : Attribute
		{
			result = null;

			var attr = propertyInfo.GetCustomAttribute<T>();
			if (attr != null)
			{
				if (criteria?.Invoke(attr) ?? true)
				{
					result = attr;
					return true;
				}
			}

			return false;
		}

		public static T GetAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute
		{		
			var attrs = provider.GetCustomAttributes(typeof(T), true).OfType<T>();
			return (attrs?.Any() ?? false) ? attrs.FirstOrDefault() : null;
		}
	}
}