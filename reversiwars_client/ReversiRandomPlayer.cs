using System;

namespace reversiwars_client
{
	/// <summary>
	/// 置けるところからランダムにおく　Player
	/// </summary>
	public class ReversiRandomPlayer : ReversiPlayer
	{
		private ReversiMark mark;
		private ReversiAction nextAction;
		public ReversiRandomPlayer ()
		{
		}
		public void SetMark(ReversiMark mark) {
			this.mark = mark;
		}
		public ReversiMark GetMark() {
			return this.mark;
		}
		public ReversiAction GetNextAction() {
			return this.nextAction;
		}
		public void CalcNextAction(ReversiBoard board) {
			var puttables = board.ListupPuttables(this.mark);
			if (puttables.Count == 0) {
				this.nextAction = ReversiAction.Pass ();
				return;
			}
			Random rd = new Random ();
			var p =  puttables [rd.Next (puttables.Count)];
			this.nextAction = ReversiAction.PutAt (p.X, p.Y);
		}
	}
}

