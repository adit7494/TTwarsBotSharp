using System.Net;

namespace MainCore.Parsers
{
    public static class StorageParser
    {
        private static long GetResource(HtmlDocument doc, string id)
        {
            var node = doc.GetElementbyId(id);
            if (node is null) return -1;
            return node.InnerText.ParseLong();
        }

        public static long GetWood(HtmlDocument doc) => GetResource(doc, "l1");

        public static long GetClay(HtmlDocument doc) => GetResource(doc, "l2");

        public static long GetIron(HtmlDocument doc) => GetResource(doc, "l3");

        public static long GetCrop(HtmlDocument doc) => GetResource(doc, "l4");

        public static long GetFreeCrop(HtmlDocument doc) => GetResource(doc, "stockBarFreeCrop");

        public static long GetWarehouseCapacity(HtmlDocument doc)
        {
            var stockBarNode = doc.GetElementbyId("stockBar");
            if (stockBarNode is null) return -1;
            var warehouseNode = stockBarNode.Descendants("div").FirstOrDefault(x => x.HasClass("warehouse"));
            if (warehouseNode is null) return -1;
            var capacityNode = warehouseNode.Descendants("div").FirstOrDefault(x => x.HasClass("capacity"));
            if (capacityNode is null) return -1;
            var valueNode = capacityNode.Descendants("div").FirstOrDefault(x => x.HasClass("value"));
            if (valueNode is null) return -1;
            return valueNode.InnerText.ParseLong();
        }

        public static long GetGranaryCapacity(HtmlDocument doc)
        {
            var stockBarNode = doc.GetElementbyId("stockBar");
            if (stockBarNode is null) return -1;
            var granaryNode = stockBarNode.Descendants("div").FirstOrDefault(x => x.HasClass("granary"));
            if (granaryNode is null) return -1;
            var capacityNode = granaryNode.Descendants("div").FirstOrDefault(x => x.HasClass("capacity"));
            if (capacityNode is null) return -1;
            var valueNode = capacityNode.Descendants("div").FirstOrDefault(x => x.HasClass("value"));
            if (valueNode is null) return -1;
            var valueStrFixed = WebUtility.HtmlDecode(valueNode.InnerText);
            if (string.IsNullOrEmpty(valueStrFixed)) return -1;
            return valueStrFixed.ParseLong();
        }

        /// <summary>
        /// Gets resource production rates from the production table.
        /// TTWars pages have a production table with resource rates.
        /// </summary>
        public static Dictionary<int, long> GetProductionRates(HtmlDocument doc)
        {
            var result = new Dictionary<int, long>();

            var productionTable = doc.GetElementbyId("production");
            if (productionTable is null) return result;

            var rows = productionTable.Descendants("tr").ToList();
            foreach (var row in rows)
            {
                var icon = row.Descendants("i").FirstOrDefault();
                if (icon is null) continue;

                var iconClass = icon.GetAttributeValue("class", "");
                if (!iconClass.StartsWith("r")) continue;

                // Extract resource type from class (r1=wood, r2=clay, r3=iron, r4=crop)
                if (!int.TryParse(iconClass.Substring(1), out var resourceType)) continue;

                var numCell = row.Descendants("td").FirstOrDefault(x => x.HasClass("num"));
                if (numCell is null) continue;

                var rate = numCell.InnerText.ParseLong();
                if (rate != -1)
                {
                    result[resourceType] = rate;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if the storage page has a production table (TTWars feature).
        /// </summary>
        public static bool HasProductionTable(HtmlDocument doc)
        {
            var productionTable = doc.GetElementbyId("production");
            return productionTable is not null;
        }
    }
}