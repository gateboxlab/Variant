using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;



namespace Gatebox.Variant.Internal
{

	/// <summary>
	/// JVaraint と他の方との変換を扱うトレイトを扱うもの。
	/// <para>
	/// 完全に VariantConverter の下請けです。外部からの利用を想定していません。</para>
	/// <para>
	/// VariantConverter が Trait の型をあつかい、このクラスは Trait のインスタンスを扱います。</para>
	/// </summary>
	internal class VariantMarshaller
	{

		//==============================================================================
		// instance members
		//==============================================================================

		private readonly VariantConverter m_Converter;
		private readonly object m_Lock = new object();
		private readonly Dictionary<Type, ConvertTrait> m_Traits = new();

		internal VariantMarshaller(VariantConverter converter)
		{
			m_Converter = converter;
		}

		/// <summary>
		/// キャッシュをクリアする。
		/// </summary>
		public void Clear()
		{
			lock (m_Lock)
			{
				m_Traits.Clear();
			}
		}

		/// <summary>
		/// 任意型を JVariant に変換する。
		/// <para>
		/// v は プリミティブや string, JVariant に関連する型ではない前提です</para>
		/// </summary>
		public JVariant Marshall(object v)
		{
			var type = v.GetType();

			var trait = GetTrait(type);
			if (trait == null)
			{
				throw new VariantException($"Unable to convert {type.Name} to JVariant.");
			}
			return trait.ToVariant(v);
		}

		public T Unmarshall<T>(JVariant variant)
		{
			return (T)Unmarshall(variant, typeof(T));
		}

		public object Unmarshall(JVariant variant, Type type)
		{
			var trait = GetTrait(type);
			if (trait == null)
			{
				throw new VariantException($"Unable to convert {variant} to {type.Name}.");
			}

			return trait.FromVariant(variant);
		}

		private ConvertTrait GetTrait(Type type)
		{
			ConvertTrait trait = null;

			// 既に登録されている場合はそれを返す
			lock (m_Lock)
			{
				trait = m_Traits.GetValueOrDefault(type);
				if (trait != null)
				{
					return trait;
				}
			}

			// Converter の登録から作成
			trait = m_Converter.GetTrait(type);

			// なければ生成
			trait ??= CreateTrait(type);

			// 覚えておく
			lock (m_Lock)
			{
				m_Traits[type] = trait;
			}
			return trait;
		}

		// type に対する ConvertTrait を生成して返す。
		// 
		// IJVariantConvertible
		//	=> IJVariantConvertible の作法で変換
		// 配列
		//	=> ArrayTypeConvertTrait で変換
		// Nullable<>
		// 	=> NullableTypeConvertTrait で変換
		// デフォルトコンストラクタがあり、IDictionary<string,> を実装している
		//	=> JObject に変換
		// デフォルトコンストラクタがあり、ICollection<> を実装している
		//	=> JArray に変換
		// それ以外
		//	=> リフレクションで変換を試みる
		private static ConvertTrait CreateTrait(Type type)
		{

			// IJVariantConvertible
			if (typeof(IJVariantConvertible).IsAssignableFrom(type))
			{
				var traitType = typeof(JVariantConvertibleTrait<>).MakeGenericType(type);
				var ctor = traitType.GetConstructor(Array.Empty<Type>());
				return ctor.Invoke(Array.Empty<object>()) as ConvertTrait;
			}

			// 配列
			if (type.IsArray)
			{
				var elementType = type.GetElementType();
				var traitType = typeof(ArrayTypeConvertTrait<>).MakeGenericType(elementType);
				return Activator.CreateInstance(traitType) as ConvertTrait;
			}

			if (type.IsEnum)
			{
				var traitType = typeof(EnumTypeConvertTrait<>).MakeGenericType(type);
				return Activator.CreateInstance(traitType) as ConvertTrait;
			}

			// Nullable<>
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				var valueType = type.GetGenericArguments()[0];
				var traitType = typeof(NullableTypeConvertTrait<>).MakeGenericType(valueType);
				return Activator.CreateInstance(traitType) as ConvertTrait;
			}

			if (type == typeof(object))
			{
				return new ObjectConvertTrait();
			}

			// public なデフォルトコンストラクタがある？
			var defaultConstructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
			if (defaultConstructor != null)
			{
				// IDictionary<string,> を実装している？
				var dictInterface = type.GetInterfaces()
					.Where(t =>
						(t.IsGenericType) &&
						(t.GetGenericTypeDefinition() == typeof(IDictionary<,>)) &&
						(t.GetGenericArguments()[0] == typeof(string)))
					.FirstOrDefault();
				if (dictInterface != null)
				{
					var valueType = dictInterface.GetGenericArguments()[1];
					var traitType = typeof(DictionaryTypeConvertTrait<,>).MakeGenericType(type, valueType);
					return Activator.CreateInstance(traitType) as ConvertTrait;
				}

				// ICollection<> を実装している？
				var collectionInterface = type.GetInterfaces()
					.Where(t =>
						(t.IsGenericType) &&
						(t.GetGenericTypeDefinition() == typeof(ICollection<>)))
					.FirstOrDefault();
				if (collectionInterface != null)
				{
					var valueType = collectionInterface.GetGenericArguments()[0];
					var traitType = typeof(CollectionTypeConvertTrait<,>).MakeGenericType(type, valueType);
					return Activator.CreateInstance(traitType) as ConvertTrait;
				}

				// それ以外、リフレクションで変換を試みる
				var unknownTraitType = typeof(UnknownTypeConvertTrait<>).MakeGenericType(type);
				return Activator.CreateInstance(unknownTraitType) as ConvertTrait;
			}

			return null;
		}


	}
}
