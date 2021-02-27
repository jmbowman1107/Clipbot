using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Webhook;
using Microsoft.Extensions.Logging;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Clips.GetClips;

namespace Clipbot
{
    public class ClipPosterService
    {
        private ApplicationSettings _appSettings;
        private ILogger _logger;
        private List<Clip> _cachedClips;

        #region TwitchApi
        public TwitchAPI TwitchApi { get; set; } 
        #endregion

        #region Constructor
        public ClipPosterService(ILogger logger, ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
            _logger = logger;
            TwitchApi = new TwitchAPI();
            TwitchApi.Settings.ClientId = "5j1aae4x7qqx17shppz7tc2g9rd6fw";
            TwitchApi.Settings.Secret = "ubx843ckzgxlzwt1558wlicsf6yuir";
        }
        #endregion

        #region PostNewClips
        public async Task PostNewClips()
        {
            _logger.LogTrace("Getting new clips and posting them to discord.");
            if (_appSettings.LastPostedClips == null) _appSettings.LastPostedClips = new List<string>();
            await GetNewClipsAndPostToDiscord();
            UpdateApplicationSettings();
        } 
        #endregion

        #region GetNewClipsAndPostToDiscord
        private async Task GetNewClipsAndPostToDiscord()
        {
            using (var webHookClient = new DiscordWebhookClient(_appSettings.DiscordWebhookUrl))
            {
                foreach (var clip in (await GetNewClips()).OrderBy(a => DateTime.Parse(a.CreatedAt)))
                {
                    try
                    {
                        if (_appSettings.LastPostedClips.All(a => a != clip.Id)) await webHookClient.SendMessageAsync(text: clip.Url);
                    }
                    catch (Exception ex)
                    {
                        // TODO: Log Exception
                    }
                }
            }
        }
        #endregion
        #region GetNewClips
        private async Task<List<Clip>> GetNewClips()
        {
            var currentClips = new List<Clip>();
            GetClipsResponse newClips = null;
            do
            {
                try
                {
                    if (newClips != null && !string.IsNullOrWhiteSpace(newClips.Pagination.Cursor))
                    {
                        newClips = await TwitchApi.Helix.Clips.GetClipsAsync(broadcasterId: _appSettings.BroadcasterId,first: 10, after: newClips.Pagination.Cursor);
                    }
                    else
                    {
                        newClips = await TwitchApi.Helix.Clips.GetClipsAsync(broadcasterId: _appSettings.BroadcasterId, first: 10, startedAt: _appSettings.LastReceivedClipTime);
                    }
                }
                catch (Exception ex)
                {
                    // TODO: Log Exception
                }

                // TODO: _logger.LogDebug(JsonConvert.SerializeObject(newClips.Clips));
                currentClips.AddRange(newClips.Clips);
            } while (!string.IsNullOrWhiteSpace(newClips.Pagination.Cursor));

            _cachedClips.AddRange(currentClips);

            return currentClips;
        }
        #endregion
        #region UpdateApplicationSettings
        private void UpdateApplicationSettings()
        {
            if (_cachedClips.Any()) _appSettings.LastReceivedClipTime = _cachedClips.Max(a => DateTime.Parse(a.CreatedAt));

            var newClipsListTwo = _cachedClips.ToList();
            foreach (var clip in newClipsListTwo.Where(clip =>
                DateTime.Now.AddDays(-1).Subtract(DateTime.Parse(clip.CreatedAt)).Days > 0))
            {
                _cachedClips.Remove(clip);
            }

            _appSettings.LastPostedClips = new List<string>();
            _appSettings.LastPostedClips.AddRange(_cachedClips.Select(a => a.Id).ToList());

            SettingsHelpers.AddOrUpdateAppSetting(_appSettings);
        }
        #endregion
    }
}
