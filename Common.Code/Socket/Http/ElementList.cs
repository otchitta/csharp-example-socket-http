using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Libraries.Struct;

namespace Libraries.Socket.Http {
	/// <summary>
	/// 要素一覧クラスです。
	/// </summary>
	public sealed class ElementList : IReadOnlyList<ElementData>, IEquatable<ElementList> {
		#region メンバー変数定義
		/// <summary>
		/// 要素一覧
		/// </summary>
		private readonly ElementData[] values;
		#endregion メンバー変数定義

		#region プロパティー定義
		/// <summary>
		/// 要素個数を取得します。
		/// </summary>
		/// <value>要素個数</value>
		public int Count => this.values.Length;
		/// <summary>
		/// 要素情報を取得します。
		/// </summary>
		/// <param name="index">要素番号</param>
		/// <value>要素情報</value>
		/// <exception cref="System.IndexOutOfRangeException"><paramref name="index" />が「0」から「<see cref="Count" /> - 1」までの範囲ではない場合</exception>
		public ElementData this[int index] => this.values[index];
		#endregion プロパティー定義

		#region 生成メソッド定義
		/// <summary>
		/// 要素一覧を生成します。
		/// </summary>
		/// <param name="values">要素配列</param>
		private ElementList(ElementData[] values) {
			this.values = values;
		}
		/// <summary>
		/// 要素一覧を生成します。
		/// </summary>
		/// <param name="values">要素集合</param>
		public ElementList(IEnumerable<ElementData> values) {
			this.values = CreateList(values);
		}
		/// <summary>
		/// 要素一覧を生成します。
		/// </summary>
		/// <param name="stream">読込処理</param>
		/// <returns>要素一覧</returns>
		/// <exception cref="StructException">読込途中で読込終端に達した場合</exception>
		/// <exception cref="StructException">読込情報の形式が正しくない場合</exception>
		public static ElementList CreateData(Stream stream) {
			return new ElementList(CreateList(stream));
		}
		#endregion 生成メソッド定義

