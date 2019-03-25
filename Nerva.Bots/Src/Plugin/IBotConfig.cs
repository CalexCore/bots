namespace Nerva.Bots.Plugin
{
    public interface IBotConfig
    {
        ulong OwnerID { get; }
        ulong ServerID { get; }
        ulong BotID { get; }
        ulong BotChannelID { get; }
		string CmdPrefix { get; }
    }
}