using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Services.Helpers
{
    internal class Query : IDictionary<string, string>
    {
        public class KeyCollection : ICollection<string>
        {
            private readonly Query query;

            public KeyCollection(Query query)
            {
                this.query = query;
            }

            public int Count
            {
                get { return query.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public void Add(string item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(string item)
            {
                for (int i = 0; i < query.Count; i++)
                {
                    var pair = query.Pairs[i];
                    if (item == pair.Key)
                        return true;
                }
                return false;
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                for (int i = 0; i < query.Count; i++)
                    array[arrayIndex++] = query.Pairs[i].Key;
            }

            public IEnumerator<string> GetEnumerator()
            {
                for (int i = 0; i < query.Count; i++)
                    yield return query.Pairs[i].Key;
            }

            public bool Remove(string item)
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class ValueCollection : ICollection<string>
        {
            private readonly Query query;

            public ValueCollection(Query query)
            {
                this.query = query;
            }

            public int Count
            {
                get { return query.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public void Add(string item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(string item)
            {
                for (int i = 0; i < query.Count; i++)
                {
                    var pair = query.Pairs[i];
                    if (item == pair.Value)
                        return true;
                }
                return false;
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                for (int i = 0; i < query.Count; i++)
                    array[arrayIndex++] = query.Pairs[i].Value;
            }

            public IEnumerator<string> GetEnumerator()
            {
                for (int i = 0; i < query.Count; i++)
                    yield return query.Pairs[i].Value;
            }

            public bool Remove(string item)
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private int _count;
        private readonly string _baseUri;
        private KeyValuePair<string, string>[] _pairs;

        public Query(string uri)
        {
            var divide = uri.IndexOf('?');

            if (divide == -1)
            {
                var amp = uri.IndexOf('&');
                if (amp == -1)
                {
                    // no query parameters
                    _baseUri = uri;
                    return;
                }

                // no base URL
                _baseUri = null;
            }
            else
            {
                // normal URL
                _baseUri = uri.Substring(0, divide);
                uri = uri.Substring(divide + 1);
            }

            var keyValues = uri.Split('&');
            _pairs = new KeyValuePair<string, string>[keyValues.Length];

            for (int i = 0; i < keyValues.Length; i++)
            {
                var pair = keyValues[i];
                var equals = pair.IndexOf('=');
                var key = string.Empty;
                var value = string.Empty;

                if (equals != pair.LastIndexOf('='))
                {
                    key = pair.Substring(0, equals);
                    value = String.Empty;
                }
                else
                {
                    key = pair.Substring(0, equals);
                    value = pair.Substring(equals + 1);
                }

                _pairs[i] = new KeyValuePair<string, string>(key, value);
            }

            _count = keyValues.Length;
        }

        public string this[string key]
        {
            get
            {
                for (int i = 0; i < _count; i++)
                {
                    var pair = _pairs[i];
                    if (pair.Key == key)
                        return pair.Value;
                }

                throw new KeyNotFoundException();
            }

            set
            {
                for (int i = 0; i < _count; i++)
                {
                    var pair = _pairs[i];
                    if (pair.Key == key)
                    {
                        _pairs[i] = new KeyValuePair<string, string>(key, value);
                        return;
                    }
                }

                throw new KeyNotFoundException();
            }
        }

        public string BaseUri
        {
            get { return _baseUri; }
        }

        public int Count
        {
            get { return _count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public KeyCollection Keys
        {
            get
            {
                return new KeyCollection(this);
            }
        }

        ICollection<string> IDictionary<string, string>.Keys
        {
            get
            {
                var keys = new List<string>();

                foreach (var pair in _pairs)
                    keys.Add(pair.Key);

                return keys;
            }
        }

        public KeyValuePair<string, string>[] Pairs
        {
            get { return _pairs; }
        }

        public ValueCollection Values
        {
            get
            {
                return new ValueCollection(this);
            }
        }

        ICollection<string> IDictionary<string, string>.Values
        {
            get
            {
                var values = new List<string>();

                foreach (var pair in _pairs)
                    values.Add(pair.Value);

                return values;
            }
        }

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(string key, string value)
        {
            EnsureCapacity(_count + 1);
            _pairs[_count++] = new KeyValuePair<string, string>(key, value);
        }

        public void Clear()
        {
            if (_count == 0)
                return;

            Array.Clear(_pairs, 0, _count);
            _count = 0;
        }

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        {
            for (int i = 0; i < _count; i++)
            {
                var pair = _pairs[i];

                if (item.Key == pair.Key && 
                    item.Value == pair.Value)
                    return true;
            }
            return false;
        }

        public bool ContainsKey(string key)
        {
            for (int i = 0; i < _count; i++)
            {
                if (key == _pairs[i].Key)
                    return true;
            }
            return false;
        }

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            Array.Copy(_pairs, 0, array, arrayIndex, _count);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
                yield return _pairs[i];
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            return Remove(item.Key);
        }

        public bool Remove(string key)
        {
            for (int i = 0; i < _count; i++)
            {
                var pair = _pairs[i];
                if (pair.Key == key)
                {
                    // found it
                    if (i != _count--)
                        Array.Copy(_pairs, i + 1, _pairs, i, _count - i);
                    _pairs[_count] = default(KeyValuePair<string, string>);
                    return true;
                }
            }
            return false;
        }

        public bool TryGetValue(string key, out string value)
        {
            for (int i = 0; i < _count; i++)
            {
                var pair = _pairs[i];
                if (key == pair.Key)
                {
                    value = pair.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            if (_count == 0)
                return _baseUri;

            var builder = new StringBuilder();
            if (_baseUri != null)
                builder.Append(_baseUri).Append('?');

            var pair = _pairs[0]; // OK since we know count is at least 1
            builder.Append(pair.Key)
                .Append('=').Append(pair.Value);

            for (int i = 1; i < _count; i++)
            {
                pair = _pairs[i];

                builder.Append('&').Append(pair.Key)
                    .Append('=').Append(pair.Value);
            }

            return builder.ToString();
        }

        private void EnsureCapacity(int capacity)
        {
            if (capacity > _pairs.Length)
            {
                capacity = Math.Max(capacity, _pairs.Length * 2);

                Array.Resize(ref _pairs, capacity);
            }
        }
    }
}
