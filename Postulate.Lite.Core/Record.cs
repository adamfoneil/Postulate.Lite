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
		public virtual bool Validate(IDbConnection connection, out string message)
		{
			message = null;
			return true;
		}

		/// <summary>
		/// Override this to verify the current user has permission to perform requested action
		/// Throws <see cref="Exceptions.PermissionException"/> when permission denied
		/// </summary>
		public virtual bool CheckSavePermission(IDbConnection connection, IUser user)
		{
			return true;
		}

		/// <summary>
		/// Override this to apply any changes to a record immediately before it's saved, such as audit tracking fields
		/// </summary>
		public virtual void BeforeSave(IDbConnection connection, SaveAction action, IUser user)
		{
			// do nothing by default
		}

		/// <summary>
		/// Override this to execute logic after a record is successfully saved
		/// </summary>
		public virtual void AfterSave(IDbConnection connection, SaveAction action)
		{
			// do nothing by default
		}

		/// <summary>
		/// Override this to verify that the user has permission to view the record that was just found
		/// Throws <see cref="Exceptions.PermissionException"/> when permission denied
		/// </summary>
		public virtual bool CheckFindPermission(IDbConnection connection, IUser user)
		{
			return true;
		}

		/// <summary>
		/// Override this to lookup related records
		/// see https://github.com/adamosoftware/Postulate.Lite/wiki/Using-IFindRelated-to-implement-navigation-properties
		/// </summary>		
		public virtual void FindReferenced(IDbConnection connection)
		{
			// do nothing by default
		}

		/// <summary>
		/// Override this to verify the user has permission to delete this record
		/// Throws <see cref="Exceptions.PermissionException"/> when permission denied
		/// </summary>
		public virtual bool CheckDeletePermission(IDbConnection connection, IUser user)
		{
			return true;
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