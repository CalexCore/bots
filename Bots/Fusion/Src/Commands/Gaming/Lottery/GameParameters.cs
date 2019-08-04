namespace Fusion.Commands.Gaming
{
    public class GameParameters
    {
        public int TicketCount { get; set; }
        public float TicketCost { get; set; }
        public float MinorPrize { get; set; }
        public float JackpotPrize { get; set; }
        public int WinnerCount { get; set; }

        public int TimeToDraw { get; set; }

        public static readonly GameParameters StandardGame = new GameParameters
        {
            TicketCount = 100,
            TicketCost = 2.0f,
            MinorPrize = 20.0f,
            JackpotPrize = 40.0f,
            WinnerCount = 5,
            TimeToDraw = 5
        };
    }
}