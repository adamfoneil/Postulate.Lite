using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Postulate.Lite.Core.Merge
{
	public class RebuildTable : Action
	{
		public IEnumerable<ColumnInfo> AddedColumns { get; private set; }
		public IEnumerable<ColumnInfo> ModifiedColumns { get; private set; }
		public IEnumerable<ColumnInfo> DeletedColumns { get; private set; }

		public RebuildTable(Type modelType, 
			IEnumerable<ColumnInfo> added = null, IEnumerable<ColumnInfo> modified = null, IEnumerable<ColumnInfo> deleted = null) : 
			base(ObjectType.Table, ActionType.Create)
		{
			ModelType = modelType;
			AddedColumns = added;
			ModifiedColumns = modified;
			DeletedColumns = deleted;
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