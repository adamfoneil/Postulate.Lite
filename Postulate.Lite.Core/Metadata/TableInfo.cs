using System;

namespace Postulate.Lite.Core.Metadata
{
	public class TableInfo
	{
		public string Schema { get; set; }
		public string Name { get; set; }

		public static TableInfo FromModelType<TKey>(Type modelType, CommandProvider<TKey> commandProvider)
		{
			TableInfo result = new TableInfo() { Name = modelType.Name };
			if (commandProvider.SupportsSchemas)
			{

			}
			else
			{

			}
			return result;
		}
	}
}