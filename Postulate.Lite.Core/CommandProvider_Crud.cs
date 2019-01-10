using Dapper;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Exceptions;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Postulate.Lite.Core
{
	/// <summary>
	/// Generates SQL commands for Crud and Merge methods. As an abstract class, it requires database-specific implementations
	/// </summary>
	/// <typeparam name="TKey">Identity column type</typeparam>
	public abstract partial class CommandProvider<TKey>
	{
		private readonly Func<object, TKey> _identityConverter;
		private readonly string _identitySyntax;

		protected readonly SqlIntegrator _integrator;

		public CommandProvider(Func<object, TKey> identityConverter, SqlIntegrator integrator, string identitySyntax)
		{
			_identityConverter = identityConverter;
			_integrator = integrator;
			_identitySyntax = identitySyntax;
		}

		/// <summary>
		/// Generates a SQL insert statement for a given model class that returns a generated identity value
		/// </summary>
		/// <typeparam name="T">Model class type</typeparam>
		protected abstract string InsertCommand<T>();

		/// <summary>
		/// Generates a SQL insert statement for a given model class without retrieving identity value
		/// </summary>
		/// <typeparam name="T">Model class type</typeparam>	
		protected abstract string PlainInsertCommand<T>(string tableName = null);

		/// <summary>
		/// Generates a SQL update statement for a given model class
		/// </summary>
		/// <typeparam name="T">Model class type</typeparam>
		protected abstract string UpdateCommand<T>();

		/// <summary>
		/// Generates a SQL delete statement for a given model class
		/// </summary>
		/// <typeparam name="T">Model class type</typeparam>
		protected abstract string DeleteCommand<T>();

		/// <summary>
		/// Generates a SQL select statement for a given model class
		/// </summary>
		/// <typeparam name="T">Model class type</typeparam>
		/// <param name="whereClause">WHERE clause to append</param>
		protected abstract string FindCommand<T>(string whereClause);

		/// <summary>
		/// Returns a type-specific identity value from an object
		/// </summary>
		/// <param name="value">Primary key value</param>
		protected TKey ConvertIdentity(object value)
		{
			return _identityConverter.Invoke(value);
		}

		/// <summary>
		/// Encloses a database object identifier in the characters appropriate for a particular database. For example, square braces for SQL Server or backticks for My SQL
		/// </summary>
		protected abstract string ApplyDelimiter(string name);

		/// <summary>
		/// Generates the syntax for column definition with a CREATE TABLE statement
		/// </summary>
		protected abstract string SqlColumnSyntax(PropertyInfo propertyInfo, bool isIdentity);

		/// <summary>
		/// Returns the portion of the column definition syntax within a CREATE TABLE statement that causes the identity column to be autoincrementing.
		/// For example SQL Server can use identity(1,1) and My SQL uses auto_increment
		/// </summary>
		protected string IdentityColumnSyntax()
		{
			return _identitySyntax;
		}

		/// <summary>
		/// Returns true if the given model object has not been saved in the database yet
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		public bool IsNew<TModel>(TModel @object)
		{
			return GetIdentity(@object).Equals(default(TKey));
		}

		/// <summary>
		/// After a database insert, applies the generated identity value to the model object
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		protected void SetIdentity<TModel>(TModel @object, TKey value)
		{
			if (IsNew(@object))
			{
				var identityProp = typeof(TModel).GetIdentityProperty();
				identityProp.SetValue(@object, value);
			}
			else
			{
				throw new InvalidOperationException("Can't set a record's identity more than once.");
			}
		}

		/// <summary>
		/// Returns the identity value of a given model object
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		public TKey GetIdentity<TModel>(TModel @object)
		{
			var property = typeof(TModel).GetIdentityProperty();
			return ConvertIdentity(property.GetValue(@object));
		}

		/// <summary>
		/// Performs a SQL insert and returns the generated identity value
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="object">Model object. If based on <see cref="Record"/>, then permission checks and row-level events are triggered</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public TKey Insert<TModel>(IDbConnection connection, TModel @object, IUser user = null)
		{
			var record = PreSave(connection, @object, user);

			TKey result = default(TKey);

			string cmd = InsertCommand<TModel>();
			try
			{
				result = connection.QuerySingleOrDefault<TKey>(cmd, @object);
			}
			catch (Exception exc)
			{
				throw new CrudException(SaveAction.Insert, exc, cmd, @object);
			}

			SetIdentity(@object, result);

			record?.AfterSave(connection, SaveAction.Insert);

			return result;
		}

		/// <summary>
		/// Performs a SQL insert without returning an identity value
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="object">Model object. If based on <see cref="Record"/>, then permission checks and row-level events are triggered</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public void PlainInsert<TModel>(IDbConnection connection, TModel @object, string tableName = null, IUser user = null)
		{
			var record = PreSave(connection, @object, user);

			string cmd = PlainInsertCommand<TModel>(tableName);
			Trace.WriteLine($"PlainInsert: {cmd}");

			try
			{
				connection.Execute(cmd, @object);
			}
			catch (Exception exc)
			{
				throw new CrudException(SaveAction.Insert, exc, cmd, @object);
			}

			record?.AfterSave(connection, SaveAction.Insert);
		}

		/// <summary>
		/// Performs a SQL insert and returns the generated identity value
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="object">Model object. If based on <see cref="Record"/>, then permission checks and row-level events are triggered</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public async Task<TKey> InsertAsync<TModel>(IDbConnection connection, TModel @object, IUser user = null)
		{
			var record = PreSave(connection, @object, user);

			string cmd = InsertCommand<TModel>();
			Trace.WriteLine($"InsertAsync: {cmd}");

			TKey result = default(TKey);
			try
			{
				result = await connection.QuerySingleOrDefaultAsync<TKey>(cmd, @object);
			}
			catch (Exception exc)
			{
				throw new CrudException(SaveAction.Insert, exc, cmd, @object);
			}

			SetIdentity(@object, result);
			record?.AfterSave(connection, SaveAction.Insert);
			return result;
		}

		/// <summary>
		/// Performs a SQL insert without returning an identity value
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="object">Model object. If based on <see cref="Record"/>, then permission checks and row-level events are triggered</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public async Task PlainInsertAsync<TModel>(IDbConnection connection, TModel @object, string tableName = null, IUser user = null)
		{
			var record = PreSave(connection, @object, user);

			string cmd = PlainInsertCommand<TModel>(tableName);
			Trace.WriteLine($"PlainInsertAsync: {cmd}");

			try
			{				
				await connection.ExecuteAsync(cmd, @object);
			}
			catch (Exception exc)
			{
				throw new CrudException(SaveAction.Insert, exc, cmd, @object);
			}

			record?.AfterSave(connection, SaveAction.Insert);
		}

		/// <summary>
		/// Executes validation, permission checks, and BeforeSave override on <see cref="Record"/> objects
		/// </summary>
		private Record PreSave<TModel>(IDbConnection connection, TModel @object, IUser user)
		{
			var record = @object as Record;

			string message = null;
			if (!record?.Validate(connection, out message) ?? false) throw new ValidationException(message);

			if (user != null)
			{
				if (!record?.CheckSavePermission(connection, user) ?? false) throw new PermissionException($"User {user.UserName} does not have save permission on {typeof(TModel).Name}.");				
				record?.BeforeSave(connection, SaveAction.Insert, user);
			}
			return record;
		}

		/// <summary>
		/// Performs a SQL update on select properties of an object
		/// </summary>
		public void Update<TModel>(IDbConnection connection, TModel @object, IUser user, params Expression<Func<TModel, object>>[] setColumns)
		{
			var changes = GetChanges(connection, @object);
			CommandDefinition cmd = GetSetColumnsUpdateCommand(@object, setColumns);
			Trace.WriteLine($"Update: {cmd}");
			connection.Execute(cmd);
			SaveChanges(connection, @object, changes, user);
		}

		/// <summary>
		/// Performs a SQL update on select properties of an object
		/// </summary>
		public async Task UpdateAsync<TModel>(IDbConnection connection, TModel @object, IUser user, params Expression<Func<TModel, object>>[] setColumns)
		{
			var changes = await GetChangesAsync(connection, @object);
			CommandDefinition cmd = GetSetColumnsUpdateCommand(@object, setColumns);
			Trace.WriteLine($"UpdateAsync: {cmd}");
			await connection.ExecuteAsync(cmd);
			await SaveChangesAsync(connection, @object, changes, user);
		}

		private CommandDefinition GetSetColumnsUpdateCommand<TModel>(TModel @object, Expression<Func<TModel, object>>[] setColumns)
		{
			var type = typeof(TModel);
			DynamicParameters dp = new DynamicParameters();

			string setColumnExpr = string.Join(", ", setColumns.Select(e =>
			{
				string propName = PropertyNameFromLambda(e);
				PropertyInfo pi = type.GetProperty(propName);
				dp.Add(propName, e.Compile().Invoke(@object));
				return $"{ApplyDelimiter(pi.GetColumnName())}=@{propName}";
			}));

			string cmdText = $"UPDATE {ApplyDelimiter(_integrator.GetTableName(type))} SET {setColumnExpr} WHERE {ApplyDelimiter(type.GetIdentityName())}=@id";
			dp.Add("id", GetIdentity(@object));

			return new CommandDefinition(cmdText, dp);
		}

		private string PropertyNameFromLambda(Expression expression)
		{
			// thanks to http://odetocode.com/blogs/scott/archive/2012/11/26/why-all-the-lambdas.aspx
			// thanks to http://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression

			LambdaExpression le = expression as LambdaExpression;
			if (le == null) throw new ArgumentException("expression");

			MemberExpression me = null;
			if (le.Body.NodeType == ExpressionType.Convert)
			{
				me = ((UnaryExpression)le.Body).Operand as MemberExpression;
			}
			else if (le.Body.NodeType == ExpressionType.MemberAccess)
			{
				me = le.Body as MemberExpression;
			}

			if (me == null) throw new ArgumentException("expression");

			return me.Member.Name;
		}

		/// <summary>
		/// Performs a SQL update
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="object">Model object. If based on <see cref="Record"/>, then permission checks and row-level events are triggered</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public void Update<TModel>(IDbConnection connection, TModel @object, IUser user = null)
		{
			var record = PreSave(connection, @object, user);

			string cmd = UpdateCommand<TModel>();
			Trace.WriteLine($"Update: {cmd}");

			try
			{
				var changes = GetChanges(connection, @object);
				connection.Execute(cmd, @object);				
				SaveChanges(connection, @object, changes, user);
			}
			catch (Exception exc)
			{
				throw new CrudException(SaveAction.Update, exc, cmd, @object);
			}

			record?.AfterSave(connection, SaveAction.Update);
		}

		/// <summary>
		/// Performs a SQL update
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="object">Model object. If based on <see cref="Record"/>, then permission checks and row-level events are triggered</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public async Task UpdateAsync<TModel>(IDbConnection connection, TModel @object, IUser user = null)
		{
			var record = PreSave(connection, @object, user);

			string cmd = UpdateCommand<TModel>();
			Trace.WriteLine($"UpdateAsync: {cmd}");

			try
			{
				var changes = await GetChangesAsync(connection, @object);
				await connection.ExecuteAsync(cmd, @object);
				await SaveChangesAsync(connection, @object, changes, user);
			}
			catch (Exception exc)
			{
				throw new CrudException(SaveAction.Update, exc, cmd, @object);
			}

			record?.AfterSave(connection, SaveAction.Update);
		}

		/// <summary>
		/// Searches for an existing record by its primary key, and updates the existing record or inserts the new one
		/// </summary>
		public TKey Merge<TModel>(IDbConnection connection, TModel @object, out SaveAction action, IUser user = null)
		{
			if (IsNew(@object))
			{
				var existing = FindByPrimaryKey(connection, @object, user);
				if (existing != null) SetIdentity(@object, GetIdentity(existing));
				action = (existing != null) ? SaveAction.Update : SaveAction.Insert;
			}
			else
			{
				action = SaveAction.Update;
			}

			return Save(connection, @object, user);
		}

		/// <summary>
		/// Searches for an existing record by its primary key, and updates the existing record or inserts the new one
		/// </summary>
		public TKey Merge<TModel>(IDbConnection connection, TModel @object, IUser user = null)
		{
			return Merge(connection, @object, out SaveAction action, user);
		}

		/// <summary>
		/// Searches for an existing record by its primary key, and updates the existing record or inserts the new one
		/// </summary>
		public async Task<TKey> MergeAsync<TModel>(IDbConnection connection, TModel @object, IUser user = null)
		{
			if (IsNew(@object))
			{
				var existing = await FindByPrimaryKeyAsync(connection, @object, user);
				if (existing != null) SetIdentity(@object, GetIdentity(existing));
			}

			return await SaveAsync(connection, @object, user);
		}

		/// <summary>
		/// Performs a SQL insert or update for a given model object. If an insert is done, the generated identity value is returned
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="object">Model object. If based on <see cref="Record"/>, then permission checks and row-level events are triggered</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public TKey Save<TModel>(IDbConnection connection, TModel @object, IUser user = null)
		{
			if (IsNew(@object))
			{
				return Insert(connection, @object, user);
			}
			else
			{
				Update(connection, @object, user);
				return GetIdentity(@object);
			}
		}

		/// <summary>
		/// Performs a SQL insert or update for a given model object. If an insert is done, the generated identity value is returned
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="object">Model object. If based on <see cref="Record"/>, then permission checks and row-level events are triggered</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public async Task<TKey> SaveAsync<TModel>(IDbConnection connection, TModel @object, IUser user = null)
		{
			if (IsNew(@object))
			{
				return await InsertAsync(connection, @object, user);
			}
			else
			{
				await UpdateAsync(connection, @object, user);
				return GetIdentity(@object);
			}
		}

		public bool Exists<TModel>(IDbConnection connection, TKey identity, IUser user = null)
		{
			var record = Find<TModel>(connection, identity, user);
			return (record != null);
		}

		public async Task<bool> ExistsAsync<TModel>(IDbConnection connection, TKey identity, IUser user = null)
		{
			var record = await FindAsync<TModel>(connection, identity, user);
			return (record != null);
		}

		public bool ExistsWhere<TModel>(IDbConnection connection, object criteria, IUser user = null)
		{
			var record = FindWhere<TModel>(connection, criteria, user);
			return (record != null);
		}

		public async Task<bool> ExistsWhereAsync<TModel>(IDbConnection connection, object criteria, IUser user = null)
		{
			var record = await FindWhereAsync<TModel>(connection, criteria, user);
			return (record != null);
		}

		/// <summary>
		/// Gets a model object for a given identity value
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="identity">Primary key value</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public TModel Find<TModel>(IDbConnection connection, TKey identity, IUser user = null)
		{
			string identityCol = typeof(TModel).GetIdentityName();
			string cmd = FindCommand<TModel>($"{ApplyDelimiter(identityCol)}=@id");
			Trace.WriteLine($"Find: {cmd}");
			TModel result = connection.QuerySingleOrDefault<TModel>(cmd, new { id = identity });			
			return FindInner(connection, result, user);
		}

		/// <summary>
		/// Gets a model object for a given identity value
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="identity">Primary key value</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public async Task<TModel> FindAsync<TModel>(IDbConnection connection, TKey identity, IUser user = null)
		{
			string identityCol = typeof(TModel).GetIdentityName();
			string cmd = FindCommand<TModel>($"{ApplyDelimiter(identityCol)}=@id");
			Trace.WriteLine($"FindAsync: {cmd}");
			TModel result = await connection.QuerySingleOrDefaultAsync<TModel>(cmd, new { id = identity });			
			return FindInner(connection, result, user);
		}

		/// <summary>
		/// Gets a model object based on arbitrary criteria.
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="criteria">Object specifying the criteria to search for</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public TModel FindWhere<TModel>(IDbConnection connection, object criteria, IUser user = null)
		{
			string whereClause = WhereClauseFromObject(criteria);
			return FindWhereInternal<TModel>(connection, whereClause, criteria, user);
		}

		/// <summary>
		/// Gets a model object based on arbitrary criteria.
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="criteria">Object specifying the criteria to search for</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public async Task<TModel> FindWhereAsync<TModel>(IDbConnection connection, object criteria, IUser user = null)
		{
			string whereClause = WhereClauseFromObject(criteria);
			string cmd = FindCommand<TModel>(whereClause);
			Trace.WriteLine($"FindWhereAsync: {cmd}");
			TModel result = await connection.QuerySingleOrDefaultAsync<TModel>(cmd, criteria);			
			return FindInner(connection, result, user);
		}

		private TModel FindByPrimaryKey<TModel>(IDbConnection connection, TModel criteria, IUser user = null)
		{
			string whereClause = PrimaryKeyWhereClauseFromObject(criteria);
			return FindWhereInternal<TModel>(connection, whereClause, criteria, user);
		}

		private async Task<TModel> FindByPrimaryKeyAsync<TModel>(IDbConnection connection, TModel criteria, IUser user = null)
		{
			string whereClause = PrimaryKeyWhereClauseFromObject(criteria);
			return await FindWhereInternalAsync(connection, whereClause, criteria, user);
		}

		private TModel FindWhereInternal<TModel>(IDbConnection connection, string whereClause, object criteria, IUser user = null)
		{
			string cmd = FindCommand<TModel>(whereClause);
			Trace.WriteLine($"FindWhereInternal: {cmd}");
			TModel result = connection.QuerySingleOrDefault<TModel>(cmd, criteria);			
			return FindInner(connection, result, user);
		}

		private async Task<TModel> FindWhereInternalAsync<TModel>(IDbConnection connection, string whereClause, TModel criteria, IUser user = null)
		{
			string cmd = FindCommand<TModel>(whereClause);
			TModel result = await connection.QuerySingleOrDefaultAsync<TModel>(cmd, criteria);			
			return FindInner(connection, result, user);
		}

		private string WhereClauseFromObject(object criteria)
		{
			var props = criteria.GetType().GetProperties().Where(pi => HasValue(pi, criteria));
			return WhereClauseFromProperties(props);
		}

		private string PrimaryKeyWhereClauseFromObject<TModel>(TModel criteria)
		{
			var props = typeof(TModel).GetProperties().Where(pi => pi.HasAttribute<PrimaryKeyAttribute>());
			if (!props.Any()) throw new Exception($"No primary key properties found on {typeof(TModel).Name}");
			return WhereClauseFromProperties(props);
		}

		private string WhereClauseFromProperties(IEnumerable<PropertyInfo> properties)
		{
			return string.Join(" AND ", properties.Select(pi => $"{ApplyDelimiter(pi.Name)}=@{pi.Name}"));
		}

		private bool HasValue(PropertyInfo pi, object @object)
		{
			var value = pi.GetValue(@object);
			if (value != null)
			{
				if (pi.PropertyType.Equals(typeof(string))) return !string.IsNullOrEmpty(value.ToString());
				var defaultValue = Activator.CreateInstance(pi.PropertyType);
				if (value.Equals(defaultValue)) return false;
				if (value.Equals(string.Empty)) return false;
				return true;
			}

			return false;
		}

		private TModel FindInner<TModel>(IDbConnection connection, TModel result, IUser user)
		{
			var record = result as Record;

			var lookup = result as IFindRelated<TKey>;
			if (lookup != null) lookup.FindRelated(connection, this);

			if (user != null)
			{
				if (!record?.CheckFindPermission(connection, user) ?? false)
				{
					throw new PermissionException($"User {user.UserName} does not have find permission on a record of {typeof(TModel).Name}.");
				}
			}

			return result;
		}

		/// <summary>
		/// Deletes a model object for a given identity value
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="identity">Primary key value</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public void Delete<TModel>(IDbConnection connection, TKey identity, IUser user = null)
		{
			var deleteMe = Find<TModel>(connection, identity, user);
			var record = deleteMe as Record;
			CheckDeletePermissionInternal<TModel>(connection, user, record);

			string cmd = DeleteCommand<TModel>();
			Trace.WriteLine($"Delete: {cmd}");

			connection.Execute(cmd, new { id = identity });

			record?.AfterDelete(connection);
		}

		private void CheckDeletePermissionInternal<TModel>(IDbConnection connection, IUser user, Record record)
		{
			if (user != null)
			{
				if (!record?.CheckDeletePermission(connection, user) ?? false) throw new PermissionException($"User {user.UserName} does not have delete permission on {typeof(TModel).Name}.");
			}
		}

		public async Task DeleteAsync<TModel>(IDbConnection connection, TKey identity, IUser user = null)
		{
			var deleteMe = Find<TModel>(connection, identity, user);
			var record = deleteMe as Record;
			CheckDeletePermissionInternal<TModel>(connection, user, record);

			string cmd = DeleteCommand<TModel>();
			await connection.ExecuteAsync(cmd, new { id = identity });

			record?.AfterDelete(connection);
		}

		/// <summary>
		/// Creates a database table for a given model class
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		public void CreateTable<TModel>(IDbConnection connection, string tableName = null)
		{
			string cmd = CreateTableCommand(typeof(TModel), tableName);
			Trace.WriteLine($"CreateTable: {cmd}");
			connection.Execute(cmd);
		}

		protected void GetInsertComponents<T>(out string columnList, out string valueList)
		{
			var columns = _integrator.GetEditableColumns(typeof(T), SaveAction.Insert);
			columnList = string.Join(", ", columns.Select(c => ApplyDelimiter(c.ColumnName)));
			valueList = string.Join(", ", columns.Select(c => $"@{c.PropertyName}"));
		}
	}
}