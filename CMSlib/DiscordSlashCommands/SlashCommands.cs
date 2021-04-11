using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSlib.DiscordSlashCommands
{
    public class DiscordSlashInteraction
    {
        public int version { get; set; }
        public int type { get; set; }
        public string token { get; set; }
        public DiscordSlashMember member { get; set; }
        public string id { get; set; }
        public string guild_id { get; set; }
        public DiscordSlashCommandData data { get; set; }
        public string channel_id { get; set; }

    }
    public class DiscordSlashCommandData
    {
        public string name { get; set; }
        public string id { get; set; }
        public DiscordSlashCommandDataOption[] options { get; set; }

    }
    public class DiscordSlashMember
    {
        public DiscordSlashUser user { get; set; }
        public string nick { get; set; }
        public string[] roles { get; set; }
        public string joined_at { get; set; }
        public string premium_since { get; set; }
        public bool deaf { get; set; }
        public bool mute { get; set; }
        public bool? is_pending { get; set; }
        public string permissions { get; set; }
    }
    public class DiscordSlashUser
    {
        public string id { get; set; }
        public string username { get; set; }
        public string discriminator { get; set; }
        public string avatar { get; set; }
        public bool? bot { get; set; }
        public bool? system { get; set; }
        public bool? mfa_enabled { get; set; }
        public string locale { get; set; }
        public bool? verified { get; set; }
        public string email { get; set; }
        public int flags { get; set; }
        public int premium_type { get; set; }
        public int public_flags { get; set; }

    }
    public class DiscordSlashResponse
    {
        public int type { get; set; }
        public DiscordSlashResponseData data { get; set; }
    }
    public class DiscordSlashCommandDataOption
    {
        public string name { get; set; }
        public string value { get; set; }
    }
    public class DiscordSlashResponseData
    {
        public string content { get; set; }
        public int flags { get; set; }
    }
    public class DiscordSlashCommand
    {
        public string name { get; set; }
        public string description { get; set; }
        public DiscordSlashCommandOption[] options { get; set; }
    }
    public class DiscordSlashCommandOption
    {
        public int type { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool required { get; set; }
        public DiscordSlashCommandOptionChoiceString[] choices { get; set; }

    }
    public class DiscordSlashCommandOptionChoice
    {
        public string name { get; set; }
    }
    public class DiscordSlashCommandOptionChoiceInt : DiscordSlashCommandOptionChoice
    {
        public int value { get; set; }
    }
    public class DiscordSlashCommandOptionChoiceString : DiscordSlashCommandOptionChoice
    {
        public string value { get; set; }

    }

}
