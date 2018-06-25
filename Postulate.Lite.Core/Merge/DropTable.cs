using Postulate.Lite.Core.Metadata;
using System.Collections.Generic;
using System.Data;

namespace Postulate.Lite.Core.Merge
{
	public class DropTable : Action
	{
		public DropTable(TableInfo tableInfo) : base(ObjectType.Table, ActionType.Drop)
		{
			TableInfo = tableInfo;
		}

		public TableInfo TableInfo { get; private set; }

		public IEnumerable<ForeignKeyInfo> GetDependentForeignKeys(IDbConnection connection)
		{
			throw new RowNotInTableException();
		}

		public override IEnumerable<string> SqlCommands<TKey>(CommandProvider<TKey> commandProvider, IDbConnection connection)
		{
			foreach (var fk in GetDependentForeignKeys(connection)) yield return commandProvider.DropForeignKeyCommand(fk);

			yield return commandProvider.DropTableCommand(TableInfo);
		}
	}
}