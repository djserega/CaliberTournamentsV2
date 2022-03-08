﻿using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.Models.Teams
{
    internal class Player
    {
        public Player(ulong id)
        {
            DiscordUser discordUser = Bot.DiscordBot.GetDiscordUserById(id).Result;

            Name = discordUser.Username;

            UserId = discordUser.Id;
            UserName = discordUser.Username;
            Discriminator = discordUser.Discriminator;
        }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public ulong UserId { get; set; }
        [JsonProperty]
        public string UserName { get; set; }
        [JsonProperty]
        public string Discriminator { get; set; }
    }
}
