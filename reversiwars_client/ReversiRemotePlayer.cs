using System;

namespace reversiwars_client
{
	/// <summary>
	/// はい
	/// </summary>
	public class ReversiRemotePlayer : ReversiPlayer
	{
		private ReversiMark mark;
		private ReversiAction nextAction;
		public ReversiRemotePlayer ()
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
		public void SetNextAction(ReversiAction nextAction) {
			this.nextAction = nextAction;
		}
		public void CalcNextAction(ReversiBoard board) {
			
		}
	}
}

