using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using WTelegram;

namespace TelegramMessageRepeator
{
    internal class Core
    {
        static void Main(string[] args) => new Core();

        internal ConfigData Config { get; private set; }
        internal AuthAcc Auth { get; private set; }

        internal Core()
        {
            // Run info update
            MonitorAsync();
            
            // Clean up last logs and init config
            if (File.Exists(fileLog)) File.Delete(fileLog);

            Config = ConfigData.Read();
            // Turn off full logs
            if (!Config.FullLogs) Helpers.Log = (i, data) => { };

            // Try log in
            GoAuth();

            // Да-да, не забываем https://t.me/CSharpHive
            GoSubscribe();

            // Main logic
            GoListen();
        }

        private static readonly string appData = $"TelegramMessageRepeator v0.1 [https://t.me/CSharpHive]";
        private static readonly string fileLog = $"{Environment.CurrentDirectory}\\LastLogs.txt";
        private Subscription subscription;
        private MsgListener msgListener;

        private void GoAuth()
        {
            if (string.IsNullOrWhiteSpace(Config.Phone))
            {
                LogWrite("Введите номер телефона: ");
                string phone = LogReadLine();
                Config = ConfigData.Write(phone, Config.FullLogs, Config.IntervalIntercept);
            }

            Auth = new AuthAcc(this);
            Auth.SignIn();
            Config.Save();
        }

        private void GoSubscribe()
        {
            subscription = new Subscription(this);

            // Every 10 minutes
            Timer timer = new Timer(600000);
            timer.Elapsed += TimerElapsed;
            timer.Enabled = true;

            TimerElapsed(null, null);
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e) => subscription.DoAsync();

        private void GoListen()
        {
            msgListener = new MsgListener(this);
            msgListener.Do();
        }

        private async Task MonitorAsync()
        {
            while (true)
            {
                Console.Title =
                    $"{appData} Аккаунт: {Auth?.Acc?.User?.first_name} | " +
                    $"Отслеживается: {msgListener?.TargetUser?.first_name} | " +
                    $"Перехвачено: {msgListener?.ListAlreadySent.Count}";
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        internal void Shutdown(string errMsg)
        {
            Auth.Acc.Dispose();

            LogWriteLine(errMsg);
            Console.WriteLine("Нажмите любую кнопку для закрытия консоли...");
            Console.ReadKey(true);

            Environment.Exit(0);
        }

        internal string LogReadLine()
        {
            string line = Console.ReadLine();
            File.AppendAllText(fileLog, $"{line}\r\n");
            return line;
        }

        internal void LogWrite(string text)
        {
            text = $"[{DateTime.Now}] {text}";
            Console.Write(text);
            File.AppendAllText(fileLog, $"{text}");
        }

        internal void LogWriteLine()
        {
            Console.WriteLine();
            File.AppendAllText(fileLog, "\r\n");
        }

        internal void LogWriteLine(string text)
        {
            text = $"[{DateTime.Now}] {text}";
            Console.WriteLine(text);
            File.AppendAllText(fileLog, $"{text}\r\n");
        }
    }
}
