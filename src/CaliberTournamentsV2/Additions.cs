using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2
{
    public static class Additions
    {
        public static ulong GetID(this string source)
        {
            return ulong.Parse(new string(source.Where(char.IsDigit).ToArray()));
        }

        public static string[] Copy(this string[] source)
        {
            string[] sourceCopy = new string[source.Length];
            Array.Copy(source, sourceCopy, source.Length);

            return sourceCopy;
        }

        public static string GetLink(this DiscordUser user)
        {
            return $"<@{user.Id}>";
        }
        public static string GetLink(this DiscordChannel user)
        {
            return $"<#{user.Id}>";
        }
        public static string GetLink(this ulong userId)
        {
            return $"<@{userId}>";
        }

        public static ulong[] GetIDs(this IConfigurationSection source)
        {
            IEnumerable<KeyValuePair<string, string>> messageIds = source.AsEnumerable().Where(el => el.Value != null);

            ulong[] _checkMessageIds = new ulong[messageIds.Count()];
            int i = 0;
            foreach (KeyValuePair<string, string> keyValueId in messageIds)
                _checkMessageIds[i++] = ulong.Parse(keyValueId.Value);

            return _checkMessageIds;
        }

        public static string[] GetStrings(this IConfigurationSection source)
        {
            IEnumerable<KeyValuePair<string, string>> messageIds = source.AsEnumerable().Where(el => el.Value != null);

            string[] _checkMessageIds = new string[messageIds.Count()];
            int i = 0;
            foreach (KeyValuePair<string, string> keyValueId in messageIds)
                _checkMessageIds[i++] = keyValueId.Value;

            return _checkMessageIds;
        }

        public static void Delete(this DiscordMessage message, int delaySecond = 10)
        {
            if (delaySecond > 0)
                Thread.Sleep(delaySecond * 1000);

            message.DeleteAsync();

            Worker.LogInf("Autodelete message");
        }
    
        public static string GetCache(this DiscordEmbed source)
        {
            StringBuilder builderCache = new();

            builderCache.Append(source.Title);
            builderCache.Append(source.Author);

            foreach (DiscordEmbedField field in source.Fields)
            {
                builderCache.Append(field.Name);
                builderCache.Append(field.Value);
            }

            return builderCache.ToString();
        }
    
        public static string GetFormattedTime(this DateTime source, string format = "HH:mm:ss")
        {
            return source.ToString(format);
        }
    }
}
