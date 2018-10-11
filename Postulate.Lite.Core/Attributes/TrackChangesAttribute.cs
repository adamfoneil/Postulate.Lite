using System;

namespace Postulate.Lite.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class TrackChangesAttribute : Attribute
	{
		public TrackChangesAttribute()
		{
		}

		public string IgnoreProperties { get; set; }
	}
}