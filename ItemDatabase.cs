using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace RoR2ItemInfo
{
    public class StatEntry {
        [JsonProperty("stat")]  public string Stat  = null!;
        [JsonProperty("value")] public string Value = null!;
        [JsonProperty("stack")] public string Stack = null!;
        [JsonProperty("add")]   public string Add   = null!;
    }

    public class ItemData {
        [JsonProperty("name")]        public string Name        = null!;
        [JsonProperty("item_id")]     public string ItemId      = null!;
        [JsonProperty("rarity")]      public string Rarity      = null!;
        [JsonProperty("category")]    public string Category    = null!;
        [JsonProperty("caption")]     public string Caption     = null!;
        [JsonProperty("description")] public string Description = null!;
        [JsonProperty("token")]       public string Token       = null!;
        [JsonProperty("desc_token")]  public string DescToken   = null!;
        [JsonProperty("stats")]       public List<StatEntry> Stats = null!;
    }

    public static class ItemDatabase
    {
        public static Dictionary<string, ItemData> ById    = new(); // item_id
        public static Dictionary<string, ItemData> ByToken = new(); // ITEM_XXX_NAME
        public static Dictionary<string, ItemData> ByName  = new(); // display name

        public static void Load()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("TooManyItems.ror2_items.json");
            using var reader = new StreamReader(stream);
            var items = JsonConvert.DeserializeObject<List<ItemData>>(reader.ReadToEnd());
            if (items == null) return;

            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.ItemId))  ById[item.ItemId]   = item;
                if (!string.IsNullOrEmpty(item.Token))   ByToken[item.Token] = item;
                if (!string.IsNullOrEmpty(item.Name))    ByName[item.Name]   = item;
            }

            Plugin.Log.LogInfo($"ItemDatabase: loaded {ById.Count} items.");
        }
    }
}