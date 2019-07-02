using System.Collections.Generic;

namespace Nerva.Bots.Plugin
{
    public interface IBotConfig
    {
        //this person is the supreme overlord of the bot
        ulong OwnerId { get; }

        //Id of the bot user
        ulong BotId { get; }

        //channels this bot is permitted to post in
        List<ulong> BotChannelIds { get; }

        //Anyone in one of these roles can issue commands outside the restricted channels
        List<ulong> DevRoleIds { get; }
        
		string CmdPrefix { get; }
    }
}