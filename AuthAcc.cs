using System;
using System.IO;
using System.Threading.Tasks;
using WTelegram;

namespace TelegramMessageRepeator
{
    internal class AuthAcc
    {
        internal Client Acc { get; private set; }

        internal AuthAcc(Core core)
        {
            this.core = core;
            Directory.CreateDirectory($@"{Environment.CurrentDirectory}\Sessions");
            sessionPath = $@"{Environment.CurrentDirectory}\Sessions\{core.Config.Phone}.session";
        }

        private static readonly string apiId = "YOUR_ID"; // Брать отсюда https://my.telegram.org/apps
        private static readonly string apiHash = "YOUR_HASH";
        private readonly string sessionPath;
        private Core core;

        internal void SignIn() => DoAsync(new Client(ConfigIn)).Wait();

        internal void SignUp() { }

        private async Task DoAsync(Client client)
        {
            try
            {
                Acc = client;
                core.LogWriteLine($"Авторизация по номеру {core.Config.Phone}...");

                await client.LoginUserIfNeeded();
                core.LogWriteLine("Успешная авторизация!");
            }
            catch (Exception e) { core.Shutdown($"Ошибка авторизации: {e.Message}"); }
        }

        private string ConfigIn(string what)
        {
            switch (what)
            {
                case "api_id": return apiId;
                case "api_hash": return apiHash;
                case "session_pathname": return sessionPath;
                case "phone_number": return core.Config.Phone;
                case "verification_code":
                    {
                        core.LogWrite("Введите код: ");
                        return core.LogReadLine();
                    }
                case "password":
                    {
                        core.LogWrite("Введите пароль: ");
                        return core.LogReadLine();
                    }
                default: return null;
            }
        }

        private string ConfigUp(string what) => null;
    }
}
