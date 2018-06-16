using System;

namespace Postulate.Lite.Core.Attributes
{
	/// <summary>
	/// Indicates which property has the identity value for a record
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class IdentityAttribute : Attribute
	{
		public IdentityAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; }
	}
}