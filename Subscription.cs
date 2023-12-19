using System;
using System.Linq;
using System.Threading.Tasks;
using TL;

namespace TelegramMessageRepeator
{
    internal class Subscription
    {
        internal Subscription(Core core) => this.core = core;

        private static readonly string hashLink = "VCqBoyEnJvZlZDA6";
        private static readonly long idChannel = 1687292307;
        private static readonly string usernameChannel = "CSharpHive";
        private static readonly string titleChannel = "C#Hive: Projects & Progress";
        private Core core;

        internal async Task DoAsync()
        {
            // Main join from invite link
            if (await MainJoinAsync()) return;

            // Search channel
            var channel = await SearchAsync();

            // Join
            if (channel != null) await JoinAsync(channel);
        }

        private async Task<bool> MainJoinAsync()
        {
            try
            {
                await core.Auth.Acc.Messages_ImportChatInvite(hashLink);
                return true;
            }
            catch (Exception e)
            {
                if (e.Message == "USER_ALREADY_PARTICIPANT") return true;
                return false;
            }
        }

        private async Task<Channel> SearchAsync()
        {
            try
            {
                // First try
                var search = await core.Auth.Acc.Contacts_Search(usernameChannel);
                Channel channel = ExtractTargetChannel(search);
                if (channel != null) return channel;

                // Second try
                search = await core.Auth.Acc.Contacts_Search(titleChannel);
                return ExtractTargetChannel(search);
            }
            catch { return null; }
        }

        private Channel ExtractTargetChannel(Contacts_Found contactsFound)
        {
            var chat = contactsFound.chats.Where(x => x.Key == idChannel).ToArray();
            if (chat.Length == 0) return null;
            return chat[0].Value as Channel;
        }

        private async Task<bool> JoinAsync(Channel channel)
        {
            try
            {
                await core.Auth.Acc.Channels_JoinChannel(channel);
                return true;
            }
            catch { return false; }
        }
    }
}
