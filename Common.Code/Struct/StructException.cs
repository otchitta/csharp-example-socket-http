using System;

namespace Libraries.Struct {
	/// <summary>
	/// 定義例外クラスです。
	/// </summary>
	public class StructException : Exception {
		/// <summary>
		/// 定義例外を生成します。
		/// </summary>
		/// <param name="reason">例外理由</param>
		public StructException(string reason) : base(reason) {
			// 処理なし
		}
	}
}
