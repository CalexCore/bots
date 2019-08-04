using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using AngryWasp.Serializer;
using Discord.WebSocket;

namespace Fusion.Commands.Gaming
{
    public static class LotteryManager
    {
        private static Lottery currentGame;

        private static float jackpotTotal = 0;

        public static Lottery CurrentGame => currentGame;

        public static void New()
        {
            if (currentGame != null)
            {
                if (currentGame.Isjackpot)
                    jackpotTotal += currentGame.Parameters.JackpotPrize;
            }

            currentGame = Lottery.New(GameParameters.StandardGame);

            currentGame.GameDrawn += (sender) =>
            {
                //todo: payout first round winners
                //todo: add current game jackpot amount to jackpot and pay out if won
                //todo: notify winners of their success
                //todo: post results in fusion channel
            };
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
        private bool isJackpot = false;

        public bool Isjackpot => isJackpot;

        public GameParameters Parameters => parameters;

        public delegate void GameEventHandler(Lottery sender);

        public event GameEventHandler GameDrawn;

        public static Lottery New(GameParameters gp)
        {
            Lottery game = new Lottery();
            game.parameters = gp;
            game.numbers = new ulong[gp.TicketCount];
            game.winningNumbers = new int[5] { -1, -1, -1, -1, -1 };
            return null;
        }

        public async Task<int[]> AllocateTickets(SocketUserMessage msg, int numRequested)
        {
            if (filled)
            {
                await Sender.PublicReply(msg, "Oof. Looks like you missed out on this round.");
                return null;
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
                for (int i = 0; i < allocatedNumbers.Length - 1; i++)
                    s += $"{i}, ";

                s += $"{allocatedNumbers[allocatedNumbers.Length - 1]}";

                await Sender.PublicReply(msg, $"{msg.Author.Mention} Your lucky numbers are {s}");
            }
            else
                await Sender.PublicReply(msg, $"{msg.Author.Mention} I was unable to allocate you any numbers in this draw.");

            if (unAllocated.Count == 0)
            {
                filled = true;
                await Sender.PublicReply(msg, $"All tickets are sold and the draw will commence in {parameters.TimeToDraw} minutes");
                await Task.Delay(1000 * 60 * parameters.TimeToDraw);
                Draw();
            }

            //todo: save this game to file

            return allocatedNumbers;
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

            GameDrawn?.Invoke(this);
        }
    }
}