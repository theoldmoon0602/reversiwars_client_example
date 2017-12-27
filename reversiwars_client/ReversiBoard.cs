using System;
using System.Linq;
using System.Collections.Generic;

namespace reversiwars_client
{
	/// <summary>
	///  この上でReversiをやる
	/// </summary>
	public class ReversiBoard
	{
		/// <summary>
		/// The board. length = 64
		/// </summary>
		private ReversiMark[] board;
		public static int size = 8;

		/// <summary>
		/// Initializes a new instance of the <see cref="reversiwars_client.ReversiBoard"/> class.
		/// </summary>
		public ReversiBoard ()
		{
			// boardの初期化
			this.board = new ReversiMark[64];
			for (int y = 0; y < size; y++) {
				for (int x = 0; x < size; x++) {
					this.Set (x, y, ReversiMark.EMPTY);
				}
			}
			this.Set (3, 3, ReversiMark.WHITE);
			this.Set (4, 4, ReversiMark.WHITE);
			this.Set (3, 4, ReversiMark.BLACK);
			this.Set (4, 3, ReversiMark.BLACK);
		}

		/// <summary>
		/// 座標がboardの範囲内にあればtrue
		/// </summary>
		/// <returns><c>true</c> if this instance is in the specified x y; otherwise, <c>false</c>.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public static bool IsIn(int x, int y) {
			return 0 <= x && x <= 7 && 0 <= y && y <= 7;
		}

		/// <summary>
		/// Asserts the is (x, y) in the board.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <exception cref="ReversiException">if (x, y) is out of the board</exception>
		public static void AssertIsIn(int x, int y) {
			if (! IsIn (x, y)) {
				throw new ReversiException(string.Format("position ({0}, {1}) is out of the board.", x, y));
			}
		}

		/// <summary>
		/// (x,y) を mark にする。ひっくり返すとかしません
		/// </summary>
		/// <param name="x">x座標( 0 <= x <= 7 )</param>
		/// <param name="y">y座標( 0 <= x <= 7 )</param>
		/// <param name="mark">Mark.</param>
		public void Set(int x, int y, ReversiMark mark) {
			ReversiBoard.AssertIsIn (x, y);
			this.board [size * y + x] = mark;
		}

		/// <summary>
		/// (x, y) の状態を返しますよ
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public ReversiMark At(int x, int y) {
			ReversiBoard.AssertIsIn (x, y);
			return this.board [size * y + x];
		}

		/// <summary>
		/// Count the specified mark.
		/// </summary>
		/// <param name="mark">Mark.</param>
		public int Count(ReversiMark mark) {
			return this.board.Where (a => a == mark).Count();
		}

		/// <summary>
		/// (x, y) に　mark を置く
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="mark">Mark.</param>
		public void PutAt(int x, int y, ReversiMark mark) {
			if (!this.IsPuttableAt(x,y,mark)) {
				throw new ReversiException (string.Format ("({0}, {1}) is not puttable for {2}", x, y, Enum.GetName(typeof(ReversiMark), mark)));
			}
			var rev = this.ReverseWhenPut (x, y, mark);
			this.Set (x, y, mark);
			foreach (Pos p in rev) {
				this.Set (p.X, p.Y, mark);
			}
		}

		/// <summary>
		/// mark を置ける場所の一覧を返す
		/// </summary>
		/// <returns>The puttables.</returns>
		/// <param name="mark">Mark.</param>
		public List<Pos> ListupPuttables(ReversiMark mark) {
			List<Pos> poses = new List<Pos> ();
			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					if (this.IsPuttableAt(x,y, mark)) {
						poses.Add (new Pos (x, y));
					}
				}
			}
			return poses;
		}

		/// <summary>
		/// (x, y) に　mark を置けるならtrueです
		/// </summary>
		/// <returns><c>true</c> if this instance is puttable at the specified x y mark; otherwise, <c>false</c>.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="mark">Mark.</param>
		public bool IsPuttableAt(int x, int y, ReversiMark mark) {
			if (!ReversiBoard.IsIn (x, y)) {
				return false;
			}
			if (this.At (x, y) != ReversiMark.EMPTY) {
				return false;
			}
			if (this.ReverseWhenPut (x, y, mark).Count == 0) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// (x,y) に　Mark をおいたときにひっくり返る座標のリストを返す。
		/// </summary>
		/// <returns>The when put.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="mark">Mark.</param>
		public List<Pos> ReverseWhenPut(int x, int y, ReversiMark mark) {
			List<Pos> poses = new List<Pos>();
			var dx = new int[]{-1, 0, 1, -1, 1, -1, 0, 1};
			var dy = new int[]{-1, -1, -1, 0, 0, 1, 1, 1};

			// 8近傍を探索
			for (int i = 0; i < dx.Length; i++) {
				if (!ReversiBoard.IsIn (x + dx [i], y + dy [i])) {
					continue;
				}
				// 隣が反対の色であることが重要
				if ((int)this.At (x + dx [i], y + dy [i]) == -(int)mark) {
					List<Pos> candidates = new List<Pos> ();
					bool flag = false; // その方向にひっくり返せるかのフラグ
					for (int j = 1; true; j++) {
						var x2 = x + dx [i] * j;
						var y2 = y + dy [i] * j;
						if (!ReversiBoard.IsIn (x2, y2)) {
							break;
						}

						if ((int)this.At (x2, y2) == -(int)mark) {
							candidates.Add (new Pos (x2, y2));
						} else if (this.At (x2, y2) == mark) {
							flag = true;
							break;
						} else {
							break;
						}
					}
					if (flag) {
						poses.AddRange (candidates);
					}
				}
			}
			return poses;
		}

		/// <summary>
		/// Board as the string.
		/// </summary>
		/// <returns>The string.</returns>
		public String BoardString() {
			String s = "v01234567v\n";
			for (int y = 0; y < size; y++) {
				s += "|";
				for (int x = 0; x < size; x++) {
					if (this.At (x, y) == ReversiMark.BLACK) {
						s += "x";
					} else if (this.At (x, y) == ReversiMark.WHITE) {
						s += "o";
					} else {
						s += "_";
					}
				}
				s += y.ToString ();
				s += "\n";
			}
			s += "^^^^^^^^^^\n";
			return s;
		}
	}
}

