using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gatebox.Variant.Internal;

namespace Gatebox.Variant
{


	/// <summary>
	/// JVariant と他の型、JSON文字列との変換を行う。
	/// <para>
	/// 基本的な使用方法ではこのクラスを利用する必要はありません。
	/// <see cref="JVariant"/> の各種の static メソッドを利用してください。</para>
	/// <para>
	/// このクラスは、変換のための <see cref="ConvertTrait"/> を登録し、管理します。
	/// ある型をどのように <see cref="JVariant"/> と相互変換するかは一意に決まるものではないため、
	/// （例えばサーバによって日付の文字列のフォーマットが異なる場合など）
	/// 特別な型変換を行いたい場合に VariantConverter のインスタンスをつくり、
	/// そこに <see cref="ConvertTrait"/> を登録して変換を行うことで対応できます。</para>
	/// <para>
	/// また、変換のために利用される内部オブジェクトのキャッシュもこの中で行われるため、
	/// 特定の用途の型変換を同じ VariantConverter に集めることで実行時効率が多少向上します。</para>
	/// </para>
	/// </summary>
	public class VariantConverter
	{
		//==============================================================================
		// static members
		//==============================================================================

		// 変換対象型 => ConvertTrait 型
		// これは ConvertTraitAttribute が付与されたクラスを収集したもの。
		// VariantConverter のインスタンスごとにカスタムを行うことができ、そちらのほうが優先される。
		private static Dictionary<Type, Type> s_BaseTraits;


		/// <summary>
		/// JVariant が利用するデフォルトの変換器
		/// </summary>
		public static VariantConverter Default => new();


		/// <summary>
		/// ConvertTraitAttribute が付与されたクラスを収集し、その対応を返す。
		/// </summary>
		public static IReadOnlyDictionary<Type, Type> GetBaseTraits()
		{
			s_BaseTraits ??= CollectTraitsDefinitions();
			return s_BaseTraits;
		}

		/// <summary>
		/// プリミティブ及び、JVariant 関連の型を指定の型に変換する。
		/// <para>
		/// この挙動は ConvertTrait インスタンスによる変換とは異なり固定で行われ、カスタマイズすることはできません。
		/// </para>
		/// </summary>
		public static object ConvertVariantFixed(JVariant variant, Type type)
		{
			if (variant.IsNull())
			{
				return null;
			}

			if (type == typeof(JVariant))
			{
				return variant;
			}

			if (type == typeof(JArray))
			{
				return variant.AsArray();
			}

			if (type == typeof(JObject))
			{
				return variant.AsObject();
			}

			if (type == typeof(JVariantTag))
			{
				return variant;
			}

			if (type == typeof(string))
			{
				return variant.AsString();
			}

			if (type == typeof(int))
			{
				return variant.AsInt();
			}

			if (type == typeof(bool))
			{
				return variant.AsBool();
			}

			if (type == typeof(double))
			{
				return variant.AsDouble();
			}
			if (type == typeof(float))
			{
				return variant.AsFloat();
			}

			if (type == typeof(char))
			{
				return (char)variant.AsInt();
			}

			if (type == typeof(long))
			{
				return variant.AsLong();
			}

			if (type == typeof(short))
			{
				return (short)variant.AsInt();
			}

			if (type == typeof(sbyte))
			{
				return (sbyte)variant.AsInt();
			}

			if (type == typeof(uint))
			{
				return (uint)variant.AsLong();
			}

			if (type == typeof(ushort))
			{
				return (ushort)variant.AsInt();
			}

			if (type == typeof(byte))
			{
				return (byte)variant.AsInt();
			}

			if (type == typeof(ulong))
			{
				return (ulong)variant.AsDouble();
			}

			return null;
		}


