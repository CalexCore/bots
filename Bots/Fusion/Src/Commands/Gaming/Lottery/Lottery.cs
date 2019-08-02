using System;
using System.Collections.Generic;
using AngryWasp.Helpers;
using AngryWasp.Serializer;

namespace Fusion.Commands.Gaming
{
    public static class LotteryManager
    {
        private static Lottery currentGame;

        private static float jackpotTotal = 0;

        public static Lottery CurrentGame =>  currentGame;

        public static void New()
        {
            if (currentGame != null)
            {
                if (currentGame.Isjackpot)
                    jackpotTotal += currentGame.Parameters.JackpotPrize;
            }

            currentGame = Lottery.New(GameParameters.StandardGame);
            currentGame.GameFilled += (sender) =>
            {
                sender.Draw();
            };

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

        public event GameEventHandler GameFilled;
        public event GameEventHandler GameDrawn;

        private readonly object reserveLock = new object();

        public static Lottery New(GameParameters gp)
        {
            Lottery game = new Lottery();
            game.parameters = gp;
            game.numbers = new ulong[gp.TicketCount];
            game.winningNumbers = new int[5] { -1, -1, -1, -1, -1 };
            return null;
        }

        public int ReserveTickets(ulong id, int numRequested)
        {
            //bot commands are run asynchronously and multiple requests can be fulfilled simultaneously.
            //iot is therefore necessary to lock this method to prevent race conditions in fetching/modifying the collection of reserved numbers
            lock (reserveLock)
            {
                if (filled)
                return -1;

                List<int> unReserved = new List<int>();
                
                for (int i = 0; i < numbers.Length; i++)
                {
                    if (numbers[i] == 0)
                        unReserved.Add(i);
                }

                //if we have requested more than are available, we need to fix that
                int r = Math.Min(numRequested, unReserved.Count);

                for (int i = 0; i < r; i++)
                {
                    int x = MathHelper.Random.NextInt(0, unReserved.Count);
                    numbers[unReserved[x]] = id;
                    unReserved.RemoveAt(x);
                }

                if (unReserved.Count == 0)
                {
                    filled = true;
                    GameFilled?.Invoke(this);
                }

                //todo: save this game to file

                return r;
            }
            
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