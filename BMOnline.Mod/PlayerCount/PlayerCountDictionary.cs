using System.Collections;
using System.Collections.Generic;

namespace BMOnline.Mod.PlayerCount
{
    internal class PlayerCountDictionary<T> : IReadOnlyDictionary<T, int>
    {
        private readonly Dictionary<T, int> dict;

        public PlayerCountDictionary()
        {
            dict = new Dictionary<T, int>();
        }

        public void IncrementCount(T key)
        {
            dict.TryGetValue(key, out int count);
            dict[key] = count + 1;
        }

        public int this[T key]
        {
            get
            {
                dict.TryGetValue(key, out int count);
                return count;
            }
        }

        public IEnumerable<T> Keys => dict.Keys;

        public IEnumerable<int> Values => dict.Values;

        public int Count => dict.Count;

        public bool ContainsKey(T key) => dict.ContainsKey(key);

        public IEnumerator<KeyValuePair<T, int>> GetEnumerator() => dict.GetEnumerator();

        public bool TryGetValue(T key, out int value)
        {
            dict.TryGetValue(key, out int count);
            value = count;
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => dict.GetEnumerator();
    }
}