		/// <summary>
		/// プリミティブ、JVaraint 関連の型を JVariant に変換する。
		/// <para>
		/// 変換できないときは null を返します。
		/// null に対しては JVaraint の Null を返却するため、失敗とは区別できます。</para>
		/// <para>
		/// この挙動は ConvertTrait インスタンスによる変換とは異なり固定で行われ、カスタマイズすることはできません。</para>
		/// </summary>
		public static JVariant CreateVariantFixed(object v)
		{
			if (v == null)
			{
				return new JVariant();
			}

			if (v is JVariant variant)
			{
				return variant;
			}

			if (v is JArray array)
			{
				return array.AsVariant();
			}

			if (v is JObject obj)
			{
				return obj.AsVariant();
			}

			if (v is JVariantTag tag)
			{
				return tag.Value;
			}

			// string
			if (v is string s)
			{
				return new JVariant(s);
			}

			if (v is int i)
			{
				return new JVariant(i);
			}

			if (v is bool b)
			{
				return new JVariant(b);
			}

			if (v is double d)
			{
				return new JVariant(d);
			}
			if (v is float f)
			{
				return new JVariant(f);
			}

			if (v is char c)
			{
				return new JVariant(c);
			}

			// int 系 ulong 以外。結果的に long に変換される。
			if (v is long || v is short || v is sbyte || v is uint || v is ushort || v is byte)
			{
				return new JVariant(Convert.ToInt64(v));
			}

			// ulong これは double に更に変換される
			if (v is ulong)
			{
				return new JVariant(Convert.ToUInt64(v));
			}

			return null;
		}


		//==============================================================================
		// instance members
		//==============================================================================

		private readonly object m_Lock = new();
		private readonly Dictionary<Type, Type> m_CustomTraits = new();
		private readonly VariantMarshaller m_Marshaller;
		private JsonFormatPolicy m_DefaultFormatPolicy = JsonFormatPolicy.OneLiner;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public VariantConverter()
		{
			m_Marshaller = new VariantMarshaller(this);
		}

		public JsonFormatPolicy JsonFormatPolicy
		{
			get => m_DefaultFormatPolicy;
			set => m_DefaultFormatPolicy = value;
		}

		/// <summary>
		/// Target の変換を Trait で行うように登録する。
		/// <para>
		/// <see cref="ConvertTraitAttribute"/> をつけた <see cref="ConvertTrait"/> 派生クラスは自動的に登録されます。
		/// そのため、通常はこれを利用する必要はありません。</para>
		/// <para>
		/// 文脈によって特定の型に対して特別な変換を行いたい場合に利用してください。</para>
		/// </summary>
		/// <typeparam name="Target">変換したい型</typeparam>
		/// <typeparam name="Trait">Target を変換する <see cref="ConvertTrait"/> 派生クラス。</typeparam>
		public void RegisterTrait<Target, Trait>() where Trait : ConvertTrait, new()
		{
			lock (m_Lock)
			{
				m_CustomTraits[typeof(Target)] = typeof(Trait);
			}
		}

		/// <summary>
		/// 変換用の型情報のキャッシュをクリアする。
		/// </summary>
		public void ClearCache()
		{
			m_Marshaller.Clear();
		}


		/// <summary>
		/// 任意型を JSON に変換する
		/// </summary>
		public string ConvertToJSON(object value, JsonFormatPolicy policy = null)
		{
			return CreateVariant(value).ToJSON(policy ?? m_DefaultFormatPolicy);
		}


		/// <summary>
		/// 任意型を U8View の JSON に変換する。
		/// </summary>
		public U8View ConvertToU8JSON(object value, JsonFormatPolicy policy = null)
		{
			return CreateVariant(value).ToU8JSON(policy ?? m_DefaultFormatPolicy);
		}


		/// <summary>
		/// 任意の型から JVariant を生成して返す。
		/// </summary>
		public JVariant CreateVariant(object value)
		{
			var v = CreateVariantFixed(value);
			if (v is not null)
			{
				return v;
			}

			var context = ConvertContext.Acquire();
			try
			{
				context.PushConverter(this);
				return m_Marshaller.Marshall(value);
			}
			finally
			{
				context.PopConverter();
				context.Release();
			}
		}

