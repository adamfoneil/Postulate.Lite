using System;

namespace Postulate.Lite.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class PrimaryKeyAttribute : Attribute
	{
	}
}