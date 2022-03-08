using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2
{
    internal class Access
    {
        private static ulong[] _adminUsers = Array.Empty<ulong>();
        private static ulong[] _refereeUsers = Array.Empty<ulong>();

        internal Access(IConfigurationRoot? config)
        {
            try
            {
                _adminUsers = config?.GetSection("admins").GetIDs() ?? Array.Empty<ulong>();

                if (_adminUsers.Length == 0)
                    Worker.LogWarn("Не найдена (не заполнена) группа админ доступов");
                else
                    Worker.LogInf($"Админы: {string.Join(", ", _adminUsers)}");

                _refereeUsers = config?.GetSection("referee").GetIDs() ?? Array.Empty<ulong>();

                if (_refereeUsers.Length == 0)
                    Worker.LogWarn("Не найдена (не заполнена) группа доступов для судей");
                else
                    Worker.LogInf($"Судьи: {string.Join(", ", _refereeUsers)}");
            }
            catch (Exception ex)
            {
                throw new InitException("Не удалось загрузить базовые параметры настроек", ex);
            }
        }

        internal static void RegisterReferee(ulong id)
        {
            ulong[] refereeUsers = new ulong[_refereeUsers.Length + 1];

            int i = 0;
            for (; i < _refereeUsers.Length; i++)
                refereeUsers[i] = _refereeUsers[i];

            refereeUsers[i] = id;

            _refereeUsers = refereeUsers;

            foreach (var item in _refereeUsers)
                Console.WriteLine(item);
        }

        internal static bool IsAdmin(DiscordUser user, string detailed = "")
        {
            bool result = _adminUsers.Any(el => el == user.Id);

            if (!result)
                SendErrorAccess(user, detailed);

            return result;
        }
        internal static bool IsReferee(ulong userId)
            => _refereeUsers.Any(el => el == userId);
        internal static bool IsReferee(DiscordUser user, string detailed = "")
        {
            bool result = IsReferee(user.Id);

            if (!result)
                SendErrorAccess(user, detailed);

            return result;
        }
       
        private static void SendErrorAccess(DiscordUser user, string detailed)
        {
            string prefix = string.IsNullOrEmpty(detailed) ? "" : $"{detailed}. ";
            Worker.LogErr($"{prefix}Ошибка доступа. {user.Username}#{user.Discriminator}");
        }
    }
}