		internal JVariant Marshal(object value)
		{
			return m_Marshaller.Marshall(value);
		}
		internal object Unmarshal(JVariant variant, Type type)
		{
			return m_Marshaller.Unmarshall(variant, type);
		}
		internal T Unmarshal<T>(JVariant variant)
		{
			return m_Marshaller.Unmarshall<T>(variant);
		}

		/// <summary>
		/// JVariant を任意の型に変換する。
		/// </summary>
		public T ConvertVariantTo<T>(JVariant variant)
		{
			if (variant is null)
			{
				return default;
			}
			object v = ConvertVariantFixed(variant, typeof(T));
			if (v is not null)
			{
				return (T)v;
			}

			var context = ConvertContext.Acquire();
			try
			{
				context.PushConverter(this);
				return m_Marshaller.Unmarshall<T>(variant);
			}
			finally
			{
				context.PopConverter();
				context.Release();
			}
		}


		/// <summary>
		/// Target の変換を Trait で行うように登録する。
		/// <para>
		/// <see cref="ConvertTraitAttribute"/> をつけた <see cref="ConvertTrait"/> 派生クラスは自動的に登録されます。
		/// そのため、通常はこれを利用する必要はなく、特定の状況下で変換をカスタマイズするために必要なものです。</para>
		/// <para>
		/// trait は <see cref="ConvertTrait"/> の派生型である必要があります。</para>
		/// <para>
		/// target, trait 両者に同じ型パラメータから生成できるジェネリック定義型を登録することができます。
		/// <code><pre>class MyValue&lt;T&gt; {}</pre></code>があるとして、
		/// <code><pre>class MyValueTrait&lt;T&gt; : ConvertTrait&lt;MyValue&lt;T&gt;&gt; {}</pre></code>としたうえで、
		/// <code><pre>RegisterTrait(typeof(MyValue&lt;&gt;), typeof(MyValueTrait&lt;&gt;));</pre></code>という登録を行うと、
		/// 例えば <c>MyValue&lt;int&gt;</c> に対して <c>MyValueTrait&lt;int&gt;</c>  が生成されて利用されます。</para>
		/// <para>
		/// 変換方法はキャッシュされるため、すでに変換したことにある型はこれを呼んでも反映されません。
		/// 変換したことのある型の登録後は <see cref="ClearCache"/> を呼び出してください。</para>
		/// </summary>
		public void RegisterTrait(Type target, Type trait)
		{
			ChackTraitType(trait, target);
			lock (m_Lock)
			{
				m_CustomTraits[target] = trait;
			}
		}


		/// <summary>
		/// type に対応する ConvertTrait を取得する。
		/// </summary>
		public ConvertTrait GetTrait(Type type)
		{
			var traitType = GetTraitType(type);
			if (traitType == null)
			{
				return null;
			}

			return Activator.CreateInstance(traitType) as ConvertTrait;
		}

		private Type GetTraitType(Type target)
		{
			lock (m_Lock)
			{
				// まず具体型でそのまま探す
				Type ret = GetMatchingType(target);
				if (ret != null)
				{
					return ret;
				}

				// Generic?
				if (!target.IsGenericType)
				{
					return null;
				}

				// Generic 定義で探す
				var generic = target.GetGenericTypeDefinition();
				var targetGeneric = GetMatchingType(generic);
				if (targetGeneric == null)
				{
					return null;
				}

				// 具体化
				var args = target.GetGenericArguments();

				try
				{
					ret = targetGeneric.MakeGenericType(args);
				}
				catch (ArgumentException e)
				{
					throw new VariantException($"failed to concretize a {targetGeneric}.", e);
				}

				return ret;
			}
		}

		// この型に対応する ConvertTrait を取得する。
		// カスタムが先、共通があと
		private Type GetMatchingType(Type target)
		{
			Type ret = null;

			if (m_CustomTraits.TryGetValue(target, out ret))
			{
				return ret;
			}

			var baseTraits = GetBaseTraits();
			if (baseTraits.TryGetValue(target, out ret))
			{
				return ret;
			}

			return null;
		}


