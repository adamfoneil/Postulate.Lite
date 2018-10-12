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

		private async Task SaveChangesAsync<TModel>(IDbConnection connection, IEnumerable<PropertyChange> changes, IUser user)
		{
			if (user == null) return;
			if (!changes?.Any() ?? false) return;

			VerifyChangeTrackingObjects(connection, typeof(TModel));
		}

		private void SaveChanges<TModel>(IDbConnection connection, IEnumerable<PropertyChange> changes, IUser user)
		{
			if (user == null) return;
			if (!changes?.Any() ?? false) return;

			VerifyChangeTrackingObjects(connection, typeof(TModel));
		}

		private void VerifyChangeTrackingObjects(IDbConnection connection, Type type)
		{
			throw new NotImplementedException();
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