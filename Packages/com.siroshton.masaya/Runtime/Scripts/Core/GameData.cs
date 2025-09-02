using System;
using System.Collections.Generic;
using UnityEngine;

namespace Siroshton.Masaya.Core
{
    [Serializable]
    public class GameData
    {
        private Dictionary<string, bool> _keyedBools = new Dictionary<string, bool>();

#if UNITY_EDITOR
        public List<string> GetBoolKeys() => new List<String>(_keyedBools.Keys);
#endif

        public bool HasBool(string key)
        {
            return _keyedBools.ContainsKey(key);
        }

        public void SetBool(string key, bool value)
        {
            _keyedBools[key] = value;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            bool value = defaultValue;
            _keyedBools.TryGetValue(key, out value);
            return value;
        }

    }
}