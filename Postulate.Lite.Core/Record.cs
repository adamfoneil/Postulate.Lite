using Postulate.Lite.Core.Interfaces;
using System;
using System.Data;

namespace Postulate.Lite.Core
{
	[Flags]
	public enum SaveAction
	{
		Insert = 1,
		Update = 2
	}

	public abstract class Record
	{
		public virtual void LookupForeignKeys(IDbConnection connection)
		{
			// do nothing by default
		}

		public virtual void Validate(IDbConnection connection)
		{
			// do nothing by default
		}

		public virtual void CheckSavePermission(IDbConnection connection, IUser user)
		{
			// do nothing by default			
		}

		public virtual void BeforeSave(IDbConnection connection, SaveAction action, IUser user)
		{
			// do nothing by default
		}

		public virtual void AfterSave(IDbConnection connection, SaveAction action)
		{
			// do nothing by default
		}

		public virtual void CheckFindPermission(IDbConnection connection, IUser user)
		{
			// do nothing by default
		}

		public virtual void FindReferenced(IDbConnection connection)
		{
			// do nothing by default
		}

		public virtual void CheckDeletePermission(IDbConnection connection, IUser user)
		{
			// do nothing by default
		}

		public virtual void BeforeDelete(IDbConnection connection)
		{
			// do nothing by default
		}

		public virtual void AfterDelete(IDbConnection connection)
		{
			// do nothing by default
		}
	}
}