		// ConvertTraitAttribute が付与されたクラスを収集する。
		private static Dictionary<Type, Type> CollectTraitsDefinitions()
		{
			var ret = new Dictionary<Type, Type>();

			// ConvertTraitAttribute つきクラスを収集
			var types = FindTypesWithCustomAttribute(typeof(ConvertTraitAttribute));

			foreach (var type in types)
			{
				// 変換対象の型を取得
				var attr = type.GetCustomAttributes().FirstOrDefault(attr => attr is ConvertTraitAttribute) as ConvertTraitAttribute;
				var targetType = attr.TargetType;

				// ConvertTrait を継承しているはず
				if (!type.IsSubclassOf(typeof(ConvertTrait)))
				{
					throw new VariantException($"Failed to analyze {type.Name}. Classes with the {nameof(ConvertTraitAttribute)} must inherit from {nameof(ConvertTrait)}.");
				}

				if (ret.ContainsKey(targetType))
				{
					throw new VariantException($"${nameof(ConvertTrait)} for {targetType.Name} duplicated.");
				}

				ChackTraitType(type, targetType);
				ret[targetType] = type;
			}
			return ret;
		}


		// type が targetType に対して ConvertTrait として利用できるかどうかを判定する
		// だめなときは例外を投げる。
		private static void ChackTraitType(Type type, Type targetType)
		{
			// ConvertTrait を継承しているはず
			if (!type.IsSubclassOf(typeof(ConvertTrait)))
			{
				throw new VariantException($"{type.Name} must inherit from ${nameof(ConvertTrait)}.");
			}

			if (type.GetConstructor(Type.EmptyTypes) == null)
			{
				throw new VariantException($"{type.Name} requires a no parameter constructor.");
			}

			if (type.IsGenericType && type.ContainsGenericParameters && (!type.IsGenericTypeDefinition))
			{
				throw new VariantException($"{type.Name}  has partially unresolved type parameters. cannot be used as ${nameof(ConvertTrait)}");
			}

			if (type.IsGenericTypeDefinition)
			{
				if (!targetType.IsGenericTypeDefinition)
				{
					throw new VariantException($"The generic type definition {type.Name} does not match {targetType.Name}. ");
				}
				if (!CanConcretizeWithSameArguments(type, targetType))
				{
					throw new VariantException($"The generic type definition {type.Name} does not match {targetType.Name}. The type arguments and constraints must be equivalent.");
				}
			}
		}

		// 2つのジェネリック型定義が同じ型引数で具体化できるかどうかを判定する
		// type2 のほうが多少厳しいのは許される。（つまり [type1 は type2 と同じ型引数で具体化できるか] を返す。）
		private static bool CanConcretizeWithSameArguments(Type type1, Type type2)
		{
			if (!type1.IsGenericTypeDefinition || !type2.IsGenericTypeDefinition)
			{
				return false;
			}

			var args1 = type1.GetGenericArguments();
			var args2 = type2.GetGenericArguments();

			if (args1.Length != args2.Length)
			{
				return false;
			}

			for (int i = 0; i < args1.Length; i++)
			{
				if (args1[i].GenericParameterAttributes != args2[i].GenericParameterAttributes)
				{
					return false;
				}

				var constraint1 = args1[i].GetGenericParameterConstraints();
				var constraint2 = args2[i].GetGenericParameterConstraints();

				foreach (var c1 in constraint1)
				{
					if (!constraint2.Contains(c1))
					{
						return false;
					}
				}
			}
			return true;
		}


		// 指定された属性を持つクラス定義をアプリケーションドメイン全体から集める。
		private static List<Type> FindTypesWithCustomAttribute(Type attr)
		{
			var ret = new List<Type>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in assembly.GetTypes())
				{
					if (Attribute.IsDefined(type, attr))
					{
						ret.Add(type);
					}
				}
			}
			return ret;
		}



	}
}
