using Services.Helpers;
using System.Collections;
using UMP.Services.Helpers;

namespace UMP.Services.Youtube
{
    public partial class YoutubeVideo : Video
    {
        private string _uri;
        private bool _encrypted;

        private readonly string _title;
        private readonly string _jsPlayer;
        private readonly int _formatCode;

        internal YoutubeVideo(string title, UnscrambledQuery query, string jsPlayer)
        {
            _title = title;
            _uri = query.Uri;
            _jsPlayer = jsPlayer;
            _encrypted = query.IsEncrypted;
            _formatCode = int.Parse(new Query(_uri)["itag"]);
        }

        public override string Title
        {
            get { return _title; }
        }

        public override string Url
        {
            get { return _uri; }
        }

        /*=>
        GetUriAsync().GetAwaiter().GetResult();*/

        /*public string GetUri(Func<DelegatingClient> makeClient) =>
            GetUriAsync(makeClient).GetAwaiter().GetResult();

        public override Task<string> GetUriAsync() =>
            GetUriAsync(() => new DelegatingClient());

        public async Task<string> GetUriAsync(Func<DelegatingClient> makeClient)
        {
            if (encrypted)
            {
                uri = await
                    DecryptAsync(uri, makeClient)
                    .ConfigureAwait(false);
                encrypted = false;
            }

            return uri;
        }*/

        public int FormatCode
        {
            get { return _formatCode; }
        }

        public bool IsEncrypted
        {
            get { return _encrypted; }
        }

        public override string ToString()
        {
            return string.Format("{0}, Resolution: {1}, AudioBitrate: {2}, Is3D: {3}", base.ToString(), Resolution, AudioBitrate, Is3D);
        }
    }
}