		#region 内部メソッド定義(生成関連)
		/// <summary>
		/// 要素配列へ変換します。
		/// </summary>
		/// <param name="values">要素集合</param>
		/// <returns>要素配列</returns>
		private static ElementData[] CreateList(IEnumerable<ElementData> values) {
			var result = new List<ElementData>(values);
			return result.ToArray();
		}
		/// <summary>
		/// 要素情報を抽出します。
		/// </summary>
		/// <param name="stream">読込処理</param>
		/// <returns>要素情報</returns>
		/// <exception cref="StructException">読込途中で読込終端に達した場合</exception>
		private static string CreateText(Stream stream) {
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
		/// <summary>
		/// 要素情報を生成します。
		/// </summary>
		/// <param name="source">読込情報</param>
		/// <param name="result">要素情報</param>
		/// <returns>要素情報の形式が正しい場合、<c>True</c>を返却</returns>
		private static bool CreateData(string source, [MaybeNullWhen(false)]out ElementData result) {
			var offset = source.IndexOf(": ");
			if (offset < 0) {
				result = default;
				return false;
			} else {
				var value1 = source.Substring(0, offset);
				var value2 = source.Substring(offset + 2);
				result = new ElementData(value1, value2);
				return true;
			}
		}
		/// <summary>
		/// 要素配列を生成します。
		/// </summary>
		/// <param name="stream">読込処理</param>
		/// <returns>要素配列</returns>
		/// <exception cref="StructException">読込途中で読込終端に達した場合</exception>
		/// <exception cref="StructException">読込情報の形式が正しくない場合</exception>
		private static ElementData[] CreateList(Stream stream) {
			var result = new List<ElementData>();
			while (true) {
				var choose = CreateText(stream);
				if (String.IsNullOrEmpty(choose)) {
					// 読込終了である場合：情報返却
					return result.ToArray();
				} else if (!CreateData(choose, out var output)) {
					// 形式不正である場合：例外発行
					throw new StructException("Illegal element format." + Environment.NewLine + "source=" + choose);
				} else {
					// 上記以外である場合：情報追加
					result.Add(output);
				}
			}
		}
		/// <summary>
		/// 出力情報を生成します。
		/// </summary>
		/// <param name="source">出力情報</param>
		/// <returns>出力情報</returns>
		private static byte[] CreateData(string source) {
			// TODO ASCII以外は現時点では考慮しない
			var result = new byte[source.Length];
			for (var index = 0; index < result.Length; index ++) {
				result[index] = (byte)source[index];
			}
			return result;
		}
		/// <summary>
		/// 出力情報を生成します。
		/// </summary>
		/// <param name="source">出力情報</param>
		/// <returns>出力情報</returns>
		private static byte[] CreateData(ElementData source) => CreateData($"{source.Name}: {source.Text}\r\n");
		#endregion 内部メソッド定義(生成関連)

		#region 内部メソッド定義(継承関連)
		/// <summary>
		/// 引数情報が等価であるか判定します。
		/// </summary>
		/// <param name="item1">判定情報</param>
		/// <param name="item2">判定情報</param>
		/// <returns>等価である場合、<c>True</c>を返却</returns>
		private static bool Equals(ElementData[] item1, ElementData[] item2) {
			if (item1 == null) {
				return item2 == null;
			} else if (item2 == null) {
				return false;
			} else if (item1.Length != item2.Length) {
				return false;
			} else {
				var count = item1.Length;
				for (var index = 0; index < count; index ++) {
					if (ElementData.Equals(item1, item2) == false) {
						return false;
					}
				}
				return true;
			}
		}
		/// <summary>
		/// 引数情報のハッシュ値を算出します。
		/// </summary>
		/// <param name="source">算出情報</param>
		/// <returns>ハッシュ値</returns>
		private static int GetHashCode(object? source) => source?.GetHashCode() ?? 0;
		#endregion 内部メソッド定義(継承関連)

		#region 公開メソッド定義
		/// <summary>
		/// 要素情報を抽出します。
		/// </summary>
		/// <param name="name">検索名称</param>
		/// <param name="data">要素情報</param>
		/// <returns><paramref name="name" />に該当する情報が存在した場合、<c>True</c>を返却</returns>
		public bool ChooseData(string name, [MaybeNullWhen(false)]out ElementData data) {
			for (var index = 0; index < this.values.Length; index ++) {
				var choose = this.values[index];
				if (String.Equals(name, choose.Name)) {
					data = choose;
					return true;
				}
			}
			data = default;
			return false;
		}
		/// <summary>
		/// 要素内容を抽出します。
		/// </summary>
		/// <param name="name">検索名称</param>
		/// <param name="text">要素内容</param>
		/// <returns><paramref name="name" />に該当する情報が存在した場合、<c>True</c>を返却</returns>
		public bool ChooseText(string name, [MaybeNullWhen(false)]out string text) {
			if (ChooseData(name, out var data)) {
				text = data.Text;
				return true;
			} else {
				text = default;
				return false;
			}
		}
		/// <summary>
		/// 要素情報を抽出します。
		/// </summary>
		/// <param name="name">検索名称</param>
		/// <returns>要素情報</returns>
		/// <exception cref="StructException">要素名称が存在しない場合</exception>
		public string ChooseText(string name) {
			if (ChooseText(name, out var text)) {
				return text;
			} else {
				throw new StructException("name is not found." + Environment.NewLine + "name=" + name);
			}
		}
		/// <summary>
		/// 要素情報を抽出します。
		/// </summary>
		/// <param name="name">検索名称</param>
		/// <returns>要素情報</returns>
		/// <exception cref="StructException">要素名称が存在しない場合</exception>
		/// <exception cref="StructException">要素内容の形式が正しくない場合</exception>
		public int ChooseInt4(string name) {
			var text = ChooseText(name);
			if (Int32.TryParse(text, out var data)) {
				return data;
			} else {
				throw new StructException("Illegal int32 format." + Environment.NewLine + "name=" + name + Environment.NewLine + "text=" + text);
			}
		}
		/// <summary>
		/// 保持情報を出力します。
		/// </summary>
		/// <param name="stream">出力処理</param>
		/// <exception cref="NotSupportedException"><paramref name="stream" />が出力操作に対応していない場合</exception>
		public void OutputData(Stream stream) {
			foreach (var choose in this.values) {
				var values = CreateData(choose);
				stream.Write(values, 0, values.Length);
			}
			stream.Write(CreateData("\r\n"), 0, 2);
		}
		#endregion 公開メソッド定義

		#region 実装メソッド定義
		/// <summary>
		/// 反復処理を取得します。
		/// </summary>
		/// <returns>反復処理</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			foreach (var choose in this.values) {
				yield return choose;
			}
		}
		/// <summary>
		/// 反復処理を取得します。
		/// </summary>
		/// <returns>反復処理</returns>
		IEnumerator<ElementData> IEnumerable<ElementData>.GetEnumerator() {
			foreach (var choose in this.values) {
				yield return choose;
			}
		}
		/// <summary>
		/// 当該情報と等価であるか判定します。
		/// </summary>
		/// <param name="some">判定情報</param>
		/// <returns>等価である場合、<c>True</c>を返却</returns>
		public bool Equals(ElementList? some) => some == null? false: Equals(this.values, some.values);
		#endregion 実装メソッド定義

		#region 継承メソッド定義
		/// <summary>
		/// 当該情報と等価であるか判定します。
		/// </summary>
		/// <param name="some">判定情報</param>
		/// <returns>等価である場合、<c>True</c>を返却</returns>
		public override bool Equals(object? some) => Equals(some as ElementList);
		/// <summary>
		/// 当該情報のハッシュ値を算出します。
		/// </summary>
		/// <returns>ハッシュ値</returns>
		public override int GetHashCode() {
			var result = 1;
			foreach (var choose in this.values) {
				result = result * 31 + GetHashCode(choose);
			}
			return result;
		}
		/// <summary>
		/// 当該情報を表現文字列へ変換します。
		/// </summary>
		/// <returns>表現文字列</returns>
		public override string ToString() {
			var result = new System.Text.StringBuilder();
			var prefix = "";
			result.Append('[');
			for (var index = 0; index < this.values.Length; index ++) {
				var choose = this.values[index];
				result.Append(prefix);
				result.Append(choose);
				prefix = ", ";
			}
			result.Append(']');
			return result.ToString();
		}
		#endregion 継承メソッド定義
	}
}
