using System;
using UnityEngine;

namespace Siroshton.Masaya.Attribute
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class InterfaceAttribute : PropertyAttribute
    {
        public Type RequiredType { get; private set; }

        public InterfaceAttribute(Type type)
        {
            RequiredType = type;
        }
    }
}