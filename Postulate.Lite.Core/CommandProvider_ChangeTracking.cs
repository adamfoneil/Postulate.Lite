using Dapper;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Interfaces;
using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Postulate.Lite.Core
{
	public abstract partial class CommandProvider<TKey>
	{
		private bool IsTrackingChanges<TModel>(out string[] ignoreProperties)
		{
			ignoreProperties = null;
			var attr = typeof(TModel).GetAttribute<TrackChangesAttribute>();
			if (attr == null) return false;

			ignoreProperties = (!string.IsNullOrEmpty(attr.IgnoreProperties)) ?
				attr.IgnoreProperties.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray() :
				Enumerable.Empty<string>().ToArray();
			return true;
		}

		protected abstract bool SchemaExists(IDbConnection connection, TableInfo table);
		protected abstract bool TableExists(IDbConnection connection, TableInfo table);
		protected abstract string CreateTableScript(TableInfo table, Type modelType);

		private async Task<IEnumerable<PropertyChange>> GetChangesAsync<TModel>(IDbConnection connection, TModel @object)
		{			
			if (IsTrackingChanges<TModel>(out string[] ignoreProperties))
			{
				var existing = await FindAsync<TModel>(connection, GetIdentity(@object));
				if (existing != null) return GetPropertyChanges(connection, existing, @object, ignoreProperties);
			}
			return null;
		}

		private IEnumerable<PropertyChange> GetChanges<TModel>(IDbConnection connection, TModel @object)
		{
			if (IsTrackingChanges<TModel>(out string[] ignoreProperties))
			{
				var existing = Find<TModel>(connection, GetIdentity(@object));
				if (existing != null) return GetPropertyChanges(connection, existing, @object, ignoreProperties);
			}
			return null;
		}

		private async Task SaveChangesAsync<TModel>(IDbConnection connection, TKey identity, IEnumerable<PropertyChange> changes, IUser user)
		{
			if (user == null) return;
			if (!changes?.Any() ?? false) return;

			VerifyChangeTrackingObjects<TModel>(connection);

			int version = await GetNextRecordVersionAsync<TModel>(connection);

			foreach (var change in changes)
			{
				PropertyChangeHistory<TKey> history = GetChangeHistoryRecord(identity, user, version, change);
				await InsertAsync(connection, history);
			}
		}

		private static PropertyChangeHistory<TKey> GetChangeHistoryRecord(TKey identity, IUser user, int version, PropertyChange change)
		{
			return new PropertyChangeHistory<TKey>()
			{
				RecordId = identity,
				Version = version,
				ColumnName = change.PropertyName,
				UserName = user?.UserName ?? "<unknown>",
				DateTime = user?.LocalTime ?? DateTime.UtcNow,
				OldValue = CleanMinDate(change.OldValue)?.ToString() ?? "<null>",
				NewValue = CleanMinDate(change.NewValue)?.ToString() ?? "<null>"
			};
		}

		private static object CleanMinDate(object value)
		{
			// prevents DateTime.MinValue from getting passed to SQL Server as a parameter, where it fails
			if (value is DateTime && value.Equals(default(DateTime))) return null;
			return value;
		}

		private Task<int> GetNextRecordVersionAsync<TModel>(IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private void SaveChanges<TModel>(IDbConnection connection, TKey identity, IEnumerable<PropertyChange> changes, IUser user)
		{
			if (user == null) return;
			if (!changes?.Any() ?? false) return;

			VerifyChangeTrackingObjects<TModel>(connection);

			int version = GetNextRecordVersion<TModel>(connection);

			foreach (var change in changes)
			{
				PropertyChangeHistory<TKey> history = GetChangeHistoryRecord(identity, user, version, change);
				Insert(connection, history);
			}
		}

		private int GetNextRecordVersion<TModel>(IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private void VerifyChangeTrackingObjects<TModel>(IDbConnection connection)
		{
			var table = _integrator.GetTableInfo(typeof(TModel));

			const string changesSchema = "changes";

			if (!SchemaExists(connection, changesSchema)) connection.Execute(CreateSchemaCommand(changesSchema));
			
			TableInfo historyTbl = new TableInfo(changesSchema, $"{table.Schema}_{table.Name}_History");
			CreateTableIfNotExists(connection, historyTbl, typeof(PropertyChangeHistory<TKey>));

			TableInfo versionTbl = new TableInfo(changesSchema, $"{table.Schema}_{table.Name}_Version");
			CreateTableIfNotExists(connection, versionTbl, typeof(RowVersion<TKey>));
		}

		private void CreateTableIfNotExists(IDbConnection connection, TableInfo table, Type modelType)
		{
			if (!TableExists(connection, table))
			{
				var script = CreateTableScript(table, modelType);
				connection.Execute(script);
			}
		}

		private IEnumerable<PropertyChange> GetPropertyChanges<TModel>(IDbConnection connection, TModel currentRecord, TModel newRecord, string[] ignoreProperties)
		{
			var properties = _integrator.GetEditableColumns(typeof(TModel), SaveAction.Update);
			return properties
				.Where(col => !ignoreProperties.Contains(col.PropertyName))
				.Select(col => new PropertyChange()
				{
					PropertyName = col.PropertyName,
					OldValue = GetPropertyValue(connection, col.PropertyInfo, currentRecord),
					NewValue = GetPropertyValue(connection, col.PropertyInfo, newRecord)
				}).Where(pc => pc.IsChanged());
		}
		
		private object GetPropertyValue<TModel>(IDbConnection connection, PropertyInfo propertyInfo, TModel record)
		{
			object result = propertyInfo.GetValue(record);
			if (result == null) return null;

			var lookupQuery = propertyInfo.GetAttribute<ForeignKeyLookupAttribute>();
			if (lookupQuery != null)
			{
				var lookup = connection.QuerySingleOrDefault<ForeignKeyLookup>(lookupQuery.Query, new { id = result });
				if (lookup != null) result = lookup.Text;
			}

			return result;
		}
	}
}