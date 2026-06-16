namespace MainCore.Parsers
{
    public static class BuildingTabParser
    {
        public static HtmlNode? GetNavigationBar(HtmlDocument doc)
        {
            var navigationBar = doc.DocumentNode
             .Descendants("div")
             .FirstOrDefault(x => x.HasClass("contentNavi") && x.HasClass("subNavi"));
            if (navigationBar is null) return null;
            return navigationBar;
        }

        private static IEnumerable<HtmlNode> GetTabs(HtmlDocument doc)
        {
            var navigationBar = GetNavigationBar(doc);
            if (navigationBar is null) return [];
            var tabs = navigationBar
                .Descendants("a")
                .Where(x => x.HasClass("tabItem"));
            return tabs;
        }

        public static int CountTab(HtmlDocument doc)
        {
            var count = GetTabs(doc)
                .Count();
            return count;
        }

        public static HtmlNode GetTab(HtmlDocument doc, int index)
        {
            var tab = GetTabs(doc)
                .ElementAt(index);
            return tab;
        }

        public static bool IsTabActive(HtmlNode node)
        {
            return node.HasClass("active");
        }

        /// <summary>
        /// Checks if the page has a building tab navigation.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool HasBuildingTabs(HtmlDocument doc)
        {
            var navigationBar = GetNavigationBar(doc);
            return navigationBar is not null;
        }

        /// <summary>
        /// Gets the active tab index.
        /// Returns -1 if no active tab is found.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static int GetActiveTabIndex(HtmlDocument doc)
        {
            var tabs = GetTabs(doc).ToList();
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].HasClass("active")) return i;
            }
            return -1;
        }

        /// <summary>
        /// Gets all tab names.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static IEnumerable<string> GetTabNames(HtmlDocument doc)
        {
            var tabs = GetTabs(doc);
            foreach (var tab in tabs)
            {
                yield return tab.InnerText.Trim();
            }
        }
    }
}