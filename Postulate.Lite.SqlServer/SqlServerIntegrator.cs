using Postulate.Lite.Core;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Postulate.Lite.SqlServer
{
	public class SqlServerIntegrator : SqlIntegrator
	{
		public override string DefaultSchema => "dbo";

		public override Dictionary<Type, SqlTypeInfo> SupportedTypes(int length = 0, int precision = 0, int scale = 0)
		{
			return new Dictionary<Type, SqlTypeInfo>()
			{
				{ typeof(string), new SqlTypeInfo("nvarchar", $"nvarchar({((length == 0) ? "max" : length.ToString())})") },
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
				{ typeof(Guid), new SqlTypeInfo("uniqueidentifier") },
				{ typeof(char), new SqlTypeInfo("char(1)") },
				{ typeof(byte[]), new SqlTypeInfo("varbinary", $"varbinary({length})") }
			};
		}
	}
}