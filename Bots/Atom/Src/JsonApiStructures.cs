using System.Collections.Generic;
using Newtonsoft.Json;

namespace Atom
{
    [JsonObject]
    public class JsonResult<T>
    {
        [JsonProperty("result")]
        public T Result { get; set; }
    }

    [JsonObject]
    public class NodeInfo
    {
        [JsonProperty("target_height")]
        public int TargetHeight { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("top_block_hash")]
        public string TopBlockHash { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("incoming_connections_count")]
        public int IncomingConnections { get; set; }

        [JsonProperty("outgoing_connections_count")]
        public int OutgoingConnections { get; set; }

        [JsonProperty("difficulty")]
        public int Difficulty { get; set; }
    }

    [JsonObject]
    public class GetBlockCount 
    {
        [JsonProperty("count")]
        public ulong Count { get; set; }
    }

    [JsonObject]
    public class GetGeneratedCoins
    {
        [JsonProperty("coins")]
        public ulong Coins { get; set; }
    }

    [JsonObject]
    public class MarketInfo
    {
        [JsonProperty("bid")]
        public double Bid { get; set; }

        [JsonProperty("ask")]
        public double Ask { get; set; }

        [JsonProperty("volume")]
        public double Volume { get; set; }

        [JsonProperty("high")]
        public double High { get; set; }

        [JsonProperty("low")]
        public double Low { get; set; }
    }

    [JsonObject]
    public class CoinGeckoInfo
    {
        [JsonProperty("market_cap_rank")]
        public int MarketCapRank { get; set; }

        [JsonProperty("coingecko_rank")]
        public int CoinGeckoRank { get; set; }

        [JsonProperty("coingecko_score")]
        public double CoinGeckoScore { get; set; }

        [JsonProperty("developer_score")]
        public double DeveloperScore { get; set; }

        [JsonProperty("community_score")]
        public double CommunityScore { get; set; }

        [JsonProperty("public_interest_score")]
        public double PublicInterestScore { get; set; }

        [JsonProperty("market_data")]
        public CoinGeckoMarketData MarketData { get; set; }
    }

    [JsonObject]
    public class CoinGeckoMarketData
    {
        [JsonProperty("current_price")]
        public CoinGeckoPrice CurrentPrice { get; set; }

        [JsonProperty("market_cap")]
        public CoinGeckoPrice MarketCap { get; set; }

        [JsonProperty("total_volume")]
        public CoinGeckoPrice TotalVolume { get; set; }

        [JsonProperty("high_24h")]
        public CoinGeckoPrice High24h { get; set; }

        [JsonProperty("low_24h")]
        public CoinGeckoPrice Low24h { get; set; }

        [JsonProperty("price_change_percentage_24h")]
        public double PercentageChange { get; set; }

        [JsonProperty("price_change_24h")]
        public double PriceChange { get; set; }
    }

    [JsonObject]
    public class CoinGeckoPrice
    {
        [JsonProperty("btc")]
        public double BTC { get; set; }

        [JsonProperty("usd")]
        public double USD { get; set; }

        [JsonProperty("aud")]
        public double AUD { get; set; }
    }

    [JsonObject]
    public class LinkData
    {
        [JsonProperty("cli_version")]
        public string CliVersion { get; set; }

        [JsonProperty("gui_version")]
        public string GuiVersion { get; set; }

        [JsonProperty("binary_url")]
        public string BinaryUrl { get; set; }

        [JsonProperty("windows")]
        public string WindowsLink { get; set; }

        [JsonProperty("linux")]
        public string LinuxLink { get; set; }

        [JsonProperty("mac")]
        public string MacLink { get; set; }

        [JsonProperty("linux_gui")]
        public string LinuxGuiLink { get; set; }

        [JsonProperty("mac_gui")]
        public string MacGuiLink { get; set; }

        [JsonProperty("windows_gui")]
        public string WindowsGuiLink { get; set; }

        [JsonProperty("bootstrap")]
        public string[] BootstrapFiles { get; set; }
    }

    [JsonObject]
    public class BanList
    {
        [JsonProperty("bans")]
        public List<BanListItem> Bans { get; set; }
    }

    [JsonObject]
    public class BanListItem
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("ban")]
        public bool Banned { get; set; } = true;

        [JsonProperty("seconds")]
        public uint Seconds => 6000;

    }
}