using System;
using System.Collections.Generic;
using System.Threading;
using AngryWasp.Helpers;

namespace MagellanServer
{
    public static class Application
    {
        public delegate bool CliFunc<T>(T arg);

        private static Dictionary<string, CliFunc<string[]>> commands = new Dictionary<string, CliFunc<string[]>>();

        public static void RegisterCommand(string key, CliFunc<string[]> value)
        {
            if (!commands.ContainsKey(key))
                commands.Add(key, value);
        }

        public static string[] SplitArguments(string commandLine)
        {
            var parmChars = commandLine.ToCharArray();
            var inSingleQuote = false;
            var inDoubleQuote = false;
            for (var index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    parmChars[index] = '\n';
                }
                if (parmChars[index] == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    parmChars[index] = '\n';
                }
                if (!inSingleQuote && !inDoubleQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            
            return (new string(parmChars)).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void Start()
        {
            Thread t = new Thread(new ThreadStart( () =>
            {
                List<string> lines = new List<string>();
                int lineIndex = 0, lastLineIndex = 0;
                bool noPrompt = false;
                List<char> enteredText = new List<char>();

                while (true)
                {
                    if (!noPrompt)
                        Console.Write("> ");

                    var key = Console.ReadKey();
                    noPrompt = false;
                    if (key.Key == ConsoleKey.UpArrow)
                    {
                        --lineIndex;
                        MathHelper.Clamp(ref lineIndex, 0, lines.Count - 1);

                        if (lineIndex == lastLineIndex)
                        {
                            noPrompt = true;
                            continue;
                        }

                        lastLineIndex = lineIndex;

                        Console.CursorLeft = 0;
                        Console.Write(new string(' ', Console.WindowWidth));

                        if (lineIndex < lines.Count)
                        {
                            Console.CursorLeft = 0;
                            Console.Write("> " + lines[lineIndex]);
                            noPrompt = true;
                        }

                        enteredText.Clear();
                        enteredText.AddRange(lines[lineIndex]);
                        
                        continue;
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        ++lineIndex;
                        MathHelper.Clamp(ref lineIndex, 0, lines.Count - 1);

                        if (lineIndex == lastLineIndex)
                        {
                            noPrompt = true;
                            continue;
                        }

                        lastLineIndex = lineIndex;

                        Console.CursorLeft = 0;
                        Console.Write(new string(' ', Console.WindowWidth));

                        if (lineIndex < lines.Count)
                        {
                            Console.CursorLeft = 0;
                            Console.Write("> " + lines[lineIndex]);
                            noPrompt = true;
                        }

                        enteredText.Clear();
                        enteredText.AddRange(lines[lineIndex]);
                        
                        continue;
                    }
                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        if (enteredText.Count > 0)
                        {
                            enteredText.RemoveAt(enteredText.Count - 1);
                            Console.Write("\b \b");
                        }

                        noPrompt = true;
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        string s = new string(enteredText.ToArray());
                        enteredText.Clear();
                        Console.WriteLine();
                        string[] args = SplitArguments(s);
                        if (args.Length > 0)
                        {
                            if (commands.ContainsKey(args[0]))
                            {
                                if (!commands[args[0]].Invoke(args))
                                    Console.WriteLine("Command failed");
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(s))
                            lines.Add(s);

                        lineIndex = lines.Count;
                        lastLineIndex = lineIndex;
                    }
                    else if (key.KeyChar != '\u0000')
                    {
                        enteredText.Add(key.KeyChar);
                        noPrompt = true;
                    }
                }
            }));

            t.Start();
            t.Join();
        }
    }
}