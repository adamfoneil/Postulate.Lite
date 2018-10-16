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

		const string changesSchema = "changes";
		
		protected abstract bool TableExists(IDbConnection connection, TableInfo table);
		protected abstract string CreateTableScript(TableInfo table, Type modelType);

		/// <summary>
		/// Query that returns the max version number for a given RecordId (must use parameter @id)
		/// </summary>
		protected abstract string SqlSelectNextVersion(string tableName);

		protected abstract string SqlUpdateNextVersion(string tableName);

		protected abstract string SqlInsertRowVersion(string tableName);

		private async Task<IEnumerable<PropertyChange>> GetChangesAsync<TModel>(IDbConnection connection, TModel @object)
		{			
			if (IsTrackingChanges<TModel>(out string[] ignoreProperties))
			{
				var existing = await FindAsync<TModel>(connection, GetIdentity(@object));
				if (existing != null) return GetPropertyChanges(connection, existing, @object, ignoreProperties).ToArray();
			}
			return null;
		}

		private IEnumerable<PropertyChange> GetChanges<TModel>(IDbConnection connection, TModel @object)
		{
			if (IsTrackingChanges<TModel>(out string[] ignoreProperties))
			{
				var existing = Find<TModel>(connection, GetIdentity(@object));
				if (existing != null) return GetPropertyChanges(connection, existing, @object, ignoreProperties).ToArray();
			}
			return null;
		}

		private async Task SaveChangesAsync<TModel>(IDbConnection connection, TModel @object, IEnumerable<PropertyChange> changes, IUser user)
		{			
			if (!changes?.Any() ?? true) return;

			var trackedRecord = @object as ITrackedRecord;
			bool useHistoryTable = trackedRecord?.UseDefaultHistoryTable ?? true;
			VerifyChangeTrackingObjects<TModel>(connection, useHistoryTable, out string historyTableName);

			TKey identity = GetIdentity(@object);
			int version = await IncrementNextRecordVersionAsync<TModel>(connection, identity);

			if (useHistoryTable)
			{				
				foreach (var change in changes)
				{
					PropertyChangeHistory<TKey> history = GetChangeHistoryRecord(identity, user, version, change);
					await PlainInsertAsync(connection, history, historyTableName);
				}
			}
			
			trackedRecord?.TrackChanges(connection, version, changes, user);
		}

		private void SaveChanges<TModel>(IDbConnection connection, TModel @object, IEnumerable<PropertyChange> changes, IUser user)
		{
			if (!changes?.Any() ?? true) return;

			var trackedRecord = @object as ITrackedRecord;
			bool useHistoryTable = trackedRecord?.UseDefaultHistoryTable ?? true;
			VerifyChangeTrackingObjects<TModel>(connection, useHistoryTable, out string historyTableName);

			TKey identity = GetIdentity(@object);
			int version = IncrementNextRecordVersion<TModel>(connection, identity);

			if (useHistoryTable)
			{				
				foreach (var change in changes)
				{
					PropertyChangeHistory<TKey> history = GetChangeHistoryRecord(identity, user, version, change);					
					PlainInsert(connection, history, historyTableName);
				}
			}

			trackedRecord?.TrackChanges(connection, version, changes, user);
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

		private async Task<int> IncrementNextRecordVersionAsync<TModel>(IDbConnection connection, TKey identity)
		{
			var table = _integrator.GetTableInfo(typeof(TModel));
			var versionTbl = GetVersionTableInfo(table, changesSchema);

			var param = new { id = identity };
			string tableName = versionTbl.ToString();

			int result = await connection.QuerySingleOrDefaultAsync<int?>(SqlSelectNextVersion(tableName), param) ?? 0;

			if (result == 0)
			{
				RowVersion<TKey> initialVersion = new RowVersion<TKey>()
				{
					RecordId = identity,
					NextVersion = 1
				};
				await PlainInsertAsync(connection, initialVersion, tableName);
				result = 1;
			}

			await connection.ExecuteAsync(SqlUpdateNextVersion(tableName), param);

			return result;
		}

		private int IncrementNextRecordVersion<TModel>(IDbConnection connection, TKey identity)
		{
			var table = _integrator.GetTableInfo(typeof(TModel));
			var versionTbl = GetVersionTableInfo(table, changesSchema);

			var param = new { id = identity };
			string tableName = versionTbl.ToString();

			int result = connection.QuerySingleOrDefault<int?>(SqlSelectNextVersion(tableName), param) ?? 0;

			if (result == 0)
			{
				RowVersion<TKey> initialVersion = new RowVersion<TKey>()
				{
					RecordId = identity,
					NextVersion = 1
				};
				PlainInsert(connection, initialVersion, tableName);				
				result = 1;
			}

			connection.Execute(SqlUpdateNextVersion(tableName), param);

			return result;
		}

		private void VerifyChangeTrackingObjects<TModel>(IDbConnection connection, bool createHistoryTable, out string historyTableName)
		{
			historyTableName = null;
			var targetTable = _integrator.GetTableInfo(typeof(TModel));			

			if (!SchemaExists(connection, changesSchema)) connection.Execute(CreateSchemaCommand(changesSchema));

			if (createHistoryTable)
			{
				TableInfo historyTbl = new TableInfo(changesSchema, $"{targetTable.Schema}_{targetTable.Name}_History");
				historyTableName = historyTbl.ToString();
				CreateTableIfNotExists(connection, historyTbl, typeof(PropertyChangeHistory<TKey>));
			}

			TableInfo versionTbl = GetVersionTableInfo(targetTable, changesSchema);
			CreateTableIfNotExists(connection, versionTbl, typeof(RowVersion<TKey>));
		}

		private static TableInfo GetVersionTableInfo(TableInfo table, string changesSchema)
		{
			return new TableInfo(changesSchema, $"{table.Schema}_{table.Name}_Version");
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

			if (DereferenceId(connection, propertyInfo, out DereferenceIdAttribute attr))
			{
				var lookup = connection.QuerySingleOrDefault<IdLookup>(attr.Query, new { id = result });
				if (lookup != null) result = lookup.Name;
			}

			return result;
		}

		private bool DereferenceId(IDbConnection connection, PropertyInfo propertyInfo, out DereferenceIdAttribute attr)
		{			
			var fk = propertyInfo.GetAttribute<ReferencesAttribute>();
			if (fk != null)
			{
				var attributes = fk.PrimaryType.GetCustomAttributes<DereferenceIdAttribute>();
				attr = attributes.FirstOrDefault(a => a.ConnectionType?.Equals(connection.GetType()) ?? false);
				if (attr != null) return true;

				attr = attributes.FirstOrDefault();
				return (attr != null);
			}

			attr = null;
			return false;
		}
	}
}