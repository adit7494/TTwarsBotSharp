namespace MainCore.Parsers
{
    public static class AdventureParser
    {
        /// <summary>
        /// Gets the adventure timer duration.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static TimeSpan GetAdventureDuration(HtmlDocument doc)
        {
            var heroAdventure = doc.GetElementbyId("heroAdventure");
            if (heroAdventure is null) return TimeSpan.Zero;

            // Standard Travian: span.timer with value attribute
            var timer = heroAdventure
                .Descendants("span")
                .FirstOrDefault(x => x.HasClass("timer"));
            if (timer is not null)
            {
                var seconds = timer.GetAttributeValue("value", 0);
                if (seconds > 0) return TimeSpan.FromSeconds(seconds);
            }

            // TTWars: div.duration with text like "00:00:02"
            var durationDiv = heroAdventure
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("duration"));
            if (durationDiv is not null)
            {
                var durationText = durationDiv.InnerText.Trim();
                if (TimeSpan.TryParse(durationText, out var duration))
                {
                    return duration;
                }
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// Checks if the current page is an adventure page.
        /// Handles both standard Travian (table.adventureList) and TTWars (table.borderGap.adventureList).
        /// </summary>
        public static bool IsAdventurePage(HtmlDocument doc)
        {
            // Standard Travian: table.adventureList
            var table = doc.DocumentNode
                .Descendants("table")
                .Any(x => x.HasClass("adventureList"));
            if (table) return true;

            // TTWars: table.borderGap.adventureList
            table = doc.DocumentNode
                .Descendants("table")
                .Any(x => x.HasClass("borderGap") && x.HasClass("adventureList"));
            if (table) return true;

            // TTWars: check for heroAdventure content class
            var contentNode = doc.GetElementbyId("content");
            if (contentNode is not null)
            {
                var contentClass = contentNode.GetAttributeValue("class", "");
                if (contentClass.Contains("heroAdventure")) return true;
            }

            // TTWars: check for #heroAdventure element (React-rendered adventure container)
            var heroAdventureDiv = doc.GetElementbyId("heroAdventure");
            if (heroAdventureDiv is not null) return true;

            // TTWars: check for any button with data-mapid (adventure start button)
            var adventureButton = doc.DocumentNode
                .Descendants("button")
                .FirstOrDefault(x => x.GetAttributeValue("data-mapid", "") != "");
            if (adventureButton is not null) return true;

            return false;
        }

        /// <summary>
        /// Gets the hero adventure button from the sidebar.
        /// Handles both standard Travian (a.adventure.round) and TTWars (div.heroState).
        /// </summary>
        public static HtmlNode? GetHeroAdventureButton(HtmlDocument doc)
        {
            // Standard Travian: a.adventure.round
            var adventureButton = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.HasClass("adventure") && x.HasClass("round"));
            if (adventureButton is not null) return adventureButton;

            // TTWars: look for adventure link anywhere on the page
            var adventureLink = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("href", "").Contains("hero_adventures"));
            if (adventureLink is not null) return adventureLink;

            // TTWars: check for adventure tab/button (singular)
            var adventureTab = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("href", "").Contains("hero_adventure"));
            if (adventureTab is not null) return adventureTab;

            // TTWars: look for any element with class containing "adventure"
            var adventureElement = doc.DocumentNode
                .Descendants()
                .FirstOrDefault(x => x.HasClass("adventure"));
            if (adventureElement is not null) return adventureElement;

            return null;
        }

        /// <summary>
        /// Checks if the hero can start an adventure.
        /// Handles both standard Travian and TTWars.
        /// </summary>
        public static bool CanStartAdventure(HtmlDocument doc)
        {
            // Standard Travian: div.heroStatus > i.heroHome
            var heroStatus = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("heroStatus"));
            if (heroStatus is not null)
            {
                var heroHome = heroStatus.Descendants("i")
                    .Any(x => x.HasClass("heroHome"));
                if (heroHome)
                {
                    var adventureButton = GetHeroAdventureButton(doc);
                    if (adventureButton is not null)
                    {
                        var adventureAvailable = adventureButton.Descendants("div")
                            .Any(x => x.HasClass("content"));
                        return adventureAvailable;
                    }
                }
            }

            // TTWars: div.heroState > i.statusHome_medium
            var heroState = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("heroState"));
            if (heroState is not null)
            {
                var heroHome = heroState.Descendants("i")
                    .Any(x => x.HasClass("statusHome_medium"));
                return heroHome;
            }

            return false;
        }

        /// <summary>
        /// Gets the adventure start button.
        /// Handles both standard Travian and TTWars.
        /// </summary>
        public static HtmlNode? GetAdventureButton(HtmlDocument doc)
        {
            // Standard Travian: #heroAdventure > tbody > tr > button
            var adventureTable = doc.GetElementbyId("heroAdventure");
            if (adventureTable is not null)
            {
                var adventureTableBody = adventureTable
                    .Descendants("tbody")
                    .FirstOrDefault();
                if (adventureTableBody is not null)
                {
                    var adventureTableBodyRow = adventureTableBody
                        .Descendants("tr")
                        .FirstOrDefault();
                    if (adventureTableBodyRow is not null)
                    {
                        var startAdventureButton = adventureTableBodyRow
                            .Descendants("button")
                            .FirstOrDefault();
                        if (startAdventureButton is not null) return startAdventureButton;
                    }
                }
            }

            // TTWars: table.adventureList > tbody > tr > button (with data-mapid)
            var adventureList = doc.DocumentNode
                .Descendants("table")
                .FirstOrDefault(x => x.HasClass("adventureList"));
            if (adventureList is not null)
            {
                var tbody = adventureList.Descendants("tbody").FirstOrDefault();
                if (tbody is not null)
                {
                    var firstRow = tbody.Descendants("tr").FirstOrDefault();
                    if (firstRow is not null)
                    {
                        var button = firstRow.Descendants("button")
                            .FirstOrDefault(x => x.HasClass("green"));
                        if (button is not null) return button;

                        // TTWars fallback: any button with data-mapid in the first row
                        button = firstRow.Descendants("button")
                            .FirstOrDefault(x => x.GetAttributeValue("data-mapid", "") != "");
                        if (button is not null) return button;
                    }
                }
            }

            // TTWars fallback: search for any button with data-mapid attribute
            var adventureStartButton = doc.DocumentNode
                .Descendants("button")
                .FirstOrDefault(x => x.GetAttributeValue("data-mapid", "") != "");
            if (adventureStartButton is not null) return adventureStartButton;

            // TTWars fallback: look for button with adventure-related text
            var buttons = doc.DocumentNode.Descendants("button");
            foreach (var btn in buttons)
            {
                var text = btn.InnerText.Trim();
                if (text.Equals("Explore", StringComparison.OrdinalIgnoreCase) ||
                    text.Equals("Jalankan", StringComparison.OrdinalIgnoreCase) ||
                    text.Equals("Abenteuer", StringComparison.OrdinalIgnoreCase) ||
                    text.Equals("Start", StringComparison.OrdinalIgnoreCase))
                {
                    return btn;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the continue button after adventure.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static HtmlNode? GetContinueButton(HtmlDocument doc)
        {
            var continueButton = doc.DocumentNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("continue"));
            return continueButton;
        }

        /// <summary>
        /// Gets adventure info string for logging.
        /// </summary>
        public static string GetAdventureInfo(HtmlNode node)
        {
            // adventureTableBodyRow/td/button
            var trNode = node.ParentNode?.ParentNode;
            if (trNode is null) return "unknown - [~|~]";

            var difficult = GetAdventureDifficult(trNode);
            var coordinates = GetAdventureCoordinates(trNode);

            return $"{difficult} - {coordinates}";
        }

        private static string GetAdventureDifficult(HtmlNode node)
        {
            var tdList = node.Descendants("td").ToArray();
            if (tdList.Length < 3) return "unknown";

            // Standard Travian: icon in td[3]
            var iconDifficulty = tdList[3].FirstChild;
            if (iconDifficulty is not null)
            {
                var alt = iconDifficulty.GetAttributeValue("alt", "");
                if (!string.IsNullOrEmpty(alt)) return alt;
            }

            // TTWars: i.difficulty_normal or i.difficulty_hard
            var difficultyIcon = node.Descendants("i")
                .FirstOrDefault(x => x.HasClass("difficulty_normal") || x.HasClass("difficulty_hard"));
            if (difficultyIcon is not null)
            {
                if (difficultyIcon.HasClass("difficulty_hard")) return "Hard";
                return "Normal";
            }

            return "unknown";
        }

        private static string GetAdventureCoordinates(HtmlNode node)
        {
            var tdList = node.Descendants("td").ToArray();

            // Standard Travian: td[1] contains coordinates like "(135|−90)" or "[44|12]"
            if (tdList.Length >= 2)
            {
                var text = tdList[1].InnerText.Trim();
                if ((text.Contains("(") && text.Contains(")")) ||
                    (text.Contains("[") && text.Contains("]")))
                    return text;
            }

            // TTWars: no coordinates column, try to extract from distance text
            // Distance is in td[1] as "3 Bidang" or "2,8 Bidang"
            if (tdList.Length >= 2)
            {
                var distanceText = tdList[1].InnerText.Trim();
                if (distanceText.Contains("Bidang") || distanceText.Contains("field"))
                    return $"Distance: {distanceText}";
            }

            return "[~|~]";
        }

        /// <summary>
        /// Checks if the hero is available for adventures.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool IsHeroAvailable(HtmlDocument doc)
        {
            // Standard Travian: div.heroStatus > i.heroHome
            var heroStatus = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("heroStatus"));
            if (heroStatus is not null)
            {
                var heroHome = heroStatus.Descendants("i")
                    .Any(x => x.HasClass("heroHome"));
                if (heroHome) return true;
            }

            // TTWars: div.heroState > i.statusHome_medium
            var heroState = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("heroState"));
            if (heroState is not null)
            {
                var heroHome = heroState.Descendants("i")
                    .Any(x => x.HasClass("statusHome_medium"));
                return heroHome;
            }

            return false;
        }

        /// <summary>
        /// Gets the number of available adventures.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static int GetAvailableAdventureCount(HtmlDocument doc)
        {
            // Standard Travian: #heroAdventure > tbody > tr
            var adventureTable = doc.GetElementbyId("heroAdventure");
            if (adventureTable is not null)
            {
                var adventureTableBody = adventureTable
                    .Descendants("tbody")
                    .FirstOrDefault();
                if (adventureTableBody is not null)
                {
                    var rows = adventureTableBody
                        .Descendants("tr")
                        .Where(x => x.Descendants("button").Any())
                        .Count();
                    if (rows > 0) return rows;
                }
            }

            // TTWars: table.adventureList > tbody > tr
            var adventureList = doc.DocumentNode
                .Descendants("table")
                .FirstOrDefault(x => x.HasClass("adventureList"));
            if (adventureList is not null)
            {
                var tbody = adventureList.Descendants("tbody").FirstOrDefault();
                if (tbody is not null)
                {
                    return tbody.Descendants("tr")
                        .Where(x => x.Descendants("button").Any())
                        .Count();
                }
            }

            return 0;
        }
    }
}