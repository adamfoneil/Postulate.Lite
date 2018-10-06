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
		public IdentityAttribute(string propertyName, IdentityPosition position = IdentityPosition.NotSet, string constraintName = null)
		{
			PropertyName = propertyName;
			Position = position;
			ConstraintName = constraintName;
		}

		public string PropertyName { get; }
		public IdentityPosition? Position { get; }
		public string ConstraintName { get; }
	}
}