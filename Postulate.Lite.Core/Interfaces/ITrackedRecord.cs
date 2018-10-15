using Postulate.Lite.Core.Models;
using System.Collections.Generic;
using System.Data;

namespace Postulate.Lite.Core.Interfaces
{
	public interface ITrackedRecord
	{
		/// <summary>
		/// Indicates whether changes are saved to the default history table
		/// </summary>
		bool UseDefaultHistoryTable { get; }

		/// <summary>
		/// Enables custom save behavior for property changes
		/// </summary>
		void TrackChanges(IDbConnection connection, int version, IEnumerable<PropertyChange> changes, IUser user);
	}
}