namespace MainCore.Parsers
{
    public static class CompleteImmediatelyParser
    {
        public static int CountQueueBuilding(HtmlDocument doc)
        {
            var finishButton = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("finishNow"));
            if (finishButton is null) return 0;
            var nodes = finishButton.ParentNode
                .Descendants("li");
            return nodes.Count();
        }

        public static HtmlNode? GetCompleteButton(HtmlDocument doc)
        {
            var finishDiv = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("finishNow"));

            if (finishDiv is null) return null;

            var finishButton = finishDiv
                .Descendants("button")
                .FirstOrDefault();
            return finishButton;
        }

        public static HtmlNode? GetConfirmButton(HtmlDocument doc)
        {
            var finishDialog = doc.GetElementbyId("finishNowDialog");
            if (finishDialog is null) return null;

            var confirmFinishbutton = finishDialog
                .Descendants("button")
                .FirstOrDefault();
            return confirmFinishbutton;
        }

        /// <summary>
        /// Checks if the page has a "finish now" button.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool HasFinishNowButton(HtmlDocument doc)
        {
            var finishDiv = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("finishNow"));
            return finishDiv is not null;
        }

        /// <summary>
        /// Checks if the page has a "finish now" dialog.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool HasFinishNowDialog(HtmlDocument doc)
        {
            var finishDialog = doc.GetElementbyId("finishNowDialog");
            return finishDialog is not null;
        }
    }
}