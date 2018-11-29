using System.Data;

namespace Postulate.Lite.Core.Interfaces
{
	/// <summary>
	/// Implement this on model types that require foreign key lookups whenever a record is accessed
	/// </summary>	
	public interface IFindRelated<TKey>
	{
		/// <summary>
		/// Use this to set navigation properties whenever a record is accessed
		/// </summary>
		void FindRelated(IDbConnection connection, CommandProvider<TKey> commandProvider);
	}
}