using System;

namespace reversiwars_client
{
	public interface ReversiPlayer
	{
		void SetMark(ReversiMark mark);
		ReversiMark GetMark();
		void CalcNextAction (ReversiBoard board);
		ReversiAction GetNextAction();
	}
}

