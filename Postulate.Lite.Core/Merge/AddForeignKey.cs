using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Postulate.Lite.Core.Merge
{
	public class AddForeignKey : Action
	{
		public AddForeignKey(ForeignKeyInfo foreignKeyInfo) : base(ObjectType.ForeignKey, ActionType.Create)
		{
			ForeignKeyInfo = foreignKeyInfo;
		}

		public ForeignKeyInfo ForeignKeyInfo { get; private set; }

		public override IEnumerable<string> SqlCommands<TKey>(CommandProvider<TKey> commandProvider, IDbConnection connection)
		{
			throw new NotImplementedException();
		}
	}
}