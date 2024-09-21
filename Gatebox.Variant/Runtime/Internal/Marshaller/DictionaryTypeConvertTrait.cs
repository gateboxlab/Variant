using System.Collections.Generic;


namespace Gatebox.Variant.Internal
{

	/// <summary>
	/// <see cref="IDictionaty{string, V}"/> 変換するためのトレイト
	/// <para>
	/// <see cref="IDictionaty{string, V}"/> を実装した具体型。
	/// V は IDictionary の Value の型。</para>
	/// <para>
	/// デフォルトコンストラクタを持ち、IDictionary を実装した型はそれを通して JObject と相互変換する。</para>
	/// <para>
	/// VariantConverter に interface による Trait が登録できるわけではなく、<see cref="VariantMarshaller"/> の内部で利用されているものです。
	/// </para>
	/// </summary>
	internal class DictionaryTypeConvertTrait<T, V> : ConvertTrait<T> where T : IDictionary<string, V>, new()
	{

		public override T ConvertVariant(JVariant variant)
		{
			if (!variant.IsObject)
			{
				throw new VariantException($"Unable to convert {variant.VariantType}  to {typeof(T).Name}");
			}

			T obj = new();
			IDictionary<string, V> dict = obj;

			foreach (var pair in variant.AsObject())
			{
				string key = pair.Key;
				V value = pair.Value.As<V>();
				dict.Add(key, value);
			}

			return obj;
		}

		public override JVariant CreateVariant(T v)
		{
			IDictionary<string, V> dict = v;

			JObject obj = new JObject();
			foreach (var pair in dict)
			{
				obj.Add(pair.Key, JVariant.Create(pair.Value));
			}

			return obj;
		}
	}
}
