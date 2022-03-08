using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.Builders
{
    internal class ButtonModel
    {
        public ButtonModel(string label)
        {
            Label = label;
        }

        public ButtonModel(string label, bool disabled) : this(label)
        {
            Disabled = disabled;
        }

        internal string Label { get; set; }
        internal string Id { get; set; } = Guid.NewGuid().ToString();
        internal bool Disabled { get; set; }
        internal DSharpPlus.ButtonStyle Style { get; set; } = DSharpPlus.ButtonStyle.Primary;

        internal ButtonModel AddPrefixToIdButton(string prefix)
        {
            Id = prefix + Id;

            return this;
        }
        internal ButtonModel SetStyle(DSharpPlus.ButtonStyle style)
        {
            Style = style;
            return this;
        }
        internal ButtonModel SetStyle(DSharpPlus.ButtonStyle style, string id)
        {
            SetStyle(style).Id = id;
            return this;
        }

        internal DiscordButtonComponent GetComponent()
        {
            return new DiscordButtonComponent(Style, Id, Label, Disabled);
        }

        internal string GetCache()
        {
            StringBuilder builderCache = new();

            builderCache.Append(Label);
            builderCache.Append(Id);
            builderCache.Append(Disabled);

            return builderCache.ToString();
        }

    }
}
