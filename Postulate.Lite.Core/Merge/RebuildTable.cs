using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Postulate.Lite.Core.Merge
{
	public class RebuildTable : Action
	{
		public RebuildTable(Type modelType) : base(ObjectType.Table, ActionType.Drop)
		{
			ModelType = modelType;
		}

		public Type ModelType { get; private set; }		

		public override IEnumerable<string> SqlCommands<TKey>(CommandProvider<TKey> commandProvider, IDbConnection connection)
		{
			TableInfo tableInfo = commandProvider.GetTableInfo(ModelType);
			var rebuildFKs = commandProvider.GetDependentForeignKeys(connection, tableInfo);

			DropTable drop = new DropTable(tableInfo, rebuildFKs);
			foreach (var cmd in drop.SqlCommands(commandProvider, connection)) yield return cmd;

			CreateTable create = new CreateTable(ModelType);
			foreach (var cmd in create.SqlCommands(commandProvider, connection)) yield return cmd;

			foreach (var fk in rebuildFKs) yield return commandProvider.AddForeignKeyCommand(fk);
		}
	}
}