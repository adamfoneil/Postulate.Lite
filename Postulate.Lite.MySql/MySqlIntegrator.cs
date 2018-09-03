using Postulate.Lite.Core;
using System;
using System.Collections.Generic;

namespace Postulate.Lite.MySql
{
	public class MySqlIntegrator : SqlIntegrator
	{
		public override Dictionary<Type, string> SupportedTypes(int length = 0, int precision = 0, int scale = 0)
		{
			return new Dictionary<Type, string>()
			{
				{ typeof(string), $"varchar({length})" },
				{ typeof(int), "int" },
				{ typeof(DateTime), "datetime" },
				{ typeof(bool), "bit" },
				{ typeof(decimal), $"decimal({scale}, {precision})" },
				{ typeof(long), "bigint" },
				{ typeof(short), "smallint" },
				{ typeof(byte), "tinyint" },
				{ typeof(TimeSpan), "time" },
				{ typeof(double), "float" },
				{ typeof(float), "float" },
				{ typeof(char), "char(1)" },
				{ typeof(byte[]), $"varbinary({length})" }
			};
		}
	}
}