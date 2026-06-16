namespace MainCore.Parsers
{
    public static class NavigationBarParser
    {
        /// <summary>
        /// Gets the dorf navigation button by dorf number.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static HtmlNode? GetDorfButton(HtmlDocument doc, int dorf)
        {
            return dorf switch
            {
                1 => GetDorf1Button(doc),
                2 => GetDorf2Button(doc),
                _ => null,
            };
        }

        /// <summary>
        /// Gets a navigation button by accesskey.
        /// Standard Travian uses accesskey="1" for dorf1 and accesskey="2" for dorf2.
        /// </summary>
        private static HtmlNode? GetButton(HtmlDocument doc, int key)
        {
            var navigationBar = doc.GetElementbyId("navigation");
            if (navigationBar is null) return null;

            var keyStr = key.ToString();
            var button = navigationBar
                .Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("accesskey", "") == keyStr);
            return button;
        }

        /// <summary>
        /// Gets the resource view button (dorf1).
        /// </summary>
        private static HtmlNode? GetResourceButton(HtmlDocument doc) => GetButton(doc, 1);

        /// <summary>
        /// Gets the building view button (dorf2).
        /// </summary>
        private static HtmlNode? GetBuildingButton(HtmlDocument doc) => GetButton(doc, 2);

        /// <summary>
        /// Gets a navigation button by href pattern.
        /// TTWars might use different accesskey values or none at all.
        /// </summary>
        public static HtmlNode? GetButtonByHref(HtmlDocument doc, string hrefPattern)
        {
            // Try #navigation first
            var navigationBar = doc.GetElementbyId("navigation");
            if (navigationBar is not null)
            {
                var button = navigationBar
                    .Descendants("a")
                    .FirstOrDefault(x => x.GetAttributeValue("href", "").Contains(hrefPattern));
                if (button is not null) return button;
            }

            // Fallback: search entire page for links with the href pattern
            var link = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("href", "").Contains(hrefPattern));
            return link;
        }

        /// <summary>
        /// Gets the dorf1 (resource fields) button using multiple strategies.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static HtmlNode? GetDorf1Button(HtmlDocument doc)
        {
            // Strategy 1: accesskey="1"
            var button = GetResourceButton(doc);
            if (button is not null) return button;

            // Strategy 2: href contains "dorf1.php"
            button = GetButtonByHref(doc, "dorf1.php");
            if (button is not null) return button;

            // Strategy 3: look for village center link (TTWars uses <a> with class "villageCenter")
            // On dorf2, villageCenter link goes to dorf1
            var villageCenterLink = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.HasClass("villageCenter"));
            if (villageCenterLink is not null)
            {
                var href = villageCenterLink.GetAttributeValue("href", "");
                if (href.Contains("dorf1.php"))
                {
                    return villageCenterLink;
                }
            }

            // Strategy 4: look for SVG with onclick containing dorf1.php (TTWars specific)
            var svgPath = doc.DocumentNode
                .Descendants("path")
                .FirstOrDefault(x => x.GetAttributeValue("onclick", "").Contains("dorf1.php"));
            if (svgPath is not null) return svgPath;

            // Strategy 5: look for any element with onclick containing dorf1.php
            var onclickElement = doc.DocumentNode
                .Descendants()
                .FirstOrDefault(x => x.GetAttributeValue("onclick", "").Contains("dorf1.php"));
            if (onclickElement is not null) return onclickElement;

            // Strategy 6: look for logo link (TTWars - clicking logo goes to dorf1)
            var logo = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("id", "") == "logo");
            if (logo is not null) return logo;

            return null;
        }

        /// <summary>
        /// Gets the dorf2 (village buildings) button using multiple strategies.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static HtmlNode? GetDorf2Button(HtmlDocument doc)
        {
            // Strategy 1: accesskey="2"
            var button = GetBuildingButton(doc);
            if (button is not null) return button;

            // Strategy 2: href contains "dorf2.php"
            button = GetButtonByHref(doc, "dorf2.php");
            if (button is not null) return button;

            // Strategy 3: look for village center link (TTWars uses <a> with class "villageCenter")
            // On dorf1, villageCenter link goes to dorf2
            var villageCenterLink = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.HasClass("villageCenter"));
            if (villageCenterLink is not null)
            {
                var href = villageCenterLink.GetAttributeValue("href", "");
                if (href.Contains("dorf2.php"))
                {
                    return villageCenterLink;
                }
            }

            // Strategy 4: look for SVG with onclick containing dorf2.php (TTWars specific)
            var svgPath = doc.DocumentNode
                .Descendants("path")
                .FirstOrDefault(x => x.GetAttributeValue("onclick", "").Contains("dorf2.php"));
            if (svgPath is not null) return svgPath;

            // Strategy 5: look for any element with onclick containing dorf2.php
            var onclickElement = doc.DocumentNode
                .Descendants()
                .FirstOrDefault(x => x.GetAttributeValue("onclick", "").Contains("dorf2.php"));
            if (onclickElement is not null) return onclickElement;

            return null;
        }
    }
}
