using System;

namespace Postulate.Lite.Core.Attributes
{
	public enum IdentityPosition
	{
		NotSet,
		FirstColumn,
		LastColumn
	}

	/// <summary>
	/// Indicates which property has the identity value for a record
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class IdentityAttribute : Attribute
	{
		public IdentityAttribute(string propertyName, IdentityPosition position = IdentityPosition.NotSet)
		{
			PropertyName = propertyName;
			Position = position;
		}

		public string PropertyName { get; }
		public IdentityPosition? Position { get; }
	}
}