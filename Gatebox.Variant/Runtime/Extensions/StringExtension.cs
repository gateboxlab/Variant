using System;

namespace Gatebox.Variant.Extensions
{
	public static class StringExtension
	{
		/// <summary>
		/// string からその string 内の開始位置と終了位置を指定して StringView を作って返す
		/// </summary>
		public static StringView Slice(this string s, int begin, int end = -1)
		{
			if (end < 0)
			{
				return new StringView(s, begin..);
			}

			return new StringView(s, begin..end);
		}

		/// <summary>
		/// string からその string 内の開始位置と長さを指定して StringView を作って返す
		/// </summary>
		public static StringView SubView(this string s, int begin, int length = -1)
		{
			int end = begin + length;
			if (length < 0)
			{
				end = s.Length;
			}
			return new StringView(s, begin, end);
		}


		/// <summary>
		/// string 全体を示す StringView を返す。
		/// </summary>
		public static StringView View(this string s)
		{
			return new StringView(s);
		}


		/// <summary>
		/// string から Range を指定してその範囲を示す StringView を返す
		/// </summary>
		public static StringView View(this string s, Range range)
		{
			return new StringView(s, range);
		}


		/// <summary>
		/// 文字列が空文字列であるか空白のみからなるとき true.
		/// <para>
		/// ここでいう「空白」は、いわゆる半角空白と、タブ、改行です。全角空白や U+200X 系空白などは blank ではないと判断されます。</para>
		/// </summary>
		public static bool IsBlank(this string s)
		{
			return s.View().IsBlank();
		}

		/// <summary>
		/// 文字列が空文字列であるか null であるとき true.
		/// <para>
		/// string.IsNullOrEmpty(s) と同じ</para>
		/// </summary>
		public static bool IsEmpty(this string s)
		{
			return string.IsNullOrEmpty(s);
		}

		/// <summary>
		/// 内容があるとき true.
		/// <para>
		/// ! string.IsNullOrEmpty(s) と同じ。</para>
		/// </summary>
		public static bool HasContent(this string s)
		{
			return !string.IsNullOrEmpty(s);
		}

		/// <summary>
		/// null もしくは空文字列であるとき第二引数を、そうでないときはそのまま返す。
		/// </summary>
		public static string OrDefault(this string s, string def)
		{
			if (string.IsNullOrEmpty(s))
			{
				return def;
			}
			return s;
		}

		/// <summary>
		/// 末尾が ch でないとき、ch を追加して返す。
		/// </summary>
		public static string EnsureTrail(this string s, char ch)
		{
			if (string.IsNullOrEmpty(s))
			{
				return "" + ch;
			}
			if (s[^1] == ch)
			{
				return s;
			}
			return s + ch;
		}

		/// <summary>
		/// 末尾が '/' でないとき、'/' を追加して返す。
		/// </summary>
		public static string EnsureTrailingSlash(this string s)
		{
			return s.EnsureTrail('/');
		}
	}
}

