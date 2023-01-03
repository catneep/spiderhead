using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Control;
using Windows.Storage.Streams;
using spiderhead.Models;

namespace spiderhead.Tools
{
    class MediaController
    {
        private static async Task<GlobalSystemMediaTransportControlsSessionManager>
            GetSystemMediaTransportControlsSessionManager() =>
                await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        private static async Task<GlobalSystemMediaTransportControlsSessionMediaProperties>
            GetMediaProperties(GlobalSystemMediaTransportControlsSession session) =>
                await session.TryGetMediaPropertiesAsync();

        private static async Task<byte[]> GetThumbnailBytes(IRandomAccessStreamReference thumbnailStream)
        {
            var thumbnailReference = await thumbnailStream.OpenReadAsync();
            var thumbnailBytes = new byte[thumbnailReference.Size];
            var reader = new DataReader(thumbnailReference.GetInputStreamAt(0));
            await reader.LoadAsync((uint)thumbnailReference.Size);
            reader.ReadBytes(thumbnailBytes);

            return thumbnailBytes;
        }

        public static async Task<CurrentMedia> GetMedia()
        {
            var mediaSessionMgr = await GetSystemMediaTransportControlsSessionManager();
            var mediaSession = mediaSessionMgr.GetCurrentSession();
            if (mediaSession == null)
                return new CurrentMedia
                {
                    Name = "No Media",
                    Artist = "-"
                };

            var mediaProperties = await GetMediaProperties(mediaSession);
            try
            {
                if (mediaProperties.Thumbnail == null)
                    return new CurrentMedia(
                        mediaProperties.Title,
                        mediaProperties.Artist,
                        mediaProperties.AlbumTitle
                    );

                //var thumbnailReference = await mediaProperties.Thumbnail.OpenReadAsync();
                byte[] thumbnailBytes = await GetThumbnailBytes(mediaProperties.Thumbnail);

                return new CurrentMedia(
                    mediaProperties.Title,
                    mediaProperties.Artist,
                    mediaProperties.AlbumTitle,
                    thumbnailBytes
                );

            } catch (Exception e)
            {
                Trace.WriteLine($"{e.Message}");
            }

            return new CurrentMedia
            {
                Name = "No Media",
                Artist = "-"
            };
        }

        /// <summary>
        /// These functions return the playback status
        /// of the current media session.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetPlaybackStatus()
        {
            var mediaSessionMgr = await GetSystemMediaTransportControlsSessionManager();
            var session = mediaSessionMgr.GetCurrentSession();
            if (session == null)
                return string.Empty;
            return GetPlaybackStatus(session);
        }

        public static string GetPlaybackStatus(GlobalSystemMediaTransportControlsSession session)
        {
            return session.GetPlaybackInfo().PlaybackStatus.ToString();
        }

        public static async Task<bool> TogglePlay()
        {
            var mediaSessionMgr = await GetSystemMediaTransportControlsSessionManager();
            var session = mediaSessionMgr.GetCurrentSession();

            if (session == null)
                return false;
            switch (GetPlaybackStatus(session))
            {
                case "Paused":
                    return await session.TryPlayAsync();
                case "Playing":
                    return await session.TryPauseAsync();
                default:
                    return false;
            }
        }

        public static async Task<bool> Next()
        {
            var mediaSessionMgr = await GetSystemMediaTransportControlsSessionManager();
            var session = mediaSessionMgr.GetCurrentSession();
            if (session == null)
                return false;
            return await session.TrySkipNextAsync();
        }

        public static async Task<bool> Previous()
        {
            var mediaSessionMgr = await GetSystemMediaTransportControlsSessionManager();
            var session = mediaSessionMgr.GetCurrentSession();
            if (session == null)
                return false;
            return await session.TrySkipPreviousAsync();
        }
    }
}
