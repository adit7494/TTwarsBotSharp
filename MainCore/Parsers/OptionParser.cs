namespace MainCore.Parsers
{
    public static class OptionParser
    {
        /// <summary>
        /// Checks if contextual help is enabled on the current page.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool IsContextualHelpEnable(HtmlDocument doc)
        {
            // Standard Travian: #contextualHelp element exists
            var node = doc.GetElementbyId("contextualHelp");
            if (node is not null) return true;

            // TTWars: check for contextual help in options page
            // The contextual help might be indicated by a different element
            var contextualHelpDiv = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("contextualHelp"));
            return contextualHelpDiv is not null;
        }

        /// <summary>
        /// Gets the options button from the page.
        /// Handles both standard Travian (#outOfGame) and TTWars (alternative selectors).
        /// </summary>
        public static HtmlNode? GetOptionButton(HtmlDocument doc)
        {
            // Standard Travian: #outOfGame > a.options
            var outOfGame = doc.GetElementbyId("outOfGame");
            if (outOfGame is not null)
            {
                var optionButton = outOfGame
                    .Descendants("a")
                    .FirstOrDefault(x => x.HasClass("options"));
                if (optionButton is not null) return optionButton;
            }

            // TTWars: look for options link in the page
            var optionsLink = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.HasClass("options") &&
                    x.GetAttributeValue("href", "").Contains("options"));
            if (optionsLink is not null) return optionsLink;

            // Fallback: any link with "options" class
            optionsLink = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.HasClass("options"));
            return optionsLink;
        }

        /// <summary>
        /// Gets the "hide contextual help" option checkbox.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static HtmlNode? GetHideContextualHelpOption(HtmlDocument doc)
        {
            // Standard Travian: #hideContextualHelp
            var node = doc.GetElementbyId("hideContextualHelp");
            if (node is not null) return node;

            // TTWars: look for checkbox with name or id containing "contextualHelp"
            node = doc.DocumentNode
                .Descendants("input")
                .FirstOrDefault(x =>
                    x.GetAttributeValue("name", "").Contains("contextualHelp") ||
                    x.GetAttributeValue("id", "").Contains("contextualHelp"));
            return node;
        }

        /// <summary>
        /// Gets the submit button on the options page.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static HtmlNode? GetSubmitButton(HtmlDocument doc)
        {
            // Standard Travian: div.submitButtonContainer > button
            var submitButtonContainer = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("submitButtonContainer"));
            if (submitButtonContainer is not null)
            {
                var submitButton = submitButtonContainer
                    .Descendants("button")
                    .FirstOrDefault();
                if (submitButton is not null) return submitButton;
            }

            // TTWars: look for submit button in form
            var submitButton2 = doc.DocumentNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("green") &&
                    (x.GetAttributeValue("type", "") == "submit" ||
                     x.GetAttributeValue("value", "").Contains("Save") ||
                     x.GetAttributeValue("value", "").Contains("Simpan")));
            return submitButton2;
        }

        /// <summary>
        /// Checks if the page is an options page.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool IsOptionsPage(HtmlDocument doc)
        {
            // Standard Travian: #outOfGame element exists
            var outOfGame = doc.GetElementbyId("outOfGame");
            if (outOfGame is not null) return true;

            // TTWars: check for options content class
            var contentNode = doc.GetElementbyId("content");
            if (contentNode is not null)
            {
                var contentClass = contentNode.GetAttributeValue("class", "");
                if (contentClass.Contains("options")) return true;
            }

            // Fallback: check for options-specific elements
            var submitButtonContainer = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("submitButtonContainer"));
            return submitButtonContainer is not null;
        }

        /// <summary>
        /// Checks if the page has a submit button.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool HasSubmitButton(HtmlDocument doc)
        {
            // Standard Travian: div.submitButtonContainer
            var submitButtonContainer = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("submitButtonContainer"));
            if (submitButtonContainer is not null) return true;

            // TTWars: look for submit button
            var submitButton = doc.DocumentNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("green") &&
                    (x.GetAttributeValue("type", "") == "submit" ||
                     x.GetAttributeValue("value", "").Contains("Save") ||
                     x.GetAttributeValue("value", "").Contains("Simpan")));
            return submitButton is not null;
        }
    }
}