using System;

namespace Postulate.Lite.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DecimalPrecisionAttribute : Attribute
	{
		public DecimalPrecisionAttribute(byte precision, byte scale)
		{
			if (Precision < Scale) throw new ArgumentException("Precision must be equal or greater than scale.");

			Precision = precision;
			Scale = scale;
		}

		public byte Scale { get; }
		public byte Precision { get; }
	}
}