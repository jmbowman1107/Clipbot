using System;
using System.Collections.Generic;

namespace Clipbot
{
    public class ApplicationSettings
    {
        public string BroadcasterId { get; set; }
        public string DiscordWebhookUrl { get; set; }
        public int ClipPollCycle { get; set; }
        public DateTime? LastReceivedClipTime { get; set; }
        public List<string> LastPostedClips { get; set; } 
    }
}
