namespace UMP.Services.Helpers
{
    internal static class Json
    {
        public static string GetKey(string key, string source)
        {
            var quotedKey = '"' + key + '"';
            var index = 0;

            while (true)
            {
                index = source.IndexOf(quotedKey, index);

                if (index == -1)
                    return string.Empty;

                index += quotedKey.Length;

                var start = index;
                start = source.SkipWhitespace(start);

                if (source[start++] != ':')
                    continue;

                start = source.SkipWhitespace(start);

                if (source[start++] != '"')
                    continue;

                var end = start;
                while (source[end] != '"')
                    end++;

                return source.Substring(start, end - start);
            }
        }
    }
}
