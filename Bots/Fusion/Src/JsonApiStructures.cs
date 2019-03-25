using Newtonsoft.Json;

namespace Fusion
{
    [JsonObject]
    public class AccountJson
    {
        public AccountJsonItem this[int index] => Accounts[index];

        [JsonProperty("accounts")]
        public AccountJsonItem[] Accounts { get; set; }
    }

    [JsonObject]
    public class AccountJsonItem
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("display")]
        public bool Display { get; set; }
    }
}