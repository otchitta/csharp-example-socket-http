using System.IO;

namespace Libraries.Socket.Http {
	/// <summary>
	/// 要求情報クラスです。
	/// </summary>
	public sealed class RequestData {
		#region プロパティー定義
		/// <summary>
		/// 処理番号を取得します。
		/// </summary>
		/// <value>処理番号</value>
		public string VersionCode {
			get;
		}
		/// <summary>
		/// 処理種別を取得します。
		/// </summary>
		/// <value>処理種別</value>
		public string ProcessCode {
			get;
		}
		/// <summary>
		/// 要求引数を取得します。
		/// </summary>
		/// <value>要求引数</value>
		public string RequestPath {
			get;
		}
		/// <summary>
		/// 属性一覧を取得します。
		/// </summary>
		/// <value>属性一覧</value>
		public ElementList ElementList {
			get;
		}
		#endregion プロパティー定義

		#region 生成メソッド定義
		/// <summary>
		/// 要求情報を生成します。
		/// </summary>
		/// <param name="versionCode">処理番号</param>
		/// <param name="processCode">処理種別</param>
		/// <param name="requestPath">要求引数</param>
		/// <param name="elementList">属性一覧</param>
		public RequestData(string versionCode, string processCode, string requestPath, ElementList elementList) {
			VersionCode = versionCode;
			ProcessCode = processCode;
			RequestPath = requestPath;
			ElementList = elementList;
		}
		#endregion 生成メソッド定義

		#region 内部メソッド定義
		/// <summary>
		/// 送信情報を生成します。
		/// </summary>
		/// <param name="source">送信情報</param>
		/// <returns>送信情報</returns>
		private static byte[] CreateData(string source) {
			// TODO ASCII以外は現時点では考慮しない
			var result = new byte[source.Length];
			for (var index = 0; index < result.Length; index ++) {
				result[index] = (byte)source[index];
			}
			return result;
		}
		/// <summary>
		/// 出力情報を出力します。
		/// </summary>
		/// <param name="stream">出力処理</param>
		/// <param name="output">出力情報</param>
		private static void OutputHeader(Stream stream, string output) {
			var source = CreateData(output);
			stream.Write(source, 0, source.Length);
		}
		/// <summary>
		/// 出力情報を出力します。
		/// </summary>
		/// <param name="stream">出力処理</param>
		/// <param name="output">出力情報</param>
		private static void OutputStream(Stream stream, ElementList output) {
			output.OutputData(stream);
		}
		#endregion 内部メソッド定義

		#region 公開メソッド定義
		/// <summary>
		/// 保持情報を出力します。
		/// </summary>
		/// <param name="stream">出力処理</param>
		public void OutputStream(Stream stream) {
			OutputHeader(stream, $"{ProcessCode} {RequestPath} HTTP/{VersionCode}\r\n");
			OutputStream(stream, ElementList);
		}
		#endregion 公開メソッド定義
	}
}
