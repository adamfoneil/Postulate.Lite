using System;

namespace Postulate.Lite.Core.Exceptions
{
	public class PermissionException : Exception
	{
		public PermissionException(string message) : base(message)
		{
		}
	}
}