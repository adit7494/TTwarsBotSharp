namespace MainCore.Parsers
{
    public static class QuestParser
    {
        public static HtmlNode? GetQuestMaster(HtmlDocument doc)
        {
            // Standard Travian
            var questmasterButton = doc.GetElementbyId("questmasterButton");
            if (questmasterButton is not null) return questmasterButton;

            // TTWars: look for quest/task related elements
            var taskButton = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("href", "").Contains("tasks"));
            return taskButton;
        }

        public static bool IsQuestClaimable(HtmlDocument doc)
        {
            // Standard Travian: check questmasterButton speech bubble
            var questmasterButton = doc.GetElementbyId("questmasterButton");
            if (questmasterButton is not null)
            {
                var newQuestSpeechBubble = questmasterButton
                    .Descendants("div")
                    .Any(x => x.HasClass("newQuestSpeechBubble"));
                if (newQuestSpeechBubble) return true;
            }

            // TTWars: check for collect buttons on task page
            var taskOverview = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("taskOverview"));
            if (taskOverview is not null)
            {
                var collectButton = taskOverview
                    .Descendants("button")
                    .Any(x => x.HasClass("collect") && !x.HasClass("disabled"));
                if (collectButton) return true;
            }

            return false;
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
            // Check for both village and general task tabs
            var isVillageTasks = doc.DocumentNode
                .Descendants("div")
                .Any(x => x.HasClass("tasks") && x.HasClass("tasksVillage"));

            var isGeneralTasks = doc.DocumentNode
                .Descendants("div")
                .Any(x => x.HasClass("tasks") && x.HasClass("tasksGeneral"));

            return isVillageTasks || isGeneralTasks;
        }

        /// <summary>
        /// Checks if the page has a quest master button.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool HasQuestMaster(HtmlDocument doc)
        {
            // Standard Travian
            var questmasterButton = doc.GetElementbyId("questmasterButton");
            if (questmasterButton is not null) return true;

            // TTWars: check for task navigation link
            var taskLink = doc.DocumentNode
                .Descendants("a")
                .Any(x => x.GetAttributeValue("href", "").Contains("tasks"));
            return taskLink;
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
