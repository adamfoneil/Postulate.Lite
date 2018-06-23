using System;

namespace Postulate.Lite.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class ColumnAccessAttribute : Attribute
	{
		public ColumnAccessAttribute(SaveAction action)
		{
			Action = action;
		}

		public SaveAction Action { get; private set; }
	}
}