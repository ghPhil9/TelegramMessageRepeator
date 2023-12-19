using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace TelegramMessageRepeator
{
    public struct ConfigData
    {
        public string Phone { get; set; }
        public bool FullLogs { get; set; }
        public int IntervalIntercept { get; set; }

        private static readonly string fileConfig = $"{Environment.CurrentDirectory}\\Config.txt";

        internal static ConfigData Read()
        {
            try
            {
                if (!File.Exists(fileConfig)) throw new Exception();
                string json = File.ReadAllText(fileConfig);
                
                ConfigData config = new ConfigData();
                return (ConfigData)JsonSerializer.Deserialize(json, config.GetType());
            }
            catch { return new ConfigData() { FullLogs = false, IntervalIntercept = 3 }; }
        }

        internal static ConfigData Write(string phone, bool fullLogs, int intervalIntercept)
        {
            return new ConfigData()
            {
                Phone = phone,
                FullLogs = fullLogs,
                IntervalIntercept = intervalIntercept
            };
        }

        internal void Save()
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(fileConfig, json, Encoding.UTF8);
        }
    }
}
