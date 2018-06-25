using Postulate.Lite.Core.Metadata;
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
			TableInfo = TableInfo.FromModelType(modelType);
		}

		public Type ModelType { get; private set; }
		public TableInfo TableInfo { get; private set; }

		public override IEnumerable<string> SqlCommands<TKey>(CommandProvider<TKey> commandProvider, IDbConnection connection)
		{
			DropTable drop = new DropTable(TableInfo);
			var rebuildFKs = drop.GetDependentForeignKeys(connection);
			foreach (var cmd in drop.SqlCommands(commandProvider, connection)) yield return cmd;

			CreateTable create = new CreateTable(ModelType);
			foreach (var cmd in create.SqlCommands(commandProvider, connection)) yield return cmd;

			foreach (var fk in rebuildFKs) yield return commandProvider.AddForeignKeyCommand(fk);
		}
	}
}