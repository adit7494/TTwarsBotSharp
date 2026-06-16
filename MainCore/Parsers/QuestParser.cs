namespace MainCore.Parsers
{
    public static class QuestParser
    {
        public static HtmlNode GetQuestMaster(HtmlDocument doc)
        {
            var questmasterButton = doc.GetElementbyId("questmasterButton");
            return questmasterButton;
        }

        public static bool IsQuestClaimable(HtmlDocument doc)
        {
            var questmasterButton = GetQuestMaster(doc);
            if (questmasterButton is null) return false;
            var newQuestSpeechBubble = questmasterButton
                .Descendants("div")
                .Any(x => x.HasClass("newQuestSpeechBubble"));
            return newQuestSpeechBubble;
        }

        public static HtmlNode? GetQuestCollectButton(HtmlDocument doc)
        {
            var taskOverviewTable = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("taskOverview"));

            if (taskOverviewTable is null) return null;

            var collectButton = taskOverviewTable
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("collect") && !x.HasClass("disabled"));
            return collectButton;
        }

        public static bool IsQuestPage(HtmlDocument doc)
        {
            var table = doc.DocumentNode
                .Descendants("div")
                .Any(x => x.HasClass("tasks") && x.HasClass("tasksVillage"));
            return table;
        }

        /// <summary>
        /// Checks if the page has a quest master button.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool HasQuestMaster(HtmlDocument doc)
        {
            var questmasterButton = doc.GetElementbyId("questmasterButton");
            return questmasterButton is not null;
        }

        /// <summary>
        /// Gets the number of collectible quests.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static int GetCollectibleQuestCount(HtmlDocument doc)
        {
            var taskOverviewTable = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("taskOverview"));

            if (taskOverviewTable is null) return 0;

            var collectButtons = taskOverviewTable
                .Descendants("button")
                .Where(x => x.HasClass("collect") && !x.HasClass("disabled"));

            return collectButtons.Count();
        }
    }
}