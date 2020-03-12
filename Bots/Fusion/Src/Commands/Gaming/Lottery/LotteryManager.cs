using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using AngryWasp.Helpers;
using AngryWasp.Serializer;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Rpc;
using Log = Nerva.Bots.Helpers.Log;

namespace Fusion.Commands.Gaming
{
    public static class LotteryManager
    {
        private static Lottery currentGame;

        public static Lottery CurrentGame => currentGame;

        public static void Start()
        {
            currentGame = Lottery.New(GameParameters.StandardGame, 0);
            Log.Write($"New lottery game started. Jackpot {currentGame.JackpotAmount}");
        }

        public static void Restart(float jackpot)
        {
            currentGame = Lottery.New(GameParameters.StandardGame, jackpot);
            Log.Write($"New lottery game started. Jackpot {currentGame.JackpotAmount}");
        }

        public static void Load(string path)
        {
            currentGame = new ObjectSerializer().Deserialize<Lottery>(XDocument.Load(path));
            Log.Write($"Existing lottery game loaded. Jackpot {currentGame.JackpotAmount}");
        }

        public static void ProcessResults(Lottery sender)
        {
            //todo: post results in fusion channel
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            ulong tsNow = DateTimeHelper.TimestampNow;
            //save the game in case of dispute or a problem paying out
            new ObjectSerializer().Serialize(sender, Path.Combine(cfg.DataDir, $"{tsNow}.xml"));

            var n = sender.Numbers;
            var wn = sender.WinningNumbers;

            string winnerList = "Lottery Winners: ";

            //pay minor prizes
            foreach (var w in wn)
            {
                SocketUser winner = Globals.Client.GetUser(n[w]);
                winnerList += $"{winner.Mention} ";

                RequestError err = AccountHelper.PayUser((double)sender.Parameters.MinorPrize, cfg.BotId, n[w]);
                if (err != null)
                    Sender.SendPrivateMessage(winner, $"You just won {sender.Parameters.MinorPrize}xmr in the lottery, but there was a problem with the payout. Please contact an admin and quote number `{tsNow}`");
                else
                    Sender.SendPrivateMessage(winner, $"You just won {sender.Parameters.MinorPrize}xmr in the lottery.");
            }

            winnerList += ":fireworks:";

            foreach (var i in cfg.BotChannelIds)
                ((ISocketMessageChannel)Globals.Client.GetChannel(i)).SendMessageAsync(winnerList);

            float jackpot = sender.JackpotAmount;

            foreach (var w in wn)
            {
                if (sender.JackpotNumber == w)
                {
                    RequestError err = AccountHelper.PayUser((double)sender.JackpotAmount, cfg.BotId, n[w]);
                    if (err != null)
                        Sender.SendPrivateMessage(Globals.Client.GetUser(n[w]), $"You just won the lottery jackpot of {sender.JackpotAmount}xmr, but there was a problem with the payout. Please contact an admin and quote number `{tsNow}`");
                    else
                        Sender.SendPrivateMessage(Globals.Client.GetUser(n[w]), $"You just won the lottery jackpot of {sender.JackpotAmount}xmr.");

                    jackpot = 0;
                }
            }

            Restart(jackpot);
        }
    }

    public class Lottery
    {
        [SerializerInclude]
        private GameParameters parameters;

        [SerializerInclude]
        private ulong[] numbers;

        [SerializerInclude]
        private bool filled = false;

        [SerializerInclude]
        private int[] winningNumbers;

        [SerializerInclude]
        private int jackPotNumber;

        [SerializerInclude]
        private float jackpotAmount;

        [SerializerInclude]
        private bool isJackpot = false;
        
        public bool Isjackpot => isJackpot;

        public float JackpotAmount => jackpotAmount;

        public GameParameters Parameters => parameters;

        public ulong[] Numbers => numbers;

        public int[] WinningNumbers => winningNumbers;

        public int JackpotNumber => jackPotNumber;
        
        public static Lottery New(GameParameters gp, float existingJackpot)
        {
            Lottery game = new Lottery();
            game.parameters = gp;
            game.numbers = new ulong[gp.TicketCount];
            game.winningNumbers = new int[5] { -1, -1, -1, -1, -1 };
            game.jackpotAmount = gp.JackpotPrize + existingJackpot;
            return game;
        }

        public void AllocateTickets(SocketUserMessage msg, int numRequested)
        {
            if (filled)
            {
                Sender.PublicReply(msg, "Oof. Looks like you missed out on this round.");
                return;
            }

            List<int> unAllocated = new List<int>();

            for (int i = 0; i < numbers.Length; i++)
            {
                if (numbers[i] == 0)
                    unAllocated.Add(i);
            }

            //if we have requested more than are available, we need to fix that
            int r = Math.Min(numRequested, unAllocated.Count);
            int[] allocatedNumbers = new int[r];

            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);
            RequestError err = AccountHelper.PayUser(r * LotteryManager.CurrentGame.Parameters.TicketCost, msg.Author.Id, cfg.BotId);

            if (err != null)
            {
                Sender.PublicReply(msg, $"{msg.Author.Mention} There was an error paying for your tickets.");
                return;
            }

            for (int i = 0; i < r; i++)
            {
                int x = MathHelper.Random.NextInt(0, unAllocated.Count);
                numbers[unAllocated[x]] = msg.Author.Id;
                allocatedNumbers[i] = unAllocated[x];
                unAllocated.RemoveAt(x);
            }

            if (allocatedNumbers.Length > 0)
            {
                string s = string.Empty;
                for (int i = 0; i < allocatedNumbers.Length; i++)
                    s += $"{allocatedNumbers[i]} ";

                s = s.TrimEnd();

                Sender.PublicReply(msg, $"{msg.Author.Mention} Your lucky numbers are {s}");

                //if we have allocated numbers here, we need to save this game
                //then we can persist after a restart
                new ObjectSerializer().Serialize(this, Path.Combine(cfg.DataDir, "lottery.xml"));
            }
            else
                Sender.PublicReply(msg, $"{msg.Author.Mention} I was unable to allocate you any numbers in this draw.");

            if (unAllocated.Count == 0)
            {
                filled = true;
                Sender.PublicReply(msg, $"All tickets are sold. Drawing the lottery!");
                Draw();
            }
        }

        public int GetRemainingTickets()
        {
            int x = 0;

            for (int i = 0; i < numbers.Length; i++)
                if (numbers[i] == 0)
                    ++x;
            
            return x;
        }

        public string GetUsersNumbers(ulong id)
        {
            
            string s = string.Empty;
            for (int i = 0; i < numbers.Length; i++)
            {
                if (numbers[i] == id)
                    s += $"{i} ";
            }

            return s.TrimEnd();
        }

        public void Draw()
        {
            { //first round draw
                List<int> allNumbers = new List<int>();
                for (int i = 0; i < numbers.Length; i++)
                    allNumbers.Add(i);

                for (int i = 0; i < winningNumbers.Length; i++)
                {
                    //todo: maybe there is a better rng?
                    int x = MathHelper.Random.NextInt(0, allNumbers.Count);
                    winningNumbers[i] = allNumbers[x];
                    allNumbers.RemoveAt(x);
                }
            }

            jackPotNumber = MathHelper.Random.NextInt(0, numbers.Length);

            isJackpot = false;
            foreach (var n in winningNumbers)
            {
                if (n == jackPotNumber)
                {
                    isJackpot = true;
                    break;
                }
            }

            LotteryManager.ProcessResults(this);
        }
    }
}