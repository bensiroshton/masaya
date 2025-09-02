using System;
using System.Collections.Generic;
using UnityEngine;

namespace Siroshton.Masaya.Util
{
    [Serializable]
    public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> _keys = new List<TKey>();
        [SerializeField] private List<TValue> _values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            this.Clear();

            if (_keys.Count != _values.Count)
            {
                throw new System.Exception(string.Format("key count != value count; did something not serialize properly?"));
            }

            for (int i = 0; i < _keys.Count; i++)
            {
                this.Add(_keys[i], _values[i]);
            }
        }
    }
}
