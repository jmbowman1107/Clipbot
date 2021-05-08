using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Webhook;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
            _cachedClips = new List<Clip>();
            TwitchApi = new TwitchAPI();
            TwitchApi.Settings.ClientId = "5j1aae4x7qqx17shppz7tc2g9rd6fw";
            TwitchApi.Settings.Secret = "ubx843ckzgxlzwt1558wlicsf6yuir";
        }
        #endregion

        #region PostNewClips
        public async Task PostNewClips(bool useDynamoDbSettings = false)
        {
            if (string.IsNullOrWhiteSpace(_appSettings.BroadcasterId))
            {
                _logger.LogError("Broadcaster ID is not set, please set it in appsettings.json");
                return;
            }
            if (string.IsNullOrWhiteSpace(_appSettings.DiscordWebhookUrl))
            {
                _logger.LogError("Discord Webhook URL is not set, please set it in appsettings.json");
                return;
            }
            _logger.LogTrace("Getting new clips and posting them to discord.");
            if (_appSettings.LastPostedClips == null) _appSettings.LastPostedClips = new List<string>();
            await GetNewClipsAndPostToDiscord();
            UpdateApplicationSettings(useDynamoDbSettings);
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
                        if (_appSettings.LastPostedClips.All(a => a != clip.Id))
                        {
                            _logger.LogTrace($"Posting Clip {clip.Id} to Discord",JsonConvert.SerializeObject(clip));
                            await webHookClient.SendMessageAsync(text: clip.Url);
                        }
                        else
                        {
                            _logger.LogTrace($"Clip {clip.Id} has already been posted, skipping this.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error Posting Clip to Discord", clip.Id);
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
                        DateTime? endedAt = null;
                        if (_appSettings.LastReceivedClipTime != null) endedAt = DateTime.UtcNow;
                        _logger.LogTrace($"Sending message: Broadcaster: {_appSettings.BroadcasterId}, first {10}, startedAt: {_appSettings.LastReceivedClipTime.Value.ToUniversalTime()} UTC, endedAt: {endedAt} UTC");
                        newClips = await TwitchApi.Helix.Clips.GetClipsAsync(broadcasterId: _appSettings.BroadcasterId, first: 10, startedAt: _appSettings.LastReceivedClipTime, endedAt: endedAt);
                    }
                    _logger.LogTrace(JsonConvert.SerializeObject(newClips.Clips));
                    currentClips.AddRange(newClips.Clips);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving clips from Twitch.");
                }
            } while (newClips != null && !string.IsNullOrWhiteSpace(newClips.Pagination.Cursor));

            _cachedClips.AddRange(currentClips.Where(a => _cachedClips.All(b => b.Id != a.Id)));
            return currentClips;
        }
        #endregion
        #region UpdateApplicationSettings
        private void UpdateApplicationSettings(bool useDynamoDbSettings)
        {
            if (_cachedClips.Any())
            {
                _appSettings.LastReceivedClipTime = _cachedClips.Max(a => DateTime.Parse(a.CreatedAt));
                // Since we delete any cached clips older than 1 day, we need to make sure the last clip time is never older than 24 hours or else we post the same clip over and over
                if (_appSettings.LastReceivedClipTime < DateTime.UtcNow.AddDays(-1)) _appSettings.LastReceivedClipTime = DateTime.UtcNow.AddDays(-1);
            }

            var newClipsListTwo = _cachedClips.ToList();
            foreach (var clip in newClipsListTwo.Where(clip =>DateTime.UtcNow.AddDays(-1).Subtract(DateTime.Parse(clip.CreatedAt)).Days > 0))
            {
                _cachedClips.Remove(clip);
            }

            _appSettings.LastPostedClips = new List<string>();
            _appSettings.LastPostedClips.AddRange(_cachedClips.Select(a => a.Id).ToList());

            SettingsHelpers.AddOrUpdateAppSetting(_appSettings, _logger, useDynamoDbSettings);
        }
        #endregion
    }
}