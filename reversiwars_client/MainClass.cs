using System;
using System.Net.Sockets;
using System.Text;
using Codeplex.Data;

namespace reversiwars_client
{


	public class MainClass
	{
		static NetworkStream stream;
		public static string Recv() {
			byte[] buf = new byte[1024];
			stream.Read (buf, 0, buf.Length);
			return Encoding.UTF8.GetString (buf).Split('\n')[0];
		}
		public static void Send(string msg) {
			byte[] buf = Encoding.UTF8.GetBytes (msg+"\n");
			stream.Write (buf, 0, buf.Length);
			stream.Flush ();
		}

		public static void Main(string[] args) {
			TcpClient client = new TcpClient ("localhost", 8888);
			stream = client.GetStream ();

			dynamic json;
			// 繋がっていることを確認する
			Connect ();
			json = LoginOrRegister ();
			json = WaitOrBattle (json);
			Battle (json);
		}

		public static void Connect() {
			dynamic json = DynamicJson.Parse (Recv ());
			string a = CheckResult (json);
			if (a.Length == 0) {
				return;
			}
			throw new Exception (a);
		}

		// これは行儀が悪い関数。
		public static string CheckResult(dynamic json) {
			if (!json.IsDefined ("result")) {
				return "result is not defined, please contact to admin";
			}
			if (json.result != "true") {
				if (!json.IsDefined ("msg")) {
					return "result is false, but msg is not defined. please contact to admin";
				}
				return "false: " + json.msg; // msg が空だとまずいので
			}
			return "";
		}

		public static dynamic LoginOrRegister() {
			dynamic json;
			while (true) {
				// 各種情報の入力
				string username, password, login_register;
				Console.Write ("Username: ");
				username = Console.ReadLine ();
				Console.Write ("Password: ");
				password = Console.ReadLine ();

				// login か register か
				Console.Write ("Login(0) or Register(1)? -->");
				login_register = Console.ReadLine ();
				var userinfo = new {
					username = username,
					password = password,
				};
				if (login_register != "0" && login_register != "1") {
					continue;
				}

				// ログイン情報の作成
				var logininfo = new {
					action = (login_register == "0") ? "login" : "register",
					userinfo = userinfo
				};
				// 送信
				Send(DynamicJson.Serialize(logininfo));

				// 結果を受け取る
				json = DynamicJson.Parse(Recv());
				string r = CheckResult (json);
				if (r.Length == 0) {
					break;
				}

				// 失敗したのでもう一回
				Console.WriteLine ("ERROR: "+r);
			}
			return json;
		}

		public static dynamic WaitOrBattle(dynamic data) {
			dynamic json;
			while (true) {
				if (!data.IsDefined ("users")) {
					throw new Exception ("users column is not defined");
				}
				// 対戦待ちのユーザを列挙
				Console.WriteLine ("Users:");
				foreach (dynamic user in data.users) {
					Console.WriteLine (string.Format ("  username: {0}, rating: {1}", user.name, user.rating));
				}

				// 待つか戦うか
				string wait_battle;
				Console.Write ("Wait(0) or Battle(1)? -->");
				wait_battle = Console.ReadLine ();
				if (wait_battle != "0" && wait_battle != "1") {
					continue;
				}
				else if (wait_battle == "0") {
					var obj = new {
						action = "wait"
					};
					// 送信
					Send (DynamicJson.Serialize (obj));
				} else if (wait_battle == "1") {
					// 戦うなら誰と戦うか
					string username;
					Console.Write ("battle with :");
					username = Console.ReadLine ();
					var obj = new {
						action = "battle",
						user = username
					};
					// 送信
					Send (DynamicJson.Serialize (obj));
				}

				// 受信
				json = DynamicJson.Parse(Recv());
				string r = CheckResult (json);
				if (r.Length == 0) {
					break;
				}

				// 失敗してた
				Console.WriteLine("ERROR: " + r);
			}
			return json;
		}

		// 戦うよ、君が立ち上がり続ける限りは
		public static void Battle(dynamic json) {
			ReversiRemotePlayer remote = new ReversiRemotePlayer (); // あいて
			ReversiRandomPlayer random = new ReversiRandomPlayer (); // じぶん
			ReversiManager reversi; // こいつがリバーシと言える
			ReversiAction　nextAction; // いろんなところで使うから先に宣言しているけどこういうの良くない

			if (!json.IsDefined ("first")) {
				throw new Exception ("first is not defined, contact to admin");
			}

			if (json.first == "true") {
				// 先攻
				random.SetMark(ReversiMark.BLACK);
				remote.SetMark (ReversiMark.WHITE);
				reversi = new ReversiManager (random, remote);

				// 先攻なので行動します
				nextAction = reversi.Next();
				var obj = new {
					action = "put",
					pos = new int[]{ nextAction.GetPutAt ().X, nextAction.GetPutAt ().Y }
				};
				Send (DynamicJson.Serialize (obj));

				dynamic recv = DynamicJson.Parse(Recv ());
				Console.WriteLine (recv);
				string r = CheckResult (recv);
				if (r.Length > 0) {
					// ここで false が返ってきててもどうにもできない
					throw new Exception (r);
				}

			} else {
				// 後攻
				remote.SetMark (ReversiMark.BLACK);
				random.SetMark(ReversiMark.WHITE);
				reversi = new ReversiManager (remote, random);
			}

			// 戦いの渦中に身を置く
			while (true) {
				dynamic recv = DynamicJson.Parse(Recv ());
				string r = CheckResult (recv);
				if (r.Length > 0) {
					// ここで false が返ってきててもどうにもできない
					throw new Exception (r);
				}
				if (CheckEnd (recv)) {
					break;
				}

				// あいての手をremoteに反映
				if (recv.action == "pass") {
					remote.SetNextAction (ReversiAction.Pass ());
					reversi.Next ();
				} else if (recv.action == "put") {
					int x = (int)recv.pos [0]; // double で返ってくる
					int y = (int)recv.pos [1];
					nextAction = ReversiAction.PutAt (x, y);
					remote.SetNextAction (nextAction);
					reversi.Next ();
				} else { // close がありうる（けどここまでこないな）
					break;
				}

				while (true) {
					// 自分の番
					nextAction = reversi.Next ();
					if (nextAction.IsPass ()) {
						var obj = new {
							action = "pass"
						};
						Send (DynamicJson.Serialize (obj));
					} else {
						var obj = new {
							action = "put",
							pos = new int[]{ nextAction.GetPutAt ().X, nextAction.GetPutAt ().Y }
						};
						Send (DynamicJson.Serialize (obj));
					}
					recv = DynamicJson.Parse(Recv ());
					r = CheckResult (recv);
					if (r.Length > 0) {
						Console.WriteLine ("ERROR: " + r);
						continue;
					}
					break;
				}
				if (CheckEnd (recv)) {
					break;
				}
			}
		}

		public static bool CheckEnd(dynamic recv) {
			// IsDefined でチェックするの面倒になってきたのでやめました
			if (recv.isGameEnd == "true") {
				if (recv.isDraw == "true") {
					Console.WriteLine ("DRAW");
				} else if (recv.youWin == "true") {
					Console.WriteLine ("WIN");
				} else {
					Console.WriteLine ("LOSE");
				}
				return true;
			}
			return false;
		}
	}
}

