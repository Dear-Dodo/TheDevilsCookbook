using System;
using UnityEngine;

namespace TDC.Core.Type
{
    [Serializable]
    public class DirtyProperty<T>
    {
        public T Value
        {
            get => _Value;
            set
            {
                if (!Equals(_Value, value))
                {
                    OnValueChanged?.Invoke(_Value, value);
                    Dirty = true;
                }
                _Value = value;
                OnValueSet?.Invoke(_Value);
            }
        }
        [SerializeField] private T _Value;

        public Action<T, T> OnValueChanged;
        public Action<T> OnValueSet;
        public bool Dirty;


        public DirtyProperty(T value) => _Value = value;

        public static implicit operator T(DirtyProperty<T> property) => property.Value;
    }
}