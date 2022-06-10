using DSharpPlus.Entities;

namespace CaliberTournamentsV2
{
    internal class MessageQueue
    {
        private const double _intervalTime = .75 * 1000;
        private static readonly System.Timers.Timer _timerSender;
        private static readonly Queue<MessageQueue> _messages = new();

        #region Constructors

        static MessageQueue()
        {
            _timerSender = new System.Timers.Timer(_intervalTime);
            _timerSender.Elapsed += TimerSender_Elapsed;
            _timerSender.Stop();
        }
        private MessageQueue(ulong chatId, ulong? idUpdate = null, bool removerMessage = false)
        {
            ChatId = chatId;
            MessageIdUpdate = idUpdate;
            RemoveMessage = removerMessage;
        }
        private MessageQueue(ulong chatId, string? message, ulong? idUpdate = null, bool removeMessage = false) : this(chatId, idUpdate, removeMessage)
        {
            Message = message;
        }
        private MessageQueue(ulong chatId, DiscordEmbed embed, ulong? idUpdate = null, bool removeMessage = false) : this(chatId, idUpdate, removeMessage)
        {
            Embed = embed;
        }
        private MessageQueue(ulong chatId, DiscordMessageBuilder messageBuilder, ulong? idUpdate = null, bool removeMessage = false) : this(chatId, idUpdate, removeMessage)
        {
            MessageBuilder = messageBuilder;
        }
        private MessageQueue(ulong chatId, DiscordMessageBuilder messageBuilder, Action<ulong> resultActionMessage, ulong? idUpdate = null, bool removeMessage = false) : this(chatId, messageBuilder, idUpdate, removeMessage)
        {
            ResultActionMessage = resultActionMessage;
        }

        #endregion
        internal ulong ChatId { get; }
        internal ulong? MessageIdUpdate { get; }
        internal string? Message { get; }
        internal DiscordEmbed? Embed { get; }
        internal DiscordMessageBuilder? MessageBuilder { get; }
        internal bool RemoveMessage { get; }
        internal Action<ulong>? ResultActionMessage { get; }

        internal static void Add(DiscordChannel chat, string message, ulong? idUpdate = null, bool removeMessage = false)
        {
            MessageQueue messageQueue = new(chat.Id, message, idUpdate, removeMessage);
            _messages.Enqueue(messageQueue);

            if (!_timerSender.Enabled)
                _timerSender.Start();
        }
        internal static void Add(ulong chatId, string message, ulong? idUpdate = null, bool removeMessage = false)
        {
            MessageQueue messageQueue = new(chatId, message, idUpdate, removeMessage);
            _messages.Enqueue(messageQueue);

            if (!_timerSender.Enabled)
                _timerSender.Start();
        }
        internal static void Add(ulong chatId, DiscordEmbed embed, ulong? idUpdate = null)
        {
            MessageQueue messageQueue = new(chatId, embed, idUpdate);
            _messages.Enqueue(messageQueue);

            if (!_timerSender.Enabled)
                _timerSender.Start();
        }
        internal static void Add(ulong chatId, DiscordMessageBuilder messageBuilder, ulong? idUpdate = null)
        {
            MessageQueue messageQueue = new(chatId, messageBuilder, idUpdate);
            _messages.Enqueue(messageQueue);

            if (!_timerSender.Enabled)
                _timerSender.Start();
        }
        internal static void Add(ulong chatId, DiscordMessageBuilder messageBuilder, Action<ulong> resultActionMessage, ulong? idUpdate = null)
        {
            MessageQueue messageQueue = new(chatId, messageBuilder, resultActionMessage, idUpdate);
            _messages.Enqueue(messageQueue);

            if (!_timerSender.Enabled)
                _timerSender.Start();
        }

        internal static bool ChannelIsQueue(ulong chatId)
            => _messages.Any(el => el.ChatId == chatId);

        private async static void TimerSender_Elapsed(object? sender, EventArgs e)
        {
            _timerSender.Stop();

            await Bot.DiscordBot.SendQueueMessage(_messages.Dequeue());

            if (_messages.Any())
                _timerSender.Start();
        }
    }
}
