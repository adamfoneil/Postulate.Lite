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

	/// <summary>
	/// Optionally use this as the basis for your model classes to add permission checks, validation, events, and foreign key lookups to your data access layer	
	/// </summary>
	public abstract class Record
	{
		public virtual void LookupIntForeignKeys(IDbConnection connection, CommandProvider<int> commandProvider)
		{
			// do nothing
		}

		public virtual void LookupLongForeignKeys(IDbConnection connection, CommandProvider<long> commandProvider)
		{
			// do nothing
		}

		public virtual void LookupGuidForeignKeys(IDbConnection connection, CommandProvider<Guid> commandProvider)
		{
			// do nothing
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