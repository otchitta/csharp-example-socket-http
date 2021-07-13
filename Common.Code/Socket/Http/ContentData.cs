using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Libraries.Struct;

namespace Libraries.Socket.Http {
	/// <summary>
	/// 内容情報クラスです。
	/// </summary>
	public sealed class ContentData : IReadOnlyList<byte> {
		#region メンバー変数定義
		/// <summary>
		/// 要素配列
		/// </summary>
		private byte[] values;
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
		public byte this[int index] => this.values[index];
		#endregion プロパティー定義

		#region 生成メソッド定義
		/// <summary>
		/// 内容情報を生成します。
		/// </summary>
		/// <param name="values">要素配列</param>
		private ContentData(byte[] values) {
			this.values = values;
		}
		/// <summary>
		/// 内容情報を生成します。
		/// </summary>
		/// <param name="header">属性一覧</param>
		/// <param name="stream">読込処理</param>
		/// <returns>内容情報</returns>
		public static ContentData CreateData(ElementList header, Stream stream) {
			if (header.ChooseText("Transfer-Encoding", out var value1)) {
				var values = ChooseData(header, value1, stream);
				var result = ChooseData(header.ChooseText("Content-Encoding"), values);
				return new ContentData(result);
			} else if (header.ChooseText("Content-Encoding", out var value2)) {
				var values = ChooseData(header, "identity", stream);
				var result = ChooseData(value2, values);
				return new ContentData(result);
			} else {
				var result = ChooseData(header, "identity", stream);
				return new ContentData(result);
			}
		}
		#endregion 生成メソッド定義

		#region 内部メソッド定義(共通関連)
		/// <summary>
		/// 要素情報を抽出します。
		/// </summary>
		/// <param name="stream">読込処理</param>
		/// <returns>要素情報</returns>
		/// <exception cref="StructException">読込途中で読込終端に達した場合</exception>
		private static string ChooseText(Stream stream) {
			var result = new StringBuilder();
			var before = 0;
			while (true) {
				var choose = stream.ReadByte();
				if (choose < 0) {
					// 読込終端である場合：例外発行
					throw new StructException("Ended stream.");
				} else if (before == '\r' && choose == '\n') {
					// 情報終了である場合：情報返却
					return result.ToString(0, result.Length - 1);
				} else {
					// 上記以外である場合：情報追加
					result.Append((char)choose);
					before = choose;
				}
			}
		}
		/// <summary>
		/// 要素情報を抽出します。
		/// </summary>
		/// <param name="stream">読込処理</param>
		/// <returns>要素情報</returns>
		/// <exception cref="StructException">読込途中で読込終端に達した場合</exception>
		/// <exception cref="StructException">読込情報の形式が正しくない場合</exception>
		private static int ChooseInt4(Stream stream) {
			var choose = ChooseText(stream);
			if (Int32.TryParse(choose, NumberStyles.AllowHexSpecifier, null, out var result)) {
				return result;
			} else {
				throw new StructException("Invalid chuncked length." + Environment.NewLine + "choose=" + choose);
			}
		}
		/// <summary>
		/// 固定情報を抽出します。
		/// </summary>
		/// <param name="stream">読込処理</param>
		/// <param name="length">読込個数</param>
		/// <returns>読込情報</returns>
		/// <exception cref="StructException">読込途中で読込終端に達した場合</exception>
		private static byte[] ChooseData(Stream stream, int length) {
			if (length == 0) {
				return new byte[0];
			} else {
				var result = new byte[length];
				var offset = 0;
				while (true) {
					var choose = stream.Read(result, offset, length - offset);
					if (choose <= 0) {
						// 読込終端の場合：例外発行(読込個数異常)
						throw new StructException("Ended stream.");
					} else if (length <= offset + choose) {
						// 読込終了の場合：情報返却
						return result;
					} else {
						// 読込途中の場合：開始移動
						offset += choose;
					}
				}
			}
		}
		/// <summary>
		/// 分割情報を抽出します。
		/// </summary>
		/// <param name="stream">読込処理</param>
		/// <returns>読込情報</returns>
		/// <exception cref="StructException">読込途中で読込終端に達した場合</exception>
		/// <exception cref="StructException">読込情報の形式が正しくない場合</exception>
		private static byte[] ChooseData(Stream stream) {
			using (var buffer = new MemoryStream()) {
				while (true) {
					var length = ChooseInt4(stream);
					var values = ChooseData(stream, length);
					var suffix = ChooseText(stream);
					if (!String.IsNullOrEmpty(suffix)) {
						// 終端異常である場合：例外発行
						throw new StructException("Not ended chuncked data.");
					} else if (length == 0) {
						// 読込終端である場合：情報返却
						buffer.Flush();
						return buffer.ToArray();
					} else {
						// 読込途中である場合：情報追加
						buffer.Write(values, 0, values.Length);
					}
				}
			}
		}
		#endregion 内部メソッド定義(共通関連)

