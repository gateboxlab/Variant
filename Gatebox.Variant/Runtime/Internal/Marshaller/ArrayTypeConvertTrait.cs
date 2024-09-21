using System;

namespace Gatebox.Variant.Internal
{
	/// <summary>
	/// 配列を変換するためのトレイト
	/// </summary>
	internal class ArrayTypeConvertTrait<T> : ConvertTrait<T[]>
	{
		public override T[] ConvertVariant(JVariant variant)
		{
			if (!variant.IsArray)
			{
				throw new VariantException($"Unable to convert {variant.VariantType}  to {typeof(T[]).Name}");
			}

			T[] obj = new T[variant.Count];
			for (int i = 0; i < variant.Count; i++)
			{
				obj[i] = variant[i].As<T>();
			}

			return obj;
		}

		public override JVariant CreateVariant(T[] v)
		{
			JArray array = new JArray();
			foreach (var item in v)
			{
				array.Add(JVariant.Create(item));
			}
			return array;
		}
	}
}
