using System;
using UnityEngine;

namespace UMP
{
    public class ExpandedAndroidExoPlayer
    {
        private AndroidJavaObject _pluginObj;
        private MediaStats _mediaStats;

        public ExpandedAndroidExoPlayer(AndroidJavaObject player)
        {
            _pluginObj = player;
        }

        #region Platform dependent functionality
        /// <summary>
        /// Get the current statistics about the media
        /// </summary>
        public MediaStats MediaStats
        {
            get
            {
                if (_pluginObj != null)
                {
                    var statsText = _pluginObj.Call<string>("expandedGetMediaStats");

                    if (statsText != null)
                    {
                        var stats = statsText.Split('@');
                        int.TryParse(stats[0], out _mediaStats.InputReadBytes);
                        float.TryParse(stats[1], out _mediaStats.InputBitrate);
                        int.TryParse(stats[2], out _mediaStats.DemuxReadBytes);
                        float.TryParse(stats[3], out _mediaStats.DemuxBitrate);
                        int.TryParse(stats[4], out _mediaStats.DemuxCorrupted);
                        int.TryParse(stats[5], out _mediaStats.DemuxDiscontinuity);
                        int.TryParse(stats[6], out _mediaStats.DecodedVideo);
                        int.TryParse(stats[7], out _mediaStats.DecodedAudio);
                        int.TryParse(stats[8], out _mediaStats.VideoDisplayedPictures);
                        int.TryParse(stats[9], out _mediaStats.VideoLostPictures);
                        int.TryParse(stats[10], out _mediaStats.AudioPlayedAbuffers);
                        int.TryParse(stats[11], out _mediaStats.AudioLostAbuffers);
                        int.TryParse(stats[12], out _mediaStats.StreamSentPackets);
                        int.TryParse(stats[13], out _mediaStats.StreamSentBytes);
                        float.TryParse(stats[14], out _mediaStats.StreamSendBitrate);
                    }
                }

                return _mediaStats;
            }
        }

        /// <summary>
        /// Set new video subtitle file
        /// </summary>
        /// <param name="path">Path to the new video subtitle file</param>
        /// <returns></returns>
        public bool SetSubtitleFile(Uri path)
        {
            if (_pluginObj != null)
            {
                var arg = new object[1];
                arg[0] = path.AbsoluteUri;

                return _pluginObj.Call<bool>("exportSetSubtitleFile", arg);
            }

            return false;
        }
        #endregion
    }
}