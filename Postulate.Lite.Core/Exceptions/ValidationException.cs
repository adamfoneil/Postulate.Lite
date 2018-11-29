using System;

namespace Postulate.Lite.Core.Exceptions
{
	/// <summary>
	/// Can be thrown by <see cref="Record.Validate(System.Data.IDbConnection)"/>
	/// </summary>
	public class ValidationException : Exception
	{
		public ValidationException(string message) : base(message)
		{
		}
	}
}