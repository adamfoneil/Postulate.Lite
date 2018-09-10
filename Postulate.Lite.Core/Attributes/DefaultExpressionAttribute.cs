using System;

namespace Postulate.Lite.Core.Attributes
{
	/// <summary>
	/// Use this when adding required columns to a table that already has data.
	/// This specifies how to fill the new column so it won't be null
	/// Used by SchemaSync.Library
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DefaultExpressionAttribute : Attribute
	{
		public DefaultExpressionAttribute(string expression)
		{
			Expression = expression;
		}

		public string Expression { get; }
	}
}