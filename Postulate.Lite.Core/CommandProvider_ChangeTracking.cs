using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
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

		private async Task SaveChangesAsync(IDbConnection connection, IEnumerable<PropertyChange> changes)
		{
			if (!changes?.Any() ?? false) return;
		}

		private void SaveChanges(IDbConnection connection, IEnumerable<PropertyChange> changes)
		{
			if (!changes?.Any() ?? false) return;
		}

		private IEnumerable<PropertyChange> GetPropertyChanges<TModel>(IDbConnection connection, TModel currentRecord, TModel newRecord, string[] ignoreProperties)
		{
			var properties = _integrator.GetEditableColumns(typeof(TModel), SaveAction.Update);
			return properties
				.Where(col => !ignoreProperties.Contains(col.PropertyName))
				.Select(col => new PropertyChange()
				{
					PropertyName = col.PropertyName,
					OldValue = OnGetChangesPropertyValue(col.PropertyInfo, currentRecord, connection),
					NewValue = OnGetChangesPropertyValue(col.PropertyInfo, newRecord, connection)
				}).Where(pc => pc.IsChanged());
		}

		protected virtual object OnGetChangesPropertyValue<TModel>(PropertyInfo propertyInfo, TModel record, IDbConnection connection)
		{
			return propertyInfo.GetValue(record);
		}
	}
}