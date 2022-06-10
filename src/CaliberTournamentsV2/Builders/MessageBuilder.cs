using DSharpPlus.Entities;
using System.Text;

namespace CaliberTournamentsV2.Builders
{
    internal class MessageBuilder
    {
        private const int _maxRowId = 5;

        internal string? Description { get; set; }
        internal List<DiscordEmbed> Embeds { get; set; } = new();

        internal List<ButtonModel> Buttons { get; set; } = new List<ButtonModel>();

        internal string AddButton(string label, bool disabled, DSharpPlus.ButtonStyle style)
        {
            ButtonModel button = new(label, disabled);

            button.SetStyle(style);

            Buttons.Add(button);

            return button.Id;
        }
        internal string AddButtonPrefix(string label, bool disabled, DSharpPlus.ButtonStyle style, string prefix)
        {
            ButtonModel button = new ButtonModel(label, disabled)
                .AddPrefixToIdButton(prefix);

            button.SetStyle(style);

            Buttons.Add(button);

            return button.Id;
        }
        internal string AddButton(string label, bool disabled, DSharpPlus.ButtonStyle style, string id)
        {
            ButtonModel button = new(label, disabled)
            {
                Id = id
            };

            button.SetStyle(style);

            Buttons.Add(button);

            return button.Id;
        }

        internal MessageBuilder AddDescription(string description)
        {
            Description = description;
            return this;
        }

        internal MessageBuilder AddEmbed(DiscordEmbed embed)
        {
            Embeds.Add(embed);
            return this;
        }

        internal DiscordMessageBuilder GetMessage()
        {
            List<DiscordActionRowComponent> components = new();

            List<DiscordButtonComponent> buttons = new();

            for (int i = 0; i < Buttons.Count; i++)
            {
                if (string.IsNullOrEmpty(Buttons[i].Label))
                {
                    components.Add(new DiscordActionRowComponent(buttons.ToArray()));
                    buttons.Clear();
                    continue;
                }

                buttons.Add(Buttons[i].GetComponent());

                if (buttons.Count >= _maxRowId)
                {
                    components.Add(new DiscordActionRowComponent(buttons.ToArray()));
                    buttons.Clear();
                }
            }

            if (buttons.Count > 0)
                components.Add(new DiscordActionRowComponent(buttons.ToArray()));

            DiscordMessageBuilder builder = new DiscordMessageBuilder()
                .AddComponents(components);

            if (Embeds.Count > 0)
                foreach (DiscordEmbed embed in Embeds)
                    builder.AddEmbed(embed);

            if (!string.IsNullOrEmpty(Description))
                builder.WithContent(Description);

            return builder;
        }

        internal string GetCache()
        {
            StringBuilder builderCache = new();

            builderCache.Append(Description ?? string.Empty);
            builderCache.Append(string.Join(" ", Buttons.Select(el => el.GetCache())));

            return builderCache.ToString();
        }
    }
}
