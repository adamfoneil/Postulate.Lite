using System;

namespace Postulate.Lite.Core.Interfaces
{
	public interface IUser
	{
		string UserName { get; }
		DateTime LocalTime { get; }
	}
}