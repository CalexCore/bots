using System.Collections.Generic;

namespace Nerva.Bots.Plugin
{
    public interface IBotConfig
    {
        //Id of the bot user
        ulong BotId { get; }

        //channels this bot is permitted to post in
        List<ulong> BotChannelIds { get; }

        //Anyone in one of these roles can issue commands outside the restricted channels
        List<ulong> DevRoleIds { get; }
        
		string CmdPrefix { get; }
    }
}