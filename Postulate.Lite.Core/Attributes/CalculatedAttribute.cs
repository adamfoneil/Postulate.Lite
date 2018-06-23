using System;

namespace Postulate.Lite.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class CalculatedAttribute : Attribute
	{
		public CalculatedAttribute(string expression, bool isPersistent = false)
		{
			Expression = expression;
			IsPersistent = isPersistent;
		}

		public string Expression { get; private set; }

		public bool IsPersistent { get; private set; }
	}
}