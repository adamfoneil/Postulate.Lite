using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Postulate.Lite.Core.Merge
{
	public class AlterColumn : Action
	{
		public AlterColumn(ColumnInfo columnInfo) : base(ObjectType.Column, ActionType.Alter)
		{
			ColumnInfo = columnInfo;
		}

		public ColumnInfo ColumnInfo { get; private set; }

		public override IEnumerable<string> SqlCommands<TKey>(CommandProvider<TKey> commandProvider, IDbConnection connection)
		{
			throw new NotImplementedException();
		}
	}
}