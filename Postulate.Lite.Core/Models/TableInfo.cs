using System;

namespace Postulate.Lite.Core.Models
{
	public class TableInfo
	{
		public TableInfo()
		{
		}

		public TableInfo(string schema, string name)
		{
			Schema = schema;
			Name = name;
		}

		public string Schema { get; set; }
		public string Name { get; set; }
		public Type ModelType { get; set; }
		public long RowCount { get; set; }

		public bool IsEmpty()
		{
			return (RowCount == 0);
		}

		public override bool Equals(object obj)
		{
			TableInfo test = obj as TableInfo;
			if (test != null)
			{
				return test.Schema.ToLower().Equals(Schema.ToLower()) && test.Name.ToLower().Equals(Name.ToLower());
			}

			return false;
		}

		public override string ToString()
		{
			return (!string.IsNullOrEmpty(Schema)) ? $"{Schema}.{Name}" : Name;
		}

		public override int GetHashCode()
		{
			return Schema.GetHashCode() + Name.GetHashCode();
		}
	}
}