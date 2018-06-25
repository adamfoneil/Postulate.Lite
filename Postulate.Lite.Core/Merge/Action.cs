using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Postulate.Lite.Core.Merge
{
	public enum ActionType
	{
		Create,
		Alter,
		Rename,
		Drop,
		DropAndCreate
	}

	public enum ObjectType
	{
		Schema,
		Table,
		Column,
		Key,
		Index,
		ForeignKey,
		Metadata
	}

	public abstract class Action
	{
		public Action(ObjectType objectType, ActionType actionType)
		{
			ObjectType = objectType;
			ActionType = actionType;			
		}

		public ObjectType ObjectType { get; private set; }
		public ActionType ActionType { get; private set; }		

		public virtual IEnumerable<string> ValidationErrors(IDbConnection connection)
		{
			return Enumerable.Empty<string>();
		}

		public bool IsValid(IDbConnection connection)
		{
			return !ValidationErrors(connection).Any();
		}

		public abstract IEnumerable<string> SqlCommands<TKey>(CommandProvider<TKey> commandProvider, IDbConnection connection);

		public void Execute<TKey>(CommandProvider<TKey> commandProvider, IDbConnection connection)
		{
			foreach (var cmd in SqlCommands(commandProvider, connection)) connection.Execute(cmd);
		}
	}
}