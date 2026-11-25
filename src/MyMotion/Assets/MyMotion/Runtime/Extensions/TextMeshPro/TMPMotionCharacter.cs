#if MYMOTION_SUPPORT_TMP
using UnityEngine;

namespace MyMotion.Extensions
{
    /// <summary>
    /// TextMeshPro の1文字分のアニメーション対象プロパティを更新する。
    /// </summary>
    /// <typeparam name="TValue">アニメーションさせる値の型。</typeparam>
    public delegate void TMPCharacterMotionUpdateAction<in TValue>(TValue value, int index, ref TMPMotionCharacter character);

    /// <summary>
    /// TextMeshPro の1文字分の Transform と色のアニメーション値。
    /// </summary>
    public struct TMPMotionCharacter
    {
        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Rotation;
        public Color Color;
    }
}
#endif
