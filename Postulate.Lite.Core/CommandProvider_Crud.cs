using Dapper;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Interfaces;
using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Postulate.Lite.Core
{
	/// <summary>
	/// Generates SQL commands for Crud and Merge methods. As an abstract class, it requires database-specific implementations
	/// </summary>
	/// <typeparam name="TKey">Primary key type</typeparam>
	public abstract partial class CommandProvider<TKey>
	{
		private readonly Func<object, TKey> _identityConverter;
		private readonly string _identitySyntax;

		public CommandProvider(Func<object, TKey> identityConverter, string identitySyntax)
		{
			_identityConverter = identityConverter;
			_identitySyntax = identitySyntax;
		}

		/// <summary>
		/// Generates a SQL insert statement for a given model class
		/// </summary>
		/// <typeparam name="T">Model class type</typeparam>
		protected abstract string InsertCommand<T>();

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
		/// Gets the database table name for a given model class
		/// </summary>
		/// <typeparam name="T">Model class type</typeparam>
		protected abstract string TableName(Type modelType);

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
		/// Returns the properties of a model class that may be affected by an INSERT or UPDATE statement.
		/// For example calculated and identity columns are omitted.
		/// </summary>
		/// <typeparam name="T">Model class type</typeparam>
		/// <param name="action">Indicates whether an insert or update is in effect</param>
		protected IEnumerable<ColumnInfo> EditableColumns<T>(SaveAction action)
		{
			string identity = typeof(T).GetIdentityName().ToLower();
			var props = typeof(T).GetProperties().Where(pi => !pi.GetColumnName().ToLower().Equals(identity)).ToArray();
			return props.Where(pi => IsEditable(pi, action)).Select(pi => new ColumnInfo(pi)).ToArray();
		}

		/// <summary>
		/// Returns the properties of a model class that are mapped to database columns
		/// </summary>
		/// <typeparam name="T">Model class type</typeparam>
		public IEnumerable<PropertyInfo> GetMappedColumns(Type modelType)
		{
			return modelType.GetProperties().Where(pi => IsMapped(pi) && IsSupportedType(pi.PropertyType));
		}

		private bool IsEditable(PropertyInfo pi, SaveAction action)
		{
			if (!IsSupportedType(pi.PropertyType)) return false;
			if (!IsMapped(pi)) return false;
			if (IsCalculated(pi)) return false;

			var colInfo = new ColumnInfo(pi);
			return ((colInfo.SaveActions & action) == action);
		}

		/// <summary>
		/// Determines whether a given Type is reflected in a database table
		/// </summary>
		protected bool IsSupportedType(Type type)
		{
			return
				SupportedTypes().ContainsKey(type) ||
				(type.IsEnum && type.GetEnumUnderlyingType().Equals(typeof(int))) ||
				(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsSupportedType(type.GetGenericArguments()[0]));
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
		/// Specifies the types and corresponding SQL syntax for CLR types supported in your ORM mapping
		/// </summary>
		protected abstract Dictionary<Type, string> SupportedTypes(int length = 0, int precision = 0, int scale = 0);

		/// <summary>
		/// Performs a SQL insert and returns the generated identity value
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="object">Model object. If based on <see cref="Record"/>, then permission checks and row-level events are triggered</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public TKey Insert<TModel>(IDbConnection connection, TModel @object, IUser user = null)
		{
			var record = @object as Record;
			record?.Validate(connection);
			if (user != null)
			{
				record?.CheckSavePermission(connection, user);
				record?.BeforeSave(connection, SaveAction.Insert, user);
			}

			string cmd = InsertCommand<TModel>();
			TKey result = connection.QuerySingleOrDefault<TKey>(cmd, @object);
			SetIdentity(@object, result);

			record?.AfterSave(connection, SaveAction.Insert);

			return result;
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
			var record = @object as Record;
			record?.Validate(connection);
			if (user != null)
			{
				record?.CheckSavePermission(connection, user);
				record?.BeforeSave(connection, SaveAction.Insert, user);
			}

			string cmd = InsertCommand<TModel>();
			TKey result = await connection.QuerySingleOrDefaultAsync<TKey>(cmd, @object);
			SetIdentity(@object, result);

			record?.AfterSave(connection, SaveAction.Insert);

			return result;
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
			var record = @object as Record;
			record?.Validate(connection);
			if (user != null)
			{
				record?.CheckSavePermission(connection, user);
				record?.BeforeSave(connection, SaveAction.Update, user);
			}

			string cmd = UpdateCommand<TModel>();
			connection.Execute(cmd, @object);

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
			var record = @object as Record;
			record?.Validate(connection);
			if (user != null)
			{
				record?.CheckSavePermission(connection, user);
				record?.BeforeSave(connection, SaveAction.Update, user);
			}

			string cmd = UpdateCommand<TModel>();
			await connection.ExecuteAsync(cmd, @object);

			record?.AfterSave(connection, SaveAction.Update);
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

		public bool ExistsWhere<TModel>(IDbConnection connection, TModel criteria, IUser user = null)
		{
			var record = FindWhere(connection, criteria, user);
			return (record != null);
		}

		public async Task<bool> ExistsWhereAsync<TModel>(IDbConnection connection, TModel criteria, IUser user = null)
		{
			var record = await FindWhereAsync(connection, criteria, user);
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
			TModel result = connection.QuerySingleOrDefault<TModel>(cmd, new { id = identity });
			LookupForeignKeys(connection, result);
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
			TModel result = await connection.QuerySingleOrDefaultAsync<TModel>(cmd, new { id = identity });
			LookupForeignKeys(connection, result);
			return FindInner(connection, result, user);
		}

		private void LookupForeignKeys<TModel>(IDbConnection connection, TModel result)
		{
			if (typeof(TKey) == typeof(int))
			{
				(result as Record)?.LookupIntForeignKeys(connection, this as CommandProvider<int>);
			}

			if (typeof(TKey) == typeof(long))
			{
				(result as Record)?.LookupLongForeignKeys(connection, this as CommandProvider<long>);
			}

			if (typeof(TKey) == typeof(Guid))
			{
				(result as Record)?.LookupGuidForeignKeys(connection, this as CommandProvider<Guid>);
			}
		}

		/// <summary>
		/// Gets a model object based on arbitrary criteria.
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="criteria">Object specifying the criteria to search for</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public TModel FindWhere<TModel>(IDbConnection connection, TModel criteria, IUser user = null)
		{
			string whereClause = WhereClauseFromObject(criteria);
			string cmd = FindCommand<TModel>(whereClause);
			TModel result = connection.QuerySingleOrDefault<TModel>(cmd, criteria);
			LookupForeignKeys(connection, result);
			return FindInner(connection, result, user);
		}

		/// <summary>
		/// Gets a model object based on arbitrary criteria.
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		/// <param name="criteria">Object specifying the criteria to search for</param>
		/// <param name="user">Information about the current user, used when object is based on <see cref="Record"/></param>
		public async Task<TModel> FindWhereAsync<TModel>(IDbConnection connection, TModel criteria, IUser user = null)
		{
			string whereClause = WhereClauseFromObject(criteria);
			string cmd = FindCommand<TModel>(whereClause);
			TModel result = await connection.QuerySingleOrDefaultAsync<TModel>(cmd, criteria);
			LookupForeignKeys(connection, result);
			return FindInner(connection, result, user);
		}

		private string WhereClauseFromObject<TModel>(TModel criteria)
		{
			var props = typeof(TModel).GetProperties().Where(pi => HasValue(pi, criteria));
			return string.Join(" AND ", props.Select(pi => $"{ApplyDelimiter(pi.GetColumnName())}=@{pi.Name}"));
		}

		private bool HasValue(PropertyInfo pi, object @object)
		{
			var value = pi.GetValue(@object);
			if (value != null)
			{
				var defaultValue = Activator.CreateInstance(pi.PropertyType);
				if (value.Equals(defaultValue)) return false;
				if (value.Equals(string.Empty)) return false;
				return true;
			}

			return false;
		}

		private static TModel FindInner<TModel>(IDbConnection connection, TModel result, IUser user)
		{
			var record = result as Record;
			if (user != null)
			{
				record?.CheckFindPermission(connection, user);
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
			if (user != null) record?.CheckDeletePermission(connection, user);

			string cmd = DeleteCommand<TModel>();
			connection.Execute(cmd, new { id = identity });

			record?.AfterDelete(connection);
		}

		public async Task DeleteAsync<TModel>(IDbConnection connection, TKey identity, IUser user = null)
		{
			var deleteMe = Find<TModel>(connection, identity, user);
			var record = deleteMe as Record;
			if (user != null) record?.CheckDeletePermission(connection, user);

			string cmd = DeleteCommand<TModel>();
			await connection.ExecuteAsync(cmd, new { id = identity });

			record?.AfterDelete(connection);
		}

		/// <summary>
		/// Creates a database table for a given model class
		/// </summary>
		/// <typeparam name="TModel">Model class type</typeparam>
		/// <param name="connection">Open connection</param>
		public void CreateTable<TModel>(IDbConnection connection)
		{
			string cmd = CreateTableCommand(typeof(TModel));
			connection.Execute(cmd);
		}

		/// <summary>
		/// Returns true if the property is mapped to a database table column
		/// </summary>
		protected bool IsMapped(PropertyInfo propertyInfo)
		{
			return !propertyInfo.HasAttribute<NotMappedAttribute>();
		}

		/// <summary>
		/// Returns true if the property has a [Calculated] attribute
		/// </summary>
		protected bool IsCalculated(PropertyInfo propertyInfo)
		{
			return propertyInfo.HasAttribute<CalculatedAttribute>();
		}
	}
}