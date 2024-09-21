using System;
using System.Buffers;
using System.Collections.Generic;


namespace Gatebox.Variant
{

	/// <summary>
	/// <see cref="StringCache"/> の実装用基底クラス。
	/// </summary>
	public abstract class BaseStringCache : IStringCache
	{

		private readonly int m_MaxLength;
		private readonly bool m_IsToShrink;
		private Dictionary<StringView, string> m_StringMap;
		private Dictionary<U8View, string> m_U8Map;

		protected BaseStringCache(int maxLength, bool isToShrink)
		{
			m_MaxLength = maxLength;
			m_IsToShrink = isToShrink;
		}


		public void Dispose()
		{
			if (m_IsToShrink)
			{
				foreach (var p in m_U8Map)
				{
					ArrayPool<byte>.Shared.Return(p.Key.Original);
				}
				m_U8Map.Clear();
			}
		}

		public void SetString(StringView view, string value)
		{
			// 長過ぎる
			if (view.Length > m_MaxLength)
			{
				return;
			}

			if (m_IsToShrink)
			{
				view = view.Shrink();
			}

			Lock(() =>
			{
				m_StringMap ??= new();
				m_StringMap.Add(view, value);
				return null;
			});
		}


		public string GetString(StringView view)
		{
			if (view.IsEmpty())
			{
				return string.Empty;
			}

			// 長過ぎる
			if (view.Length > m_MaxLength)
			{
				return view.ToString();
			}

			return Lock(() =>
				{
					m_StringMap ??= new();

					// あればそれを返す。
					if (m_StringMap.TryGetValue(view, out string ret))
					{
						return ret;
					}

					// 部分文字列を作って、view 自身もそれを参照するようにする
					ret = view.ToString();
					view = ret;

					// キャッシュしつつ返す。
					m_StringMap[view] = ret;
					return ret;
				}
			);
		}

		public string TryGetString(StringView view)
		{
			return Lock(() => m_StringMap?.GetValueOrDefault(view));
		}

		public void SetString(U8View view, string value)
		{
			// 長過ぎる
			if (view.Length > m_MaxLength)
			{
				return;
			}

			if (m_IsToShrink)
			{
				byte[] newBytes = ArrayPool<byte>.Shared.Rent(view.Length);
				Array.Copy(view.Original, view.Begin, newBytes, 0, view.Length);
				view = new U8View(newBytes, 0, view.Length);
			}

			Lock(() =>
			{
				m_U8Map ??= new();
				m_U8Map.Add(view, value);
				return null;
			});
		}

		public string GetString(U8View view)
		{
			if (view.IsEmpty())
			{
				return string.Empty;
			}

			// 長すぎるのはキャッシュしない。
			if (view.Length > m_MaxLength)
			{
				return view.ToString();
			}

			return Lock(() =>
				{
					m_U8Map ??= new();

					//  キャッシュされていればそれを返す。
					if (m_U8Map.TryGetValue(view, out var result))
					{
						return result;
					}

					if (m_IsToShrink)
					{
						byte[] newBytes = ArrayPool<byte>.Shared.Rent(view.Length);
						Array.Copy(view.Original, view.Begin, newBytes, 0, view.Length);
						view = new U8View(newBytes, 0, view.Length);
					}

					// キャッシュにないので追加しつつ返す。
					var str = view.ToString();
					m_U8Map.Add(view, str);

					return str;
				}
			);
		}

		public string TryGetString(U8View view)
		{
			return Lock(() => m_U8Map?.GetValueOrDefault(view));
		}

		protected abstract string Lock(Func<string> action);
	}



	/// <summary>
	/// IStringCache の実装
	/// </summary>
	public class StringCache : BaseStringCache
	{
		/// <summary>
		/// ローカル利用のインスタンスを生成して返す。
		/// </summary>
		public static IStringCache CreateTemporary(int maxLength = 32)
		{
			return new TemporaryStringCache(maxLength);
		}

		public StringCache(int maxLength = 32, bool isToShrink = true) : base(maxLength, isToShrink)
		{
		}

		protected override string Lock(Func<string> action)
		{
			lock (m_Lock)
			{
				return action();
			}
		}

		private object m_Lock = new();
	}


	/// <summary>
	/// IStringCache の実装。
	/// <para>
	/// ローカルで利用することを想定しています。 </para>
	/// </summary>
	public class TemporaryStringCache : BaseStringCache
	{

		public TemporaryStringCache(int maxLength = 32, bool isToShrink = false) : base(maxLength, isToShrink)
		{
		}

		protected override string Lock(Func<string> action)
		{
			return action();
		}
	}

}
