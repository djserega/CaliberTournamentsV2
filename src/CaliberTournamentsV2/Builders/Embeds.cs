using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.Builders
{
    internal class Embeds
    {
        private DiscordEmbedBuilder? _embedBuilder;

        internal int CountFields { get; private set; }

        internal Embeds Init(string author = "Caliber", string color = "#b81831")
        {
            _embedBuilder = new DiscordEmbedBuilder
            {
                Timestamp = DateTime.Now,
                Color = new DiscordColor(color)
            };

            if (!string.IsNullOrWhiteSpace(author))
                _embedBuilder.Author = new DiscordEmbedBuilder.EmbedAuthor() { Name = author };

            Title = string.Empty;
            Description = string.Empty;
            _embedBuilder.ClearFields();

            return this;
        }
        internal string Title { set { if (_embedBuilder == null) Init(); _embedBuilder!.Title = value; } }
        internal string Description { set { if (_embedBuilder == null) Init(); _embedBuilder!.Description = value; } }

        internal Embeds AddTitle(string title)
        {
            Title = title;
            return this;
        }

        internal Embeds AddDescription(string description)
        {
            Description = description;
            return this;
        }

        internal Embeds AddImage(string src)
        {
            _embedBuilder!.ImageUrl = src;

            return this;
        }

        internal Embeds AddField(string name, string value, bool inline = false)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _embedBuilder!.AddField(name, value, inline);
                CountFields++;
            }

            return this;
        }
        internal Embeds AddField(string name, List<string> listValue, bool inline = false)
        {
            _embedBuilder!.AddField(name, string.Join("\n", listValue), inline);
            CountFields++;

            return this;
        }

        internal void ClearFiels()
        {
            _embedBuilder!.ClearFields();
            CountFields = 0;
        }

        internal DiscordEmbed GetEmbed()
            => _embedBuilder!.Build();
    }
}
