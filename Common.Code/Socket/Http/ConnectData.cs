using System;

namespace Libraries.Socket.Http {
	/// <summary>
	/// 接続情報クラスです。
	/// </summary>
	public sealed class ConnectData : IEquatable<ConnectData> {
		#region プロパティー定義
		/// <summary>
		/// 接続種別を取得します。
		/// <para>当該返却値が<c>True</c>の場合、SSL通信を行うものとします。</para>
		/// </summary>
		/// <value>接続種別</value>
		public bool SecureFlag {
			get;
		}
		/// <summary>
		/// 接続名称を取得します。
		/// </summary>
		/// <value>接続名称</value>
		public string ServerName {
			get;
		}
		/// <summary>
		/// 接続番号を取得します。
		/// </summary>
		/// <value>接続番号</value>
		public ushort ServerPort {
			get;
		}
		/// <summary>
		/// 接続引数を取得します。
		/// </summary>
		/// <value>接続引数</value>
		public string AccessPath {
			get;
		}
		#endregion プロパティー定義

		#region 生成メソッド定義
		/// <summary>
		/// 接続情報を生成します。
		/// </summary>
		/// <param name="secureFlag">接続種別</param>
		/// <param name="serverName">接続名称</param>
		/// <param name="serverPort">接続番号</param>
		/// <param name="accessPath">接続引数</param>
		public ConnectData(bool secureFlag, string serverName, ushort serverPort, string accessPath) {
			SecureFlag = secureFlag;
			ServerName = serverName;
			ServerPort = serverPort;
			AccessPath = accessPath;
		}
		#endregion 生成メソッド定義

		#region 実装メソッド定義
		/// <summary>
		/// 当該情報と等価であるか判定します。
		/// </summary>
		/// <param name="some">判定情報</param>
		/// <returns>等価である場合、<c>True</c>を返却</returns>
		public bool Equals(ConnectData? some) {
			if (some == null) {
				return false;
			} else {
				return SecureFlag == some.SecureFlag
					&& ServerName == some.ServerName
					&& ServerPort == some.ServerPort
					&& AccessPath == some.AccessPath;
			}
		}
		#endregion 実装メソッド定義

		#region 継承メソッド定義
		/// <summary>
		/// 当該情報と等価であるか判定します。
		/// </summary>
		/// <param name="some">判定情報</param>
		/// <returns>等価である場合、<c>True</c>を返却</returns>
		public override bool Equals(object? some) => Equals(some as ConnectData);
		/// <summary>
		/// 当該情報のハッシュ値を算出します。
		/// </summary>
		/// <returns>ハッシュ値</returns>
		public override int GetHashCode() {
			var source = Tuple.Create(SecureFlag, ServerName, ServerPort, AccessPath);
			return source.GetHashCode();
		}
		/// <summary>
		/// 当該情報を表現文字列へ変換します。
		/// </summary>
		/// <returns>表現文字列</returns>
		public override string ToString() => (SecureFlag? "https://": "http://") + ServerName + ':' + ServerPort + AccessPath;
		#endregion 継承メソッド定義
	}
}
