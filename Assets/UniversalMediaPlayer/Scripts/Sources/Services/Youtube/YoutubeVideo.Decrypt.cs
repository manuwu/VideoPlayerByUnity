using Services.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace UMP.Services.Youtube
{
    public partial class YoutubeVideo
    {
        private static readonly Regex DefaultDecryptionFunctionRegex = new Regex(@"\bc\s*&&\s*d\.set\([^,]+\s*,\s*\([^)]*\)\s*\(\s*([a-zA-Z0-9$]+)\(");
        private static readonly Regex FunctionRegex = new Regex(@"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(");

        public IEnumerator Decrypt(string decryptFunction = null, Action<string> errorCallback = null)
        {
            if (_encrypted)
            {
                var query = new Query(_uri);
                var signature = string.Empty;

                if (!query.TryGetValue("signature", out signature))
                    yield break;

                var requestText = string.Empty;
#if UNITY_2017_2_OR_NEWER
                var request = UnityWebRequest.Get(_jsPlayer);
                yield return request.SendWebRequest();
#else
                var request = new WWW(_jsPlayer);
                yield return request;
#endif

                try
                {
                    if (!string.IsNullOrEmpty(request.error))
                        throw new Exception(string.Format("[YouTubeVideo.Decrypt] jsPlayer request is failed: {0}", request.error));

#if UNITY_2017_2_OR_NEWER
                    requestText = request.downloadHandler.text;
#else
                    requestText = request.text;
#endif

                    query["signature"] = DecryptSignature(requestText, signature, decryptFunction);
                    _uri = query.ToString();
                    _encrypted = false;
                }
                catch (Exception error)
                {
                    if (errorCallback != null)
                        errorCallback(error.ToString());
                }
            }
        }

        private string DecryptSignature(string js, string signature, string decryptFunction)
        {
            var functionLines = GetDecryptionFunctionLines(js, decryptFunction);
            var decryptor = new Decryptor();

            foreach (var functionLine in functionLines)
            {
                if (decryptor.IsComplete)
                    break;

                var match = FunctionRegex.Match(functionLine);

                if (match.Success)
                    decryptor.AddFunction(js, match.Groups[1].Value);
            }

            foreach (var functionLine in functionLines)
            {
                var match = FunctionRegex.Match(functionLine);

                if (match.Success)
                    signature = decryptor.ExecuteFunction(signature, functionLine, match.Groups[1].Value);
            }

            return signature;
        }

        private string[] GetDecryptionFunctionLines(string js, string decryptFunction)
        {
            var decryptionFunction = GetDecryptionFunction(js, decryptFunction);
            var match = Regex.Match(js, string.Format(@"(?!h\.){0}=function\(\w+\)\{{(.*?)\}}", Regex.Escape(decryptionFunction)), RegexOptions.Singleline);

            if (!match.Success)
                throw new Exception("[YouTubeVideo.Decrypt] GetDecryptionFunctionLines failed");

            return match.Groups[1].Value.Split(';');
        }

        private string GetDecryptionFunction(string js, string decryptFunction)
        {
            var decryptionFunctionRegex = string.IsNullOrEmpty(decryptFunction) ? DefaultDecryptionFunctionRegex :
                new Regex(decryptFunction);

            var match = decryptionFunctionRegex.Match(js);

            if (!match.Success)
                throw new Exception("[YouTubeVideo.Decrypt] GetDecryptionFunction failed");

            return match.Groups[1].Value;
        }

        private class Decryptor
        {
            private static readonly Regex ParametersRegex = new Regex(@"\(\w+,(\d+)\)");

            private readonly Dictionary<string, FunctionType> _functionTypes = new Dictionary<string, FunctionType>();
            private readonly StringBuilder _stringBuilder = new StringBuilder();

            public bool IsComplete
            {
                get { return _functionTypes.Count == Enum.GetValues(typeof(FunctionType)).Length; }
            }

            public void AddFunction(string js, string function)
            {
                var escapedFunction = Regex.Escape(function);
                FunctionType? type = null;

                if (Regex.IsMatch(js, string.Format(@"{0}:\bfunction\b\(\w+\)", escapedFunction)))
                {
                    type = FunctionType.Reverse;
                }
                else if (Regex.IsMatch(js, string.Format(@"{0}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\.", escapedFunction)))
                {
                    type = FunctionType.Slice;
                }
                else if (Regex.IsMatch(js, string.Format(@"{0}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b", escapedFunction)))
                {
                    type = FunctionType.Swap;
                }

                if (type.HasValue)
                {
                    _functionTypes[function] = type.Value;
                }
            }

            public string ExecuteFunction(string signature, string line, string function)
            {
                var type = FunctionType.Reverse;

                if (!_functionTypes.TryGetValue(function, out type))
                    return signature;

                switch (type)
                {
                    case FunctionType.Reverse:
                        return Reverse(signature);
                    case FunctionType.Slice:
                    case FunctionType.Swap:
                        var index = int.Parse(ParametersRegex.Match(line).Groups[1].Value, NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo);
                        return type == FunctionType.Slice ? Slice(signature, index) : Swap(signature, index);
                    default:
                        throw new ArgumentOutOfRangeException(string.Format("[YouTubeVideo.Decryptor] {0}", type));
                }
            }

            private string Reverse(string signature)
            {
                //_stringBuilder.Clear();
                _stringBuilder.Remove(0, _stringBuilder.Length);

                for (var index = signature.Length - 1; index >= 0; index--)
                    _stringBuilder.Append(signature[index]);

                return _stringBuilder.ToString();
            }

            private string Slice(string signature, int index)
            {
                return signature.Substring(index);
            }

            private string Swap(string signature, int index)
            {
                //_stringBuilder.Clear();
                _stringBuilder.Remove(0, _stringBuilder.Length);

                _stringBuilder.Append(signature);
                _stringBuilder[0] = signature[index];
                _stringBuilder[index] = signature[0];
                return _stringBuilder.ToString();
            }

            private enum FunctionType
            {
                Reverse,
                Slice,
                Swap
            }
        }
    }
}
