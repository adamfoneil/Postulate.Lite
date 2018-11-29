using Postulate.Lite.Core.Interfaces;
using System;
using System.Collections.Generic;
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
		/// <summary>
		/// Override this to verify a record may be saved.
		/// Throws <see cref="Exceptions.ValidationException"/> on failed validation
		/// </summary>
		public virtual void Validate(IDbConnection connection)
		{
			// do nothing by default, but throw ValidationException on error
		}

		/// <summary>
		/// Override this to verify the current user has permission to perform requested action
		/// Throws <see cref="Exceptions.PermissionException"/> when permissino denied
		/// </summary>
		public virtual void CheckSavePermission(IDbConnection connection, IUser user)
		{
			// do nothing by default, but throw PermissionException on error
		}

		/// <summary>
		/// Override this to apply any changes to a record immediately before it's saved
		/// </summary>
		public virtual void BeforeSave(IDbConnection connection, SaveAction action, IUser user)
		{
			// do nothing by default
		}

		public virtual void AfterSave(IDbConnection connection, SaveAction action)
		{
			// do nothing by default
		}

		/// <summary>
		/// Override this to verify that the user has permission to view the record that was just found
		/// Throws <see cref="Exceptions.PermissionException"/> when permission denied
		/// </summary>
		public virtual void CheckFindPermission(IDbConnection connection, IUser user)
		{
			// do nothing by default
		}

		public virtual void FindReferenced(IDbConnection connection)
		{
			// do nothing by default
		}

		/// <summary>
		/// Override this to verify the user has permission to delete this record
		/// Throws <see cref="Exceptions.PermissionException"/> when permission denied
		/// </summary>
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