#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grabbit
{
    [Serializable]
    public class MeshMeshListDictionary : SerializableDictionary<Mesh, MeshList>
    {
    }

    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] public TKey defaultKey;

        protected Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        public TValue this[TKey key]
        {
            get => dictionary[key];
            set => dictionary[key] = value;
        }

        public ICollection<TKey> Keys => dictionary.Keys;

        public ICollection<TValue> Values => dictionary.Values;

        public int Count => dictionary.Count;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;


        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key, value);

            if (keys == null)
                keys = new List<TKey>();
            if (values == null)
                values = new List<TValue>();

            keys.Add(key);
            values.Add(value);
        }

        public void Clear()
        {
            dictionary.Clear();

            keys.Clear();
            values.Clear();
        }


        public bool Remove(TKey key)
        {
            var index = keys.IndexOf(key);
            keys.Remove(key);
            if (values.Count > index && index > 0)
                values.RemoveAt(index);

            return dictionary.Remove(key);
        }

        public bool ContainsKey(TKey key)
        {
            if (dictionary == null)
                return false;

            return dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (dictionary == null)
            {
                value = default;
                return false;
            }

            return dictionary.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            (dictionary as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return (dictionary as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            (dictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return (dictionary as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }


        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (dictionary.Count == 0)
            {
                keys.Clear();
                values.Clear();
            }
            else
            {
                keys = new List<TKey>(dictionary.Count);
                values = new List<TValue>(dictionary.Count);

                foreach (var pair in dictionary)
                {
                    keys.Add(pair.Key);
                    values.Add(pair.Value);
                }
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (dictionary == null)
                dictionary = new Dictionary<TKey, TValue>(keys.Count);
            else
                dictionary.Clear();

            for (var i = 0; i < keys.Count; i++)
            {
                if (keys[i] == null)
                    keys[i] = defaultKey;

                if (i < values.Count)
                    dictionary[keys[i]] = values[i];
                else
                    dictionary[keys[i]] = default;
            }

            keys.Clear();
            values.Clear();
        }

        public bool ContainsValue(TValue value)
        {
            if (dictionary == null)
                return false;

            return dictionary.ContainsValue(value);
        }

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
    }
}
#endif