using System;

namespace reversiwars_client
{
	public class ReversiManager
	{
		private ReversiBoard board;
		private ReversiPlayer[] players;
		private int turn;
		public ReversiManager (ReversiPlayer p1, ReversiPlayer p2)
		{
			this.board = new ReversiBoard ();
			this.players = new ReversiPlayer[]{ p1, p2 };
			this.turn = 0;

			// check marks
			if (p1.GetMark () == ReversiMark.EMPTY) {
				throw new ReversiException ("player mark cannot be empty");
			}
			if ((int)p1.GetMark () * (-1) != (int)p2.GetMark ()) {
				throw new ReversiException ("player marks are invalid");
			}
		}

		/// <summary>
		/// Determines whether this instance is game end.
		/// </summary>
		/// <returns><c>true</c> if this instance is game end; otherwise, <c>false</c>.</returns>
		public bool IsGameEnd() {
			return this.board.ListupPuttables (ReversiMark.BLACK).Count == 0 &&
			this.board.ListupPuttables (ReversiMark.WHITE).Count == 0;
		}

		public int GetTurn() {
			return this.turn;
		}

		public ReversiBoard GetBoard() {
			return this.board;
		}

		public ReversiPlayer GetTurnPlayer() {
			return this.players [this.turn % 2];
		}

		/// <summary>
		/// このターンの動きをやります
		/// </summary>
		public ReversiAction Next() {
			if (this.IsGameEnd ()) {
				return ReversiAction.Pass ();
			}
			GetTurnPlayer ().CalcNextAction (this.board);
			var nextAction = GetTurnPlayer().GetNextAction ();
			if (! nextAction.IsPass ()) {
				var putAt = nextAction.GetPutAt ();
				this.board.PutAt (putAt.X, putAt.Y, GetTurnPlayer ().GetMark ());
			}
			turn++;
			return nextAction;
		}
	}
}

