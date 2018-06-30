using Postulate.Lite.Core.Metadata;
using System.Collections.Generic;
using System.Data;

namespace Postulate.Lite.Core.Merge
{
	public class DropTable : Action
	{
		public DropTable(TableInfo tableInfo, IEnumerable<ForeignKeyInfo> dependentForeignKeys = null) : base(ObjectType.Table, ActionType.Drop)
		{
			TableInfo = tableInfo;
			DependentForeignKeys = dependentForeignKeys;
		}

		public TableInfo TableInfo { get; private set; }

		public IEnumerable<ForeignKeyInfo> DependentForeignKeys { get; private set; }

		public override IEnumerable<string> SqlCommands<TKey>(CommandProvider<TKey> commandProvider, IDbConnection connection)
		{
			var fks = DependentForeignKeys ?? commandProvider.GetDependentForeignKeys(connection, TableInfo);

			foreach (var fk in fks) yield return commandProvider.DropForeignKeyCommand(fk);

			yield return commandProvider.DropTableCommand(TableInfo);
		}
	}
}