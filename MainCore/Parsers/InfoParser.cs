namespace MainCore.Parsers
{
    public static class InfoParser
    {
        public static int GetGold(HtmlDocument doc)
        {
            var goldNode = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("ajaxReplaceableGoldAmount"));
            if (goldNode is null) return -1;
            return goldNode.InnerText.ParseInt();
        }

        public static int GetSilver(HtmlDocument doc)
        {
            var silverNode = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("ajaxReplaceableSilverAmount"));
            if (silverNode is null) return -1;
            return silverNode.InnerText.ParseInt();
        }

        public static bool HasPlusAccount(HtmlDocument doc)
        {
            var boxLink = doc.GetElementbyId("sidebarBoxLinklist");
            if (boxLink is null) return false;
            var editButton = boxLink.Descendants("a").FirstOrDefault(x => x.HasClass("edit") && x.HasClass("round"));
            if (editButton is null) return false;

            if (editButton.HasClass("green")) return true;
            if (editButton.HasClass("gold")) return false;
            return false;
        }

        /// <summary>
        /// Checks if the page has account info (gold/silver).
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool HasAccountInfo(HtmlDocument doc)
        {
            var goldNode = doc.DocumentNode.Descendants("div")
                .FirstOrDefault(x => x.HasClass("ajaxReplaceableGoldAmount"));
            var silverNode = doc.DocumentNode.Descendants("div")
                .FirstOrDefault(x => x.HasClass("ajaxReplaceableSilverAmount"));

            return goldNode is not null || silverNode is not null;
        }

        /// <summary>
        /// Gets the server time from the page.
        /// TTWars pages might have a different server time element.
        /// </summary>
        public static string GetServerTime(HtmlDocument doc)
        {
            var serverTime = doc.GetElementbyId("servertime");
            if (serverTime is not null) return serverTime.InnerText.Trim();

            // TTWars: try to find time in other elements
            var timeElements = doc.DocumentNode.Descendants("span")
                .Where(x => x.HasClass("timer"));

            foreach (var timeElement in timeElements)
            {
                var value = timeElement.GetAttributeValue("value", "");
                if (!string.IsNullOrEmpty(value)) return value;
            }

            return "";
        }
    }
}