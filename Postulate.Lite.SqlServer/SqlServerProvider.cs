using Postulate.Lite.Core;
using Postulate.Lite.Core.Interfaces;
using System;

namespace Postulate.Lite.SqlServer
{
	public class SqlServerProvider : Provider<int>
	{
		public SqlServerProvider(IUser user) : base(user)
		{
		}

		protected override int ConvertIdentity(object value)
		{
			return Convert.ToInt32(value);
		}

		protected override string DeleteCommand<T>()
		{
			throw new NotImplementedException();
		}

		protected override string FindCommand<T>()
		{
			throw new NotImplementedException();
		}

		protected override string InsertCommand<T>()
		{
			throw new NotImplementedException();
		}

		protected override string UpdateCommand<T>()
		{
			throw new NotImplementedException();
		}
	}
}