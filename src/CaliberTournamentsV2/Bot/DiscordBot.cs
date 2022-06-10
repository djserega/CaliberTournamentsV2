﻿using DSharpPlus;
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
