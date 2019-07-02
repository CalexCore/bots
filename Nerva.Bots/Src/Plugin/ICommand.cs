using System;
using Discord.WebSocket;

namespace Nerva.Bots.Plugin
{
    public sealed class CommandAttribute : Attribute
    {
        private string cmd;
        private string help;

        public string Cmd => cmd;
        public string Help => help;
        
        public CommandAttribute(string cmd, string help)
        {
            this.cmd = cmd;
            this.help = help;
        }
    }

    public interface ICommand
    {
        void Process(SocketUserMessage userMsg);
    }
}