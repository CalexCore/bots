using System;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Discord;
using AwLog = AngryWasp.Logger.Log;

namespace Nerva.Bots.Helpers
{
    public static class Log
    {
        public static Task Write(string msg)
		{
			AwLog.Instance.Write(msg);
			return Task.CompletedTask;
		}

		public static Task Write(Log_Severity severity, string msg)
		{
			AwLog.Instance.Write(severity, msg);
			return Task.CompletedTask;
		}

		public static Task WriteFatalException(Exception ex)
		{
			AwLog.Instance.WriteFatalException(ex);
			return Task.CompletedTask;
		}

		public static Task WriteNonFatalException(Exception ex)
		{
			AwLog.Instance.WriteNonFatalException(ex);
			return Task.CompletedTask;
		}

		public static Task Write(LogMessage msg)
		{
			Log_Severity ls = Log_Severity.None;

			// Map Discord.net LogSeverity to Log_Severity
			switch (msg.Severity)
			{
				case LogSeverity.Info:
					ls = Log_Severity.Info;
					break;
				case LogSeverity.Warning:
					ls = Log_Severity.Warning;
					break;
				case LogSeverity.Error:
					ls = Log_Severity.Error;
					break;
				case LogSeverity.Critical:
					ls = Log_Severity.Fatal;
					break;
			}

			if (msg.Exception == null)
				AwLog.Instance.Write(msg.ToString());
			else
			{
				if (ls == Log_Severity.Fatal)
					AwLog.Instance.WriteFatalException(msg.Exception, msg.Message);
				else
					AwLog.Instance.WriteNonFatalException(msg.Exception, msg.Message);
			}

			return Task.CompletedTask;
		}
    }
}