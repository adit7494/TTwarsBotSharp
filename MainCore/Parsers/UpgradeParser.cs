namespace MainCore.Parsers
{
    public static class UpgradeParser
    {
        private static HtmlNode GetContractNode(HtmlDocument doc, BuildingEnums building)
        {
            var node = doc.GetElementbyId($"contract_building{(int)building}"); // building
            node ??= doc.GetElementbyId("contract"); // site
            return node;
        }

        public static List<HtmlNode> GetRequiredResource(HtmlDocument doc, BuildingEnums building)
        {
            var node = GetContractNode(doc, building);

            if (node is null) return [];
            var resourceWrapper = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("resourceWrapper"));
            if (resourceWrapper is null) return [];

            var resources = resourceWrapper.Descendants("div")
                .Where(x => x.HasClass("resource"))
                .ToList();

            if (resources.Count != 5) return [];
            return resources;
        }

        public static TimeSpan GetTimeWhenEnoughResource(HtmlDocument doc, BuildingEnums building)
        {
            var node = GetContractNode(doc, building);

            if (node is null) return TimeSpan.Zero;

            var errorMessage = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("errorMessage"));
            if (errorMessage is null) return TimeSpan.Zero;
            var timer = errorMessage.Descendants("span")
                .FirstOrDefault(x => x.HasClass("timer"));
            if (timer is null) return TimeSpan.Zero;
            var time = timer.GetAttributeValue("value", 0);
            return TimeSpan.FromSeconds(time);
        }

        public static HtmlNode? GetConstructButton(HtmlDocument doc, BuildingEnums building)
        {
            if (building.IsResourceField()) return GetUpgradeButton(doc);

            var contract_building = doc.GetElementbyId($"contract_building{(int)building}");
            if (contract_building is null) return null;

            // Standard Travian: button with class "new"
            var button = contract_building
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("new"));
            if (button is not null) return button;

            // TTWars: button with class "contractLink" and "build"
            button = contract_building
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("contractLink") && x.HasClass("build"));
            return button;
        }

        public static HtmlNode? GetSpecialUpgradeButton(HtmlDocument doc)
        {
            var upgradeButtonsContainer = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            if (upgradeButtonsContainer is null) return null;

            var button = upgradeButtonsContainer
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("videoFeatureButton") && x.HasClass("green"));
            return button;
        }

        public static HtmlNode? GetUpgradeButton(HtmlDocument doc)
        {
            var upgradeButtonsContainer = doc.DocumentNode.Descendants("div")
               .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            if (upgradeButtonsContainer is null) return null;

            var button = upgradeButtonsContainer.Descendants("button")
                .FirstOrDefault(x => x.HasClass("build"));
            return button;
        }

        /// <summary>
        /// Gets the upgrade button for TTWars pages.
        /// TTWars uses a different button structure with "contractLink" class.
        /// </summary>
        public static HtmlNode? GetTTWarsUpgradeButton(HtmlDocument doc)
        {
            var upgradeButtonsContainer = doc.DocumentNode.Descendants("div")
               .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            if (upgradeButtonsContainer is null) return null;

            // TTWars: button with "contractLink" and "build" classes
            var button = upgradeButtonsContainer.Descendants("button")
                .FirstOrDefault(x => x.HasClass("contractLink") && x.HasClass("build"));

            // Fallback to standard "build" class
            button ??= upgradeButtonsContainer.Descendants("button")
                .FirstOrDefault(x => x.HasClass("build"));

            return button;
        }

        /// <summary>
        /// Checks if the page has an upgrade buttons container.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool HasUpgradeButtons(HtmlDocument doc)
        {
            var upgradeButtonsContainer = doc.DocumentNode.Descendants("div")
               .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            return upgradeButtonsContainer is not null;
        }

        /// <summary>
        /// Gets the build duration from the upgrade section.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static TimeSpan GetBuildDuration(HtmlDocument doc)
        {
            var upgradeButtonsContainer = doc.DocumentNode.Descendants("div")
               .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            if (upgradeButtonsContainer is null) return TimeSpan.Zero;

            var durationDiv = upgradeButtonsContainer.Descendants("div")
                .FirstOrDefault(x => x.HasClass("duration"));
            if (durationDiv is null) return TimeSpan.Zero;

            var valueSpan = durationDiv.Descendants("span")
                .FirstOrDefault(x => x.HasClass("value"));
            if (valueSpan is null) return TimeSpan.Zero;

            return valueSpan.InnerText.Trim().ToDuration();
        }
    }
}