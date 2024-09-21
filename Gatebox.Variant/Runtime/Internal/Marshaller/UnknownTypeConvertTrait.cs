using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace Gatebox.Variant.Internal
{
	internal class UnknownTypeConvertTrait<T> : ConvertTrait<T>
	{
		private readonly List<(string Name, FieldInfo Field)> m_Fields = new();


		public UnknownTypeConvertTrait()
		{
			Type t = typeof(T);

			// 継承ツリーを遡りながらインスタンスフィールドを列挙
			while (t != null)
			{
				foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
				{
					string name = field.Name;

					// バッキングフィールドの場合はプロパティ名に変換
					if (IsPropertyBackingField(field))
					{
						name = GetFrontPropertyName(field);
					}

					m_Fields.Add((name, field));
				}

				t = t.BaseType;
			}
		}

		public override T ConvertVariant(JVariant variant)
		{
			if (!variant.IsObject)
			{
				throw new VariantException($"Unable to convert {variant.VariantType}  to {typeof(T).Name}");
			}

			T v = Activator.CreateInstance<T>();

			foreach (var (name, field) in m_Fields)
			{
				var x = variant.Get(name);
				if (x.IsNull())
				{
					continue;
				}
				object value = x.ConvertTo(field.FieldType, throws: true);
				field.SetValue(v, value);
			}

			return v;
		}

		public override JVariant CreateVariant(T v)
		{
			var obj = new JObject();
			foreach (var (name, field) in m_Fields)
			{
				object value = field.GetValue(v);
				obj[name] = JVariant.Create(value);
			}

			return obj;
		}


		// プロパティバッキングフィールドかどうか
		private static bool IsPropertyBackingField(FieldInfo f)
		{
			// この根拠は見つけられなかったが、実際 <NAME>k__BackingField という名前で来るのでこれで判定
			if (f.Name.StartsWith("<"))
			{
				// コンパイラが生成したものかどうか一応判定
				return f.IsDefined(typeof(CompilerGeneratedAttribute), false);
			}
			return false;
		}

		// プロパティバッキングフィールドだとして、そのフロントプロパティ名を取得
		public static string GetFrontPropertyName(FieldInfo f)
		{
			int open = f.Name.IndexOf('<');
			int close = f.Name.IndexOf('>');
			if (open < 0 || close < 0 || open >= close)
			{
				return f.Name;
			}
			return f.Name.Substring(open + 1, close - open - 1);
		}
	}

}
