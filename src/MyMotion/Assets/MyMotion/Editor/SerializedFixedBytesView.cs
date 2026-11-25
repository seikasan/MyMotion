using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

namespace MyMotion.Editor
{
    internal readonly struct SerializedFixedBytesView
    {
        private static readonly Regex s_byteRegex = new(@"byte\d\d\d\d", RegexOptions.Compiled);
        private readonly SerializedProperty _property;
        private readonly SerializedProperty _lengthProperty;

        public SerializedFixedBytesView(SerializedProperty property)
        {
            _property = property;
            _lengthProperty = property.FindPropertyRelative("utf8LengthInBytes");
        }

        public SerializedProperty LengthProperty => _lengthProperty;

        public byte[] GetBytes() => GetByteProperties()
            .Select(prop => (byte)prop.intValue)
            .Take(_lengthProperty.intValue)
            .ToArray();

        public void SetBytes(ReadOnlySpan<byte> bytes)
        {
            int index = 0;
            foreach (SerializedProperty property in GetByteProperties())
            {
                if (index >= bytes.Length) break;

                property.intValue = bytes[index];
                index++;
            }

            _lengthProperty.intValue = bytes.Length;
        }

        private IEnumerable<SerializedProperty> GetByteProperties()
        {
            SerializedProperty property = _property.Copy();
            int currentDepth = property.depth;

            while (property.Next(true) && property.depth > currentDepth)
            {
                if (s_byteRegex.IsMatch(property.name) && property.propertyType == SerializedPropertyType.Integer)
                {
                    yield return property.Copy();
                }
            }
        }
    }
}
