using System.Collections.Generic;
using System.Text;

namespace Gatebox.Variant.Internal
{
	/// <summary>
	/// ローカルで利用する StringBuilder のプール。
	/// </summary>
	internal static class StringBuilderPool
	{
		private static readonly List<StringBuilder> s_List = new();

		/// <summary>
		/// StringBuilder を返す。
		/// <para>
		/// 不要になったら Return してください。
		/// （別にしなくてもいいですけど、Return すると再利用されます。）</para>
		/// </summary>
		public static StringBuilder Rent()
		{
			lock (s_List)
			{
				if (s_List.Count == 0)
				{
					return new StringBuilder();
				}

				var r = s_List[0];
				s_List.RemoveAt(0);
				return r;
			}
		}

		public static StringBuilder Rent( string s, int start = 0, int length = -1)
		{
			if( length <= -1)
			{
				length = s.Length - start;
			}

			if( length <= 0)
			{
				return Rent();
			}

			lock (s_List)
			{
				if (s_List.Count == 0)
				{
					return new StringBuilder(s, start, length, length);
				}

				var r = s_List[0];
				s_List.RemoveAt(0);
				r.Append(s, start, length);
				return r;
			}
		}

		/// <summary>
		/// 返却
		/// </summary>
		public static string Return(StringBuilder sb)
		{
			if (sb == null)
			{
				return "";
			}

			string ret = sb.ToString();
			sb.Clear();

			lock (s_List)
			{
				// 16 という数字に意味はないが、あんまり溜まっているのもおかしいと考える。
				if (s_List.Count < 16)
				{
					s_List.Add(sb);
				}
			}
			return ret;
		}
	}
}
