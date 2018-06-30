using System;

namespace Postulate.Lite.Core.Attributes
{
	/// <summary>
	/// Used with classes based on <see cref="Abstract.Query{TResult}"/> to allow you to map specific criteria expressions with specific property values.
	/// Cannot be used with <see cref="WhereAttribute"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class CaseAttribute : Attribute
	{
		public CaseAttribute(object value, string expression)
		{
			Value = value;
			Expression = expression;
		}

		public object Value { get; }

		public string Expression { get; }
	}
}