using System;

namespace Libraries.Socket.Http {
	/// <summary>
	/// 要素情報クラスです。
	/// </summary>
	public sealed class ElementData : IEquatable<ElementData> {
		#region プロパティー定義
		/// <summary>
		/// 要素名称を取得します。
		/// </summary>
		/// <value>要素名称</value>
		public string Name {
			get;
		}
		/// <summary>
		/// 要素内容を取得します。
		/// </summary>
		/// <value>要素内容</value>
		public string Text {
			get;
		}
		#endregion プロパティー定義

		#region 生成メソッド定義
		/// <summary>
		/// 要素情報を生成します。
		/// </summary>
		/// <param name="name">要素名称</param>
		/// <param name="text">要素内容</param>
		public ElementData(string name, string text) {
			Name = name;
			Text = text;
		}
		#endregion 生成メソッド定義

		#region 分解メソッド定義
		/// <summary>
		/// 要素情報を出力します。
		/// </summary>
		/// <param name="name">要素名称</param>
		/// <param name="text">要素内容</param>
		public void Deconstruct(out string name, out string text) {
			name = Name;
			text = Text;
		}
		#endregion 分解メソッド定義

		#region 実装メソッド定義
		/// <summary>
		/// 当該情報と等価であるか判定します。
		/// </summary>
		/// <param name="some">判定情報</param>
		/// <returns>等価である場合、<c>True</c>を返却</returns>
		public bool Equals(ElementData? some) {
			if (some == null) {
				return false;
			} else {
				return Name == some.Name
					&& Text == some.Text;
			}
		}
		#endregion 実装メソッド定義

		#region 継承メソッド定義
		/// <summary>
		/// 当該情報と等価であるか判定します。
		/// </summary>
		/// <param name="some">判定情報</param>
		/// <returns>等価である場合、<c>True</c>を返却</returns>
		public override bool Equals(object? some) => Equals(some as ElementData);
		/// <summary>
		/// 当該情報のハッシュ値を算出します。
		/// </summary>
		/// <returns>ハッシュ値</returns>
		public override int GetHashCode() {
			var source = Tuple.Create(Name, Text);
			return source.GetHashCode();
		}
		/// <summary>
		/// 当該情報を表現文字列へ変換します。
		/// </summary>
		/// <returns>表現文字列</returns>
		public override string ToString() => $"{Name}={Text}";
		#endregion 継承メソッド定義
	}
}
