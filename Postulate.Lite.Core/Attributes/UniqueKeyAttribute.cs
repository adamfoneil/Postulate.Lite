using System;

namespace Postulate.Lite.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
	public class UniqueKeyAttribute : Attribute
	{
		/// <summary>
		/// Denotes a unique constraint on a single property
		/// </summary>
		public UniqueKeyAttribute()
		{
		}

		/// <summary>
		/// At the class level, describes a unique constraint with a set of columns
		/// </summary>
		public UniqueKeyAttribute(params string[] columnNames)
		{
			ColumnNames = columnNames;
		}

		public string[] ColumnNames { get; }
	}
}