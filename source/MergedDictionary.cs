using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extras
{
    internal class MergedDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TValue : class
    {
        private readonly Dictionary<TKey, List<TValue>> dictionary = new Dictionary<TKey, List<TValue>>();

        public TValue this[TKey key]
        {
            get => dictionary[key]?.LastOrDefault();
            set
            {
                if (dictionary.TryGetValue(key, out var values))
                {
                    values.Add(value);
                } else
                {
                    dictionary[key] = new List<TValue> { value };
                }
            }
        }
        public ICollection<TKey> Keys => dictionary.Keys;

        public ICollection<TValue> Values => dictionary.Values.Select(list => list.LastOrDefault()).ToList();

        public int Count => dictionary.Count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            if (dictionary.TryGetValue(key, out var values))
            {
                values.Add(value);
            }
            else
            {
                dictionary[key] = new List<TValue> { value };
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (dictionary.TryGetValue(item.Key, out var values))
            {
                values.Add(item.Value);
            }
            else
            {
                dictionary[item.Key] = new List<TValue> { item.Value };
            }
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (dictionary.TryGetValue(item.Key, out var values))
            {
                return values.Contains(item.Value);
            }
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(TKey key)
        {
            if (dictionary.TryGetValue(key, out var values))
            {
                if (values.Count > 1)
                {
                    values.RemoveAt(values.Count - 1);
                    return true;
                } else
                {
                    return dictionary.Remove(key);
                }
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (dictionary.TryGetValue(item.Key, out var values))
            {
                if (values.Count > 0)
                {
                    if (values.Remove(item.Value))
                    {
                        if (values.Count == 0)
                        {
                            dictionary.Remove(item.Key);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (dictionary.TryGetValue(key, out var values))
            {
                value = values.LastOrDefault();
                return true;
            }
            value = default(TValue);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }
}
