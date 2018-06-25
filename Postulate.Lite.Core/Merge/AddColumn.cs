using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Postulate.Lite.Core.Merge
{
	public class AddColumn : Action
	{
		public AddColumn(PropertyInfo propertyInfo) : base(ObjectType.Column, ActionType.Create)
		{
			PropertyInfo = propertyInfo;
		}

		public PropertyInfo PropertyInfo { get; private set; }

		public override IEnumerable<string> SqlCommands<TKey>(CommandProvider<TKey> commandProvider, IDbConnection connection)
		{
			yield return commandProvider.AddColumnCommand(PropertyInfo);
		}
	}
}