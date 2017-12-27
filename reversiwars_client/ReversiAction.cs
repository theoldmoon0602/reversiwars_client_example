using System;

namespace reversiwars_client
{
	public class ReversiAction
	{
		private bool isPass;
		private Pos putAt;
		private ReversiAction ()	{
		}
		public bool IsPass() {
			return this.isPass;
		}
		public Pos GetPutAt() {
			return this.putAt;
		}
		public static ReversiAction Pass() {
			var act = new ReversiAction ();
			act.isPass = true;
			return act;
		}
		public static ReversiAction PutAt(int x, int y) {
			var act = new ReversiAction ();
			act.isPass = false;
			act.putAt = new Pos (x, y);
			return act;
		}
	}
}