using Postulate.Lite.Core;
using System;
using System.Collections.Generic;

namespace Postulate.Lite.MySql
{
	public class MySqlIntegrator : SqlIntegrator
	{
		public override string DefaultSchema => string.Empty;

		public override Dictionary<Type, SqlTypeInfo> SupportedTypes(int length = 0, int precision = 0, int scale = 0)
		{
			return new Dictionary<Type, SqlTypeInfo>()
			{
				{ typeof(string), new SqlTypeInfo((length == 0) ? "text" : "varchar", (length == 0) ? "text" : $"varchar({length})") },
				{ typeof(int), new SqlTypeInfo("int") },
				{ typeof(DateTime), new SqlTypeInfo("datetime") },
				{ typeof(bool), new SqlTypeInfo("bit") },
				{ typeof(decimal), new SqlTypeInfo("decimal", $"decimal({precision}, {scale})") },
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