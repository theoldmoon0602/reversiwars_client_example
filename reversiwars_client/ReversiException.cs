using System;

namespace reversiwars_client
{
	/// <summary>
	/// Reversi exception.
	/// </summary>
	public class ReversiException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="reversiwars_client.ReversiException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		public ReversiException (string message): base(message)
		{
		}
	}
}

