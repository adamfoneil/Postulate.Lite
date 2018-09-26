using System.Collections.Generic;

namespace Postulate.Lite.Core.Interfaces
{
	public interface ISeedData<T>
	{
		IEnumerable<T> GetRecords();
	}
}