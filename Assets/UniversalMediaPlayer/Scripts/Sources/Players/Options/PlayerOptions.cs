using System;
using System.Collections.Generic;
using UnityEngine;

namespace UMP
{
    public class PlayerOptions
    {
        private const string FIXED_SIZE_WIDTH_KEY = "fixed-size-width";
        private const string FIXED_SIZE_HEIGHT_KEY = "fixed-size-height";
        public const int DEFAULT_CACHING_VALUE = 300;

        private Dictionary<string, string> _options;

        public enum States
        {
            Default = -1,
            Disable = 0,
            Enable = 1
        }

        public PlayerOptions(string[] options)
        {
            _options = new Dictionary<string, string>();

            if (options != null)
            {
                foreach (var option in options)
                {
                    var values = option.Split(new char[] { '=' }, 2);

                    if (values.Length > 1)
                        SetValue(values[0], values[1]);
                    else
                        SetValue(values[0], null);
                }
            }
        }

        /// <summary>
        /// Fixed video size - rescale default video size.
        /// </summary>
        public Vector2 FixedVideoSize
        {
            get
            {
                var width = GetValue<int>(FIXED_SIZE_WIDTH_KEY);
                var height = GetValue<int>(FIXED_SIZE_HEIGHT_KEY);

                return new Vector2(width, height);
            }

            set
            {
                var width = value.x > 0 ? value.x : 0;
                var height = value.y > 0 ? value.y : 0;

                SetValue(FIXED_SIZE_WIDTH_KEY, width.ToString());
                SetValue(FIXED_SIZE_HEIGHT_KEY, height.ToString());
            }
        }

        public void SetValue(string key, string value)
        {
            if (!_options.ContainsKey(key))
                _options.Add(key, value);
            else
                _options[key] = value;
        }

        public string GetValue(string key)
        {
            if (_options.ContainsKey(key))
                return _options[key];

            return null;
        }

        public T GetValue<T>(string key)
        {
            var optionValue = GetValue(key);

            string typeName = typeof(T).Name;

            if (typeName == typeof(bool).Name)
                return (T)Convert.ChangeType(optionValue != null, typeof(T));

            if (!string.IsNullOrEmpty(optionValue))
            {
                if (typeName == typeof(int).Name)
                {
                    int value = 0;
                    int.TryParse(optionValue, out value);
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                if (typeName == typeof(float).Name)
                {
                    float value = 0;
                    float.TryParse(optionValue, out value);
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                if (typeName == typeof(string).Name)
                {
                    return (T)Convert.ChangeType(optionValue, typeof(T));
                }
            }

            return default(T);
        }

        public void RemoveOption(string key)
        {
            if (_options.ContainsKey(key))
                _options.Remove(key);
        }

        public void ClearOptions()
        {
            _options.Clear();
        }

        public string GetOptions(char separator)
        {
            string options = string.Empty;

            foreach (var option in _options)
            {
                if (option.Key.StartsWith(":") ||
                    option.Key.StartsWith("-") ||
                    option.Key.StartsWith("--"))
                {
                    if (!string.IsNullOrEmpty(option.Value))
                        options += string.Format(option.Key + "={0}", option.Value);
                    else
                        options += option.Key;
                    options += separator;
                }
            }

            return options.Trim();
        }
    }
}