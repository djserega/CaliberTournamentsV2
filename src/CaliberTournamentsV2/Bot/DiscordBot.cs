using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace CaliberTournamentsV2.Bot
{
    internal partial class DiscordBot
    {
        private static DiscordClient? _discord;

        internal bool DiscordBotInitialized { get; private set; }

        internal DiscordBot(IConfigurationRoot? config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            try
            {
                Console.WriteLine("Building discord client...");

                _discord = new DiscordClient(new DiscordConfiguration
                {
                    Token = config.GetValue<string>("discord:token"),
                    TokenType = TokenType.Bot
                });
            }
            catch (Exception ex)
            {
                throw new InitException("Error. Discort client not initialized.", ex);
            }

            InitCommands(config);

            _discord!.ComponentInteractionCreated += Discord_ComponentInteractionCreated;

            _discord!.ConnectAsync();
        }

        private async Task Discord_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            if (MessageQueue.ChannelIsQueue(e.Channel.Id))
                return;

            if (e.Id.StartsWith("removeVoting_"))
            {
                if (e.Id.Contains("_rejected"))
                {
                    Builders.MessageBuilder builder = new();

                    Builders.Embeds embeds = new Builders.Embeds()
                        .Init();

                    string newDescription = string.Empty;
                    if (e.Message.Embeds.Count > 0)
                        newDescription = e.Message.Embeds.First().Description;

                    embeds.AddDescription($"{newDescription}\n\nУдаление голосования отменено пользователем {Formatter.Bold(e.User.Username)}");

                    builder.AddEmbed(embeds.GetEmbed());

                    await e.Message.ModifyAsync(builder.GetMessage());
                }
                else
                {
                    Builders.MessageBuilder builder = new();
                    string newDescription = string.Empty;
                    if (e.Message.Embeds.Count > 0)
                        newDescription = e.Message.Embeds[0].Description;

                    try
                    {
                        string idWithoutPrefix = e.Id.TrimStart("removeVoting_".ToCharArray());

                        int indexDelimeterStart = idWithoutPrefix.IndexOf('_');
                        int indexDelimeterEnd = idWithoutPrefix.IndexOf('_', indexDelimeterStart + 1);

                        Models.Referee.StartPickBan pickBan = Models.Referee.StartPickBan.ListPickBans.First(el => el.Id == int.Parse(idWithoutPrefix[..indexDelimeterStart]));

                        Models.PickBans.PickBanDetailed pickBanDetailed = pickBan.PickBanMap.PickBanDetailed.First(el => el.Id == int.Parse(idWithoutPrefix[(indexDelimeterStart + 1)..indexDelimeterEnd]));

                        pickBanDetailed.Operators.DetailedIds.Clear();
                        pickBanDetailed.Operators.PickBanDetailed.Clear();
                        pickBanDetailed.Operators.TeamOperators.Clear();

                        if (pickBan.Team1 != null && pickBan.Team2 != null)
                            Models.Referee.StartPickBan.AddDetails(pickBanDetailed.Operators, pickBan.Team1, pickBan.Team2);

                        pickBanDetailed.Operators.DateEnd = default;

                        Builders.Embeds embeds = new Builders.Embeds()
                            .Init();

                        embeds.AddDescription($"{newDescription}\n\nПользователь {Formatter.Bold(e.User.Username)} подтвердил удаление голосования");

                        builder.AddEmbed(embeds.GetEmbed());

                    }
                    catch (Exception ex)
                    {
                        Worker.LogErr(ex.ToString());
                        newDescription += "\n\nУпс... Не удалось удалить голосование...";
                    }

                    await e.Message.ModifyAsync(builder.GetMessage());
                }
            }
            else
                await Task.Run(() => DataHandlers.DataHandler.ReceivedPickBanMessagesEvent(e.Channel, e.Message, e.User.Id, e.Id));

            try { await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate); }
            catch { }
        }

        internal static async Task<DiscordMessage?> SendQueueMessage(MessageQueue message)
        {
            if (_discord == null)
                return default;

            if (message == null)
                return default;

            DiscordChannel channel = await _discord.GetChannelAsync(message.ChatId);

            DiscordMessage? discordMessage = default;

            try
            {
                if (message.Embed != null)
                {
                    if (message.MessageIdUpdate == null)
                        discordMessage = await _discord.SendMessageAsync(channel, message.Embed);
                    else
                    {
                        discordMessage = await channel.GetMessageAsync((ulong)message.MessageIdUpdate);
                        await discordMessage.ModifyAsync(message.Embed);
                    }
                }
                else if (message.Message != null)
                {
                    if (message.MessageIdUpdate == null)
                        discordMessage = await _discord.SendMessageAsync(channel, message.Message);
                    else
                    {
                        discordMessage = await channel.GetMessageAsync((ulong)message.MessageIdUpdate);
                        await discordMessage.ModifyAsync(message.Message);
                    }
                }
                else if (message.MessageBuilder != null)
                {
                    if (message.MessageIdUpdate == null)
                    {
                        discordMessage = await _discord.SendMessageAsync(channel, message.MessageBuilder);

                        if (message.ResultActionMessage != null)
                            message.ResultActionMessage.Invoke(discordMessage.Id);
                    }
                    else
                    {
                        discordMessage = await channel.GetMessageAsync((ulong)message.MessageIdUpdate);
                        await discordMessage.ModifyAsync(message.MessageBuilder);
                    }
                }

                Worker.LogInf($"Sended message from queur in channel: {channel.Name} ({channel.Id}) :: message ({discordMessage?.Id}) {discordMessage?.Content}");

                if (message.RemoveMessage)
                    discordMessage?.Delete();
            }
            catch (Exception ex)
            {
                Worker.LogErr("Message not sended from queue\n" + ex);
            }

            return discordMessage;
        }

        internal static async Task<ulong> SendMessage(ulong idChannel, string text)
        {
            if (_discord == null)
                return default;

            DiscordChannel channel = await _discord.GetChannelAsync(idChannel);

            DiscordMessage message = await _discord.SendMessageAsync(channel, text);

            return message.Id;
        }

        internal static async Task<DiscordUser> GetDiscordUserById(ulong id)
        {
            return await _discord!.GetUserAsync(id);
        }

    }
}
