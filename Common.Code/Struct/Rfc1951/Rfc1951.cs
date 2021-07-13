using System.IO;
using System.IO.Compression;

namespace Libraries.Struct.Rfc1951 {
	/// <summary>
	/// RFC1951(DEFLATE Compressed Data Fromat Specification)処理クラスです。
	/// </summary>
	public static class Rfc1951 {
		#region 公開メソッド定義(復号処理)
		/// <summary>
		/// 暗号情報を復号します。
		/// <para><paramref name="reader" />にて読み込んだ復号情報を<paramref name="writer" />へ書き込みます。</para>
		/// </summary>
		/// <param name="writer">書込処理</param>
		/// <param name="reader">読込処理</param>
		/// <exception cref="InvalidDataException">読込情報の形式が正しくない場合</exception>
		public static void Decode(Stream writer, Stream reader) {
			using (var action = new DeflateStream(reader, CompressionMode.Decompress)) {
				var buffer = new byte[4096];
				while (true) {
					var length = action.Read(buffer, 0, buffer.Length);
					if (length <= 0) {
						writer.Flush();
						break;
					} else {
						writer.Write(buffer, 0, length);
					}
				}
			}
		}
		/// <summary>
		/// 暗号情報を復号します。
		/// </summary>
		/// <param name="reader">読込処理</param>
		/// <returns>復号情報</returns>
		public static byte[] Decode(Stream reader) {
			using (var action = new MemoryStream()) {
				Decode(action, reader);
				return action.ToArray();
			}
		}
		/// <summary>
		/// 暗号情報を復号します。
		/// </summary>
		/// <param name="reader">読込情報</param>
		/// <returns>復号情報</returns>
		public static byte[] Decode(byte[] values) {
			using (var action = new MemoryStream(values)) {
				return Decode(action);
			}
		}
		#endregion 公開メソッド定義(復号処理)
	}
}
