using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using Libraries.Socket.Http;

namespace Utilities.Socket.Http {
	/// <summary>
	/// HTTP通信サンプルクラスです。
	/// </summary>
	internal static class Program {
		#region 内部メソッド定義(出力処理)
		/// <summary>
		/// 内容情報を出力します。
		/// </summary>
		/// <param name="source">内容情報</param>
		/// <param name="decode">変換処理</param>
		private static void OutputText(ContentData source, Encoding decode) {
			using (var stream = source.CreateStream())
			using (var reader = new StreamReader(stream, decode)) {
				Console.WriteLine(reader.ReadToEnd());
			}
		}
		/// <summary>
		/// 内容情報を出力します。
		/// </summary>
		/// <param name="source">内容情報</param>
		private static void OutputData(ContentData source) {
			using (var writer = new StringWriter()) {
				source.OutputWriter(writer);
				Console.WriteLine(writer.ToString());
			}
		}
		/// <summary>
		/// 内容情報を出力します。
		/// </summary>
		/// <param name="source">内容情報</param>
		/// <param name="header">属性情報</param>
		private static void OutputData(ContentData source, ElementList header) {
			var values = ContentCode.CreateData(header.ChooseText("Content-Type"));
			switch (values.ContentText) {
			case "text/html":
				if (values.ElementList.ChooseText("charset", out var encode)) {
					OutputText(source, Encoding.GetEncoding(encode));
				} else {
					OutputText(source, Encoding.UTF8);
				}
				break;
			default:
				OutputData(source);
				break;
			}
		}
		/// <summary>
		/// 受信情報を出力します。
		/// </summary>
		/// <param name="source">受信情報</param>
		private static void OutputData(ReceiveData source) =>
			OutputData(source.ContentData, source.ElementList);
		#endregion 内部メソッド定義(出力処理)

		#region 内部メソッド定義(生成処理)
		/// <summary>
		/// 要求情報を生成します。
		/// </summary>
		/// <param name="source">接続情報</param>
		/// <returns>要求情報</returns>
		private static RequestData CreateData(ConnectData source) {
			var result = new List<ElementData>();
			result.Add(new ElementData("Host",            $"{source.ServerName}:{source.ServerPort}"));
			result.Add(new ElementData("Accept-Encoding", "gzip, deflate, br"));
			return new RequestData("1.1", "GET", source.AccessPath, new ElementList(result));
		}
		#endregion 内部メソッド定義(生成処理)

		#region 内部メソッド定義(通信処理)
		/// <summary>
		/// 通信処理を実行します。
		/// </summary>
		/// <param name="stream">通信処理</param>
		/// <param name="source">要求情報</param>
		/// <returns>受信情報</returns>
		private static ReceiveData InvokeData(Stream stream, RequestData source) {
			// 要求情報送信
			source.OutputStream(stream);
			// 応答情報受信
			return ReceiveData.CreateData(stream);
		}
		/// <summary>
		/// 通信処理を実行します。
		/// </summary>
		/// <param name="source">接続情報</param>
		/// <returns>受信情報</returns>
		private static ReceiveData InvokeData(ConnectData source) {
			using (var client = new TcpClient(source.ServerName, source.ServerPort))
			using (var stream = client.GetStream()) {
				if (source.SecureFlag) {
					using (var secure = new SslStream(stream)) {
						return InvokeData(secure, CreateData(source));
					}
				} else {
					return InvokeData(stream, CreateData(source));
				}
			}
		}
		#endregion 内部メソッド定義(通信処理)

		/// <summary>
		/// HTTP通信サンプルを実行します。
		/// </summary>
		/// <param name="commands">コマンドライン引数</param>
		public static void Main(string[] commands) {
			try {
				var source = new ConnectData(false, "localhost", 80, "/");
				var result = InvokeData(source);
				OutputData(result);
			} catch (Exception error) {
				Console.WriteLine(error);
			}
		}

		#region 非公開クラス定義
		/// <summary>
		/// 内容情報クラスです。
		/// </summary>
		private class ContentCode {
			/// <summary>
			/// 内容種別を取得します。
			/// </summary>
			/// <value>内容種別</value>
			public string ContentText {
				get;
			}
			/// <summary>
			/// 要素一覧を取得します。
			/// </summary>
			/// <value>要素一覧</value>
			public ElementList ElementList {
				get;
			}

			/// <summary>
			/// 内容情報を生成します。
			/// </summary>
			/// <param name="contentText">内容種別</param>
			/// <param name="elementList">要素一覧</param>
			private ContentCode(string contentText, ElementList elementList) {
				ContentText = contentText;
				ElementList = elementList;
			}
			/// <summary>
			/// 内容情報を生成します。
			/// </summary>
			/// <param name="source">読込情報</param>
			/// <returns>内容情報</returns>
			public static ContentCode CreateData(string source) {
				if (!ChooseCode(source, '/', 0, out var value1)) {
					throw new Exception("Not found separate character.");
				} else if (!ChooseCode(source, ';', value1 + 1, out var value2)) {
					return new ContentCode(source, new ElementList(new ElementData[0]));
				} else {
					var choose = source.Substring(0, value2);
					var values = CreateList(source.Substring(value2 + 1));
					return new ContentCode(choose, values);
				}
			}

			/// <summary>
			/// 指定文字を検索します。
			/// </summary>
			/// <param name="source">要素情報</param>
			/// <param name="search">検索文字</param>
			/// <param name="offset">開始位置</param>
			/// <param name="result">検索結果</param>
			/// <returns><paramref name="search" />が存在した場合、<c>True</c>を返却</returns>
			private static bool ChooseCode(string source, char search, int offset, out int result) {
				result = source.IndexOf(search, offset);
				return 0 <= result;
			}
			/// <summary>
			/// 要素一覧を生成します。
			/// </summary>
			/// <param name="source">要素情報</param>
			/// <returns>要素一覧</returns>
			private static ElementList CreateList(string source) {
				var result = new List<ElementData>();
				foreach (var choose in source.Split(';')) {
					var offset = choose.IndexOf('=');
					if (offset < 0) {
						throw new Exception("Not found equal character.");
					} else {
						var value1 = choose.Substring(0, offset);
						var value2 = choose.Substring(offset + 1);
						result.Add(new ElementData(value1.Trim(), value2.Trim()));
					}
				}
				return new ElementList(result);
			}
		}
		#endregion 非公開クラス定義
	}
}