		#region 内部メソッド定義(転送関連)
		/// <summary>
		/// 転送情報を読込みます。
		/// <para><paramref name="format" />は以下の形式に対応する。
		///   <para>chunked</para>
		///   <para>deflate</para>
		///   <para>gzip</para>
		///   <para>identity</para>
		///   <para>※「compress」も存在するが特許問題で非対応とする</para>
		/// </para>
		/// </summary>
		/// <param name="header">定義一覧</param>
		/// <param name="format">転送種別</param>
		/// <param name="reader">読込処理</param>
		/// <returns>読込情報</returns>
		/// <exception cref="StructException">読込途中で読込終端に達した場合</exception>
		/// <exception cref="StructException">読込情報の形式が正しくない場合</exception>
		/// <exception cref="StructException">転送種別が対応外である場合</exception>
		private static byte[] ChooseData(ElementList header, string format, Stream reader) {
			switch (format) {
				default:         throw new StructException("Unsupported transfer encoding." + Environment.NewLine + "encoding=" + format);
				case "identity": return ChooseData(reader, header.ChooseInt4("Content-Length"));
				case "chunked":  return ChooseData(reader);
				case "deflate":  return Struct.Rfc1951.Rfc1951.Decode(ChooseData(reader, header.ChooseInt4("Content-Length")));
				case "gzip":     return Struct.Rfc1952.Rfc1952.Decode(ChooseData(reader, header.ChooseInt4("Content-Length")));
			}
		}
		#endregion 内部メソッド定義(転送関連)

		#region 内部メソッド定義(内容関連)
		/// <summary>
		/// 内容情報へ変換します。
		/// </summary>
		/// <param name="format">変換種別</param>
		/// <param name="values">読込情報</param>
		/// <returns>内容情報</returns>
		/// <exception cref="StructException">変換種別が対応外である場合</exception>
		private static byte[] ChooseData(string format, byte[] values) {
			switch (format) {
				default:         throw new StructException("Unsupported content encoding." + Environment.NewLine + "encoding=" + format);
				case "identity": return values;
				case "deflate":  return Struct.Rfc1951.Rfc1951.Decode(values);
				case "gzip":     return Struct.Rfc1952.Rfc1952.Decode(values);
				case "br":       return Struct.Rfc7932.Rfc7932.Decode(values);
			}
		}
		#endregion 内部メソッド定義(内容関連)

		#region 公開メソッド定義
		/// <summary>
		/// 要素処理を生成します。
		/// </summary>
		/// <returns>要素処理</returns>
		public Stream CreateStream() {
			return new MemoryStream(this.values);
		}
		/// <summary>
		/// 保持情報を出力します。
		/// </summary>
		/// <param name="writer">出力処理</param>
		public void OutputWriter(TextWriter writer) {
			for (var index1 = 0; index1 < this.values.Length; index1 += 16) {
				writer.Write("{0:X4} : ", index1);
				for (var index2 = 0; index2 < 16; index2 ++) {
					if (index2 != 0) writer.Write(' ');
					if (index1 + index2 < this.values.Length) {
						writer.Write("{0:X2}", this.values[index1 + index2]);
					} else {
						writer.Write("  ");
					}
				}
				writer.Write(" - ");
				for (var index2 = 0; index2 < 16; index2 ++) {
					if (index2 != 0) writer.Write(' ');
					if (index1 + index2 < this.values.Length) {
						var choose = this.values[index1 + index2];
						writer.Write((choose < 0x20 || 0x7E < choose)? '.': (char)choose);
					} else {
						writer.Write(' ');
					}
				}
			}
			writer.Flush();
		}
		#endregion 公開メソッド定義

		#region 実装メソッド定義
		/// <summary>
		/// 反覆処理を取得します。
		/// </summary>
		/// <returns>反覆処理</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			foreach (var choose in this.values) {
				yield return choose;
			}
		}
		/// <summary>
		/// 反覆処理を取得します。
		/// </summary>
		/// <returns>反覆処理</returns>
		IEnumerator<byte> IEnumerable<byte>.GetEnumerator() {
			foreach (var choose in this.values) {
				yield return choose;
			}
		}
		#endregion 実装メソッド定義
	}
}
