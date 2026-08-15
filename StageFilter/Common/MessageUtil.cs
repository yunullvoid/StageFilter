using RoR2;

namespace StageFilter.Common;

internal class MessageUtil
{
    public static void SendBanMessage(string baseToken, string[] paramTokens)
    {
        Chat.SendBroadcastChat(new Chat.SimpleChatMessage
        {
            baseToken = baseToken,
            paramTokens = paramTokens
        });
    }
}
