using System;

namespace Postulate.Lite.Core.Exceptions
{
	/// <summary>
	/// Can be thrown by any of the Crud methods
	/// </summary>
	public class CrudException : Exception
	{
		public CrudException(SaveAction action, Exception source, string commandText, object @object) : base(source.Message, source)
		{
			Action = action;
			CommandText = commandText;
			Parameters = @object;
		}

		public SaveAction Action { get; private set; }
		public string CommandText { get; private set; }
		public object Parameters { get; private set; }
	}
}