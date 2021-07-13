using System.IO;
using Libraries.Struct;

namespace Libraries.Socket.Http {
	/// <summary>
	/// 受信情報クラスです。
	/// </summary>
	public sealed class ReceiveData {
		#region プロパティー定義
		/// <summary>
		/// 状態内容を取得します。
		/// </summary>
		/// <value>状態内容</value>
		public string ReceiveText {
			get;
		}
		/// <summary>
		/// 構成一覧を取得します。
		/// </summary>
		/// <value>構成一覧</value>
		public ElementList ElementList {
			get;
		}
		/// <summary>
		/// 内容情報を取得します。
		/// </summary>
		/// <value>内容情報</value>
		public ContentData ContentData {
			get;
		}
		#endregion プロパティー定義

		#region 生成メソッド定義
		/// <summary>
		/// 受信情報を生成します。
		/// </summary>
		/// <param name="receiveText">状態内容</param>
		/// <param name="elementList">構成一覧</param>
		/// <param name="contentData">内容情報</param>
		private ReceiveData(string receiveText, ElementList elementList, ContentData contentData) {
			ReceiveText = receiveText;
			ElementList = elementList;
			ContentData = contentData;
		}
		/// <summary>
		/// 受信情報を生成します。
		/// </summary>
		/// <param name="stream">読込処理</param>
		/// <returns>受信情報</returns>
		/// <exception cref="StructException">読込途中で読込終端に達した場合</exception>
		public static ReceiveData CreateData(Stream stream) {
			var receiveText = ChooseText(stream);
			var elementList = ElementList.CreateData(stream);
			var contentData = ContentData.CreateData(elementList, stream);
			return new ReceiveData(receiveText, elementList, contentData);
		}
		#endregion 生成メソッド定義

		#region 内部メソッド定義
		/// <summary>
		/// 要素情報を抽出します。
		/// </summary>
		/// <param name="stream">読込処理</param>
		/// <returns>要素情報</returns>
		/// <exception cref="StructException">読込途中で読込終端に達した場合</exception>
		private static string ChooseText(Stream stream) {
			var result = new System.Text.StringBuilder();
			var before = 0;
			while (true) {
				var choose = stream.ReadByte();
				if (choose < 0) {
					throw new StructException("Ended stream.");
				} else if (before == '\r' && choose == '\n') {
					return result.ToString(0, result.Length - 1);
				} else {
					result.Append((char)choose);
					before = choose;
				}
			}
		}
		#endregion 内部メソッド定義
	}
}
