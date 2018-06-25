using System;

namespace Postulate.Lite.Core.Metadata
{
	public class TableInfo
	{
		public string Schema { get; set; }
		public string Name { get; set; }

		public static TableInfo FromModelType(Type modelType)
		{
			throw new NotImplementedException();
		}
	}
}