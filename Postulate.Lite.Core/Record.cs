using System.Data;

namespace Postulate.Core
{
	public abstract class Record
	{
		public virtual bool IsValid(IDbConnection connection)
		{
			return true;
		}

		protected virtual bool AllowSave(IDbConnection connection, string userName)
		{
			return true;
		}

		protected virtual void BeforeSave(IDbConnection connection)
		{
			// do nothing by default
		}

		protected virtual void AfterSave(IDbConnection connection)
		{
			// do nothing by default
		}

		protected virtual bool AllowFind(IDbConnection connection, string userName)
		{
			return true;
		}

		protected virtual void FindReferenced(IDbConnection connection)
		{
			// do nothing by default
		}

		protected virtual bool AllowDelete(IDbConnection connection, string userName)
		{
			return true;
		}

		protected virtual void BeforeDelete(IDbConnection connection)
		{
			// do nothing by default
		}

		protected virtual void AfterDelete(IDbConnection connection)
		{
			// do nothing by default
		}
	}
}