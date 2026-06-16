namespace MainCore.Parsers
{
    public static class FarmListParser
    {
        public static IEnumerable<HtmlNode> GetFarmNodes(HtmlDocument doc)
        {
            var farmListTable = doc.GetElementbyId("rallyPointFarmList");
            if (farmListTable is null) return [];

            var farmlistNodes = farmListTable
                .Descendants("div")
                .Where(x => x.HasClass("farmListHeader"));
            return farmlistNodes;
        }

        public static FarmId GetId(HtmlNode node)
        {
            var farmlistDiv = node
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("dragAndDrop"));

            if (farmlistDiv is null) return default;

            var id = farmlistDiv.GetAttributeValue("data-list", "0");
            return new FarmId(id.ParseInt());
        }

        public static string GetName(HtmlNode node)
        {
            var farmlistName = node
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("name"));
            if (farmlistName is null) return "";
            return farmlistName.InnerText.Trim();
        }

        public static HtmlNode? GetStartButton(HtmlDocument doc, FarmId raidId)
        {
            var nodes = GetFarmNodes(doc);
            foreach (var node in nodes)
            {
                var id = GetId(node);
                if (id != raidId) continue;

                var startNode = node
                    .Descendants("button")
                    .FirstOrDefault(x => x.HasClass("startFarmList"));
                if (startNode is null) continue;
                return startNode;
            }
            return null;
        }

        public static HtmlNode? GetStartAllButton(HtmlDocument doc)
        {
            // Standard Travian: button inside #rallyPointFarmList
            var farmlistTable = doc.GetElementbyId("rallyPointFarmList");
            if (farmlistTable is not null)
            {
                var startAllFarmListButton = farmlistTable
                    .Descendants("button")
                    .FirstOrDefault(x => x.HasClass("startAllFarmLists"));
                if (startAllFarmListButton is not null) return startAllFarmListButton;
            }

            // TTWars: button may be in #stickyPin (sibling of #rallyPointFarmList)
            var startAllButton = doc.DocumentNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("startAllFarmLists"));
            return startAllButton;
        }

        /// <summary>
        /// Checks if the page has a farm list.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool HasFarmList(HtmlDocument doc)
        {
            var farmListTable = doc.GetElementbyId("rallyPointFarmList");
            return farmListTable is not null;
        }

        /// <summary>
        /// Gets the number of farm lists available.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static int GetFarmListCount(HtmlDocument doc)
        {
            var farmListTable = doc.GetElementbyId("rallyPointFarmList");
            if (farmListTable is null) return 0;

            var farmlistNodes = farmListTable
                .Descendants("div")
                .Where(x => x.HasClass("farmListHeader"));

            return farmlistNodes.Count();
        }

        /// <summary>
        /// Gets all farm list IDs and names.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static IEnumerable<(FarmId Id, string Name)> GetFarmLists(HtmlDocument doc)
        {
            var nodes = GetFarmNodes(doc);
            foreach (var node in nodes)
            {
                var id = GetId(node);
                var name = GetName(node);
                yield return (id, name);
            }
        }
    }
}