using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTelegram;
using TL;

namespace TelegramMessageRepeator
{
    internal class MsgListener
    {
        internal User TargetUser { get; private set; }
        internal HashSet<int> ListAlreadySent { get; } = new HashSet<int>();

        internal MsgListener(Core core) => this.core = core;

        private Dictionary<int, User> users = new Dictionary<int, User>();
        private DateTime runDate;
        private Core core;

        internal void Do()
        {
            OutputLastDialogsAsync().Wait();

            ChoiceTarget();

            StartInterceptAsync().Wait();
        }

        private async Task OutputLastDialogsAsync()
        {
            Messages_Dialogs dialogs = null;
            StringBuilder sb = new StringBuilder();
            int limit = 0;

            core.LogWriteLine();
            try { dialogs = await core.Auth.Acc.Messages_GetAllDialogs(); }
            catch (Exception e) { core.Shutdown($"Ошибка получения диалогов: {e.Message}"); }
            core.LogWriteLine("Список последних диалогов (до 50):");

            foreach (var user in dialogs.users)
            {
                // Пропуск ботов, деактивированных аккаунтов и самого себя
                if (user.Value.IsBot || !user.Value.IsActive || user.Key == core.Auth.Acc.UserId) continue;
                limit++;

                // Формируем строку
                sb.Append($"[#{limit}] Юзер {user.Value.first_name}");
                if (!string.IsNullOrWhiteSpace(user.Value.last_name)) sb.Append($" {user.Value.last_name}");
                if (!string.IsNullOrWhiteSpace(user.Value.MainUsername)) sb.Append($" (@{user.Value.MainUsername})");

                // Отправляем строку и очищаем её
                core.LogWriteLine(sb.ToString());
                sb.Remove(0, sb.Length);

                // Сохраняем чаты и проверяем на лимит
                users[limit] = user.Value;
                if (limit == 50) break;
            }

            // Отсутствие чатов, удовлетворяющих условиям
            if (users.Count == 0) core.Shutdown("Диалоги отсутствуют!");
        }

        private void ChoiceTarget()
        {
            core.LogWriteLine();

            while (true)
            {
                core.LogWrite($"Укажите [#номер] юзера, от 1 до {users.Count}, за которым нужно закрепить отслеживание: ");

                // Чтение номера чата
                if (!int.TryParse(core.LogReadLine(), out int target)) continue;

                // Выход за пределы
                if (target < 1 || target > users.Count) continue;

                // Закрепление чата
                TargetUser = users[target];
                break;
            }
        }

        private async Task StartInterceptAsync()
        {
            core.LogWriteLine();
            core.LogWriteLine($"Перехват сообщений от юзера \"{TargetUser.first_name}\" запущен!");
            runDate = DateTime.UtcNow;

            while (true)
            {
                try
                {
                    // Проверка сообщений
                    var history = await core.Auth.Acc.Messages_GetHistory(TargetUser);

                    // Фильтруем и преобразуем весь список
                    var idForwardMessages = history.Messages
                        .Where(x => x.From == null && !ListAlreadySent.Contains(x.ID) && x.Date > runDate)
                        .Select(x => x.ID)
                        .ToArray();

                    if (idForwardMessages.Length > 0)
                    {
                        // Генерируем список айдишек
                        List<long> idNewMessages = new List<long>();
                        while (idNewMessages.Count != idForwardMessages.Length) idNewMessages.Add(Helpers.RandomLong());

                        // Отправляем сообщения в избранное
                        await core.Auth.Acc.Messages_ForwardMessages(
                            TargetUser,
                            idForwardMessages,
                            idNewMessages.ToArray(),
                            core.Auth.Acc.User);
                        core.LogWriteLine($"Перехвачено новых сообщений: {idForwardMessages.Length} шт.");

                        // Сохраняем ID сообщений, чтобы не отправлять дубликаты
                        foreach (int id in idForwardMessages) ListAlreadySent.Add(id);
                    }
                }
                catch (Exception e) { core.LogWriteLine($"Ошибка перехвата/пересылки сообщений: {e.Message}"); }
                finally { await Task.Delay(TimeSpan.FromSeconds(core.Config.IntervalIntercept)); }
            }
        }
    }
}
