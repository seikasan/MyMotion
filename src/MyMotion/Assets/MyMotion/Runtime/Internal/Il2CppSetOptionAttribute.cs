using System;

namespace Unity.IL2CPP.CompilerServices
{
    // IL2CPP はこの名前と namespace の属性を見て、生成コードの検査を切り替える（Unity マニュアル「IL2CPP の
    // ランタイムチェック」参照）。属性自体は Unity が提供しないため、利用側のアセンブリで定義する。
    internal enum Option
    {
        NullChecks = 1,
        ArrayBoundsChecks = 2,
        DivideByZeroChecks = 3,
    }

    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Struct |
        AttributeTargets.Method |
        AttributeTargets.Property,
        Inherited = false,
        AllowMultiple = true)]
    internal sealed class Il2CppSetOptionAttribute : Attribute
    {
        public Option Option { get; }
        public object Value { get; }

        public Il2CppSetOptionAttribute(Option option, object value)
        {
            Option = option;
            Value = value;
        }
    }
}
