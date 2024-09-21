using System.Collections.Generic;


namespace Gatebox.Variant.Internal
{

	/// <summary>
	/// <see cref="ICollection{ V}"/> 変換するためのトレイト
	/// <para>
	/// <see cref="ICollection{V}"/> を実装した具体型。
	/// <para>
	/// デフォルトコンストラクタを持ち、ICollection を実装した型はそれを通して JArray と相互変換する。</para>
	/// <para>
	/// VariantConverter に interface による Trait が登録できるわけではなく、<see cref="VariantMarshaller"/> の内部で利用されているものです。
	/// </para>
	/// </summary>
	internal class CollectionTypeConvertTrait<T, V> : ConvertTrait<T> where T : ICollection<V>, new()
	{
		public override T ConvertVariant(JVariant variant)
		{
			if (!variant.IsArray)
			{
				throw new VariantException($"Unable to convert {variant.VariantType}  to {typeof(T).Name}");
			}

			T obj = new();
			ICollection<V> list = obj;

			foreach (var item in variant.AsArray())
			{
				V value = item.As<V>();
				list.Add(value);
			}

			return obj;
		}

		public override JVariant CreateVariant(T v)
		{
			ICollection<V> list = v;

			JArray array = new JArray();
			foreach (var item in list)
			{
				array.Add(JVariant.Create(item));
			}
			return array;
		}
	}

}
