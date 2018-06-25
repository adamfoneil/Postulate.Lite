namespace Postulate.Lite.Core.Metadata
{
	public class ForeignKeyInfo
	{
		public string ConstraintName { get; set; }
		public string TableSchema { get; set; }
		public string TableName { get; set; }
		public string ColumnName { get; set; }
	}
}