using Postulate.Lite.Core;
using System;
using System.Collections.Generic;

namespace Postulate.Lite.MySql
{
	public class MySqlIntegrator : SqlIntegrator
	{
		public override Dictionary<Type, SqlTypeInfo> SupportedTypes(int length = 0, int precision = 0, int scale = 0)
		{
			return new Dictionary<Type, SqlTypeInfo>()
			{
				{ typeof(string), new SqlTypeInfo("varchar", $"varchar({length})") },
				{ typeof(int), new SqlTypeInfo("int") },
				{ typeof(DateTime), new SqlTypeInfo("datetime") },
				{ typeof(bool), new SqlTypeInfo("bit") },
				{ typeof(decimal), new SqlTypeInfo("decimal", $"decimal({scale}, {precision})") },
				{ typeof(long), new SqlTypeInfo("bigint") },
				{ typeof(short), new SqlTypeInfo("smallint") },
				{ typeof(byte), new SqlTypeInfo("tinyint") },
				{ typeof(TimeSpan), new SqlTypeInfo("time") },
				{ typeof(double), new SqlTypeInfo("float") },
				{ typeof(float), new SqlTypeInfo("float") },
				{ typeof(char), new SqlTypeInfo("char(1)") },
				{ typeof(byte[]), new SqlTypeInfo("varbinary", $"varbinary({length})") }
			};
		}
	}
}