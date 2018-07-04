using System;

namespace Postulate.Lite.Core.Metadata
{
	public class TableInfo
	{
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

		public override int GetHashCode()
		{
			return Schema.GetHashCode() + Name.GetHashCode();
		}
	}
}