using System;
using System.Reflection;


namespace Gatebox.Variant.Internal
{
	/// <summary>
	/// IJVariantConvertible からの変換を行うトレイト
	/// <para>
	/// VariantConverter に interface による Trait が登録できるわけではなく、
	/// <see cref="VariantMarshaller"/> の内部で利用されているものです。</para>
	/// </summary>
	internal class JVariantConvertibleTrait<T> : ConvertTrait<IJVariantConvertible>
	{
		private readonly ConstructorInfo m_Constructor;

		public JVariantConvertibleTrait()
		{
			m_Constructor = typeof(T).GetConstructor(new Type[] { typeof(JVariantTag) });
		}

		public override IJVariantConvertible ConvertVariant(JVariant variant)
		{
			if (m_Constructor == null)
			{
				throw new VariantException($"{typeof(T)} requires a constructor that accepts a {nameof(JVariantTag)}.");
			}

			return (IJVariantConvertible)m_Constructor.Invoke(new object[] { new JVariantTag(variant) });
		}

		public override JVariant CreateVariant(IJVariantConvertible v)
		{
			return v.AsVariant();
		}
	}
}
