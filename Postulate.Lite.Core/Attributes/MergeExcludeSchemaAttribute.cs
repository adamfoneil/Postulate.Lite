using System;

namespace Postulate.Lite.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class MergeExcludeSchemaAttribute : Attribute
	{
		public MergeExcludeSchemaAttribute(string schema)
		{
			Schema = schema;
		}

		public string Schema { get; private set; }
	}
}