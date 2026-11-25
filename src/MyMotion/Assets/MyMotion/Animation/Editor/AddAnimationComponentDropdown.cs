using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
#if UNITY_7000_0_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace MyMotion.Animation.Editor
{
    public sealed class AddAnimationComponentDropdown : AdvancedDropdown
    {
        private class Item : AdvancedDropdownItem
        {
            public Item(Type type, string menuName) : base(menuName)
            {
                Type = type;
            }

            public Type Type { get; }
        }

#if UNITY_7000_0_OR_NEWER
        [NoAutoStaticsCleanup] // 変更されない TypeCache の検索テーブル。
#endif
        private static readonly (Type Type, string MenuName)[] s_cache = TypeCache.GetTypesDerivedFrom<MyMotionAnimationComponent>()
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsSpecialName)
            .Where(x => !x.IsGenericType)
            .Select(x =>
            {
                var attribute = x.GetCustomAttribute<MyMotionAnimationComponentMenuAttribute>();
                string menuName = attribute == null ? x.Name : attribute.MenuName;
                return (x, menuName);
            })
            .OrderBy(x => x.menuName)
            .ToArray();

        public event Action<Type> OnTypeSelected;

        public AddAnimationComponentDropdown(AdvancedDropdownState state) : base(state)
        {
            Vector2 minimumSize = this.minimumSize;
            minimumSize.y = 200f;
            this.minimumSize = minimumSize;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Component");
            foreach ((Type type, string menuName) in s_cache)
            {
                string[] splitStrings = menuName.Split('/');
                AdvancedDropdownItem parent = root;

                for (int i = 0; i < splitStrings.Length; i++)
                {
                    string str = splitStrings[i];

#if UNITY_6000_5_OR_NEWER
                    var foundChildItem = parent.childList.FirstOrDefault(item => item.name == str);
#else
                    var foundChildItem = parent.children.FirstOrDefault(item => item.name == str);
#endif
                    if (foundChildItem != null)
                    {
                        parent = foundChildItem;
                        continue;
                    }

                    var child = new Item(type, str);

                    if (i == splitStrings.Length - 1)
                    {
                        FieldInfo targetField = ReflectionHelper.GetField(type, "_target", includingBaseNonPublic: true);
                        if (targetField != null)
                        {
                            child.icon = GUIHelper.GetComponentIcon(targetField.FieldType);
                        }
                        else
                        {
                            child.icon = (Texture2D)EditorGUIUtility.IconContent("ScriptableObject Icon").image;
                        }
                    }

                    parent.AddChild(child);

                    parent = child;
                }
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);

            if (item is Item componentItem)
            {
                OnTypeSelected?.Invoke(componentItem.Type);
            }
        }
    }
}
