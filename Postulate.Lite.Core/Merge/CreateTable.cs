using System;
using System.Collections.Generic;
using System.Data;

namespace Postulate.Lite.Core.Merge
{
	public class CreateTable : Action
	{
		public CreateTable(Type modelType) : base(ObjectType.Table, ActionType.Create)
		{
			ModelType = modelType;
		}

		public Type ModelType { get; private set; }

		public override IEnumerable<string> SqlCommands<TKey>(CommandProvider<TKey> commandProvider, IDbConnection connection)
		{
			yield return commandProvider.CreateTableCommand(ModelType);
		}
	}
}