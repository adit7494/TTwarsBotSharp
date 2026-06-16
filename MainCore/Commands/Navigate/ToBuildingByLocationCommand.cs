using System.Web;

namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class ToBuildingByLocationCommand
    {
        public sealed record Command(int Location) : ICommand;

        private static async ValueTask<Result> HandleAsync(
           Command command,
           IChromeBrowser browser,
           CancellationToken cancellationToken
           )
        {
            return await ToBuilding(command.Location, browser, cancellationToken);
        }

        public static async ValueTask<Result> ToBuilding(
            int location,
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {
            var (_, isFailed, element, errors) = await browser.GetElement(doc => GetBuilding(doc, location), cancellationToken);
            if (isFailed) return Result.Fail(errors).WithError($"Failed to find [building at #{location}]");

            var node = GetBuilding(browser.Html, location)!;

            Result result;
            if (location > 18 && IsEmptySlot(node))
            {
                if (location == 40) // wall
                {
                    var currentUrl = new Uri(browser.CurrentUrl);
                    var host = currentUrl.GetLeftPart(UriPartial.Authority);
                    await browser.Navigate($"{host}/build.php?id={location}", cancellationToken);
                }
                else
                {
                    // Try standard Travian CSS selector first
                    var css = $"#villageContent > div.buildingSlot.a{location} > svg > path";
                    (_, isFailed, element, errors) = await browser.GetElement(By.CssSelector(css), cancellationToken);

                    // TTWars fallback: look for div with data-aid attribute
                    if (isFailed)
                    {
                        css = $"#villageContent > div[data-aid='{location}'] > svg > path";
                        (_, isFailed, element, errors) = await browser.GetElement(By.CssSelector(css), cancellationToken);
                    }

                    if (isFailed) return Result.Fail(errors);

                    result = await browser.Click(element, cancellationToken);
                    if (result.IsFailed) return result;
                }
            }
            else
            {
                if (location == 40) // wall
                {
                    var path = node.Descendants("path").FirstOrDefault();
                    if (path is null) return Retry.Error.WithError("Failed to find [wall]");

                    var javascript = path.GetAttributeValue("onclick", "");
                    if (string.IsNullOrEmpty(javascript)) return Retry.Error.WithError("Failed to find [wall's onclick event]");

                    var decodedJs = HttpUtility.HtmlDecode(javascript);

                    result = await browser.ExecuteJsScript(decodedJs);
                    if (result.IsFailed) return result;
                }
                else
                {
                    result = await browser.Click(element, cancellationToken);
                    if (result.IsFailed) return result;
                }
            }

            result = await browser.WaitPageChanged("build", cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        /// <summary>
        /// Checks if a building slot is empty.
        /// Handles both standard Travian (class "g0") and TTWars (data-gid="0").
        /// </summary>
        private static bool IsEmptySlot(HtmlNode node)
        {
            // Standard Travian: class "g0"
            if (node.HasClass("g0")) return true;

            // TTWars: data-gid="0"
            var dataGid = node.GetAttributeValue("data-gid", "-1");
            if (dataGid == "0") return true;

            return false;
        }

        private static HtmlNode? GetBuilding(HtmlDocument doc, int location)
        {
            if (location < 19) return GetField(doc, location);
            return GetInfrastructure(doc, location);
        }

        /// <summary>
        /// Gets a resource field by location.
        /// Handles both standard Travian (class buildingSlot{N}) and TTWars (data-aid attribute).
        /// </summary>
        private static HtmlNode? GetField(HtmlDocument doc, int location)
        {
            // Standard Travian: a.buildingSlot{location}
            var node = doc.DocumentNode
                   .Descendants("a")
                   .FirstOrDefault(x => x.HasClass($"buildingSlot{location}"));
            if (node is not null) return node;

            // TTWars: a[data-aid="{location}"]
            node = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("data-aid", "") == location.ToString());
            return node;
        }

        /// <summary>
        /// Gets an infrastructure building by location.
        /// Handles both standard Travian (XPath by position) and TTWars (data-aid attribute).
        /// </summary>
        private static HtmlNode? GetInfrastructure(HtmlDocument doc, int location)
        {
            // TTWars: look for div with data-aid attribute first
            var div = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("buildingSlot") &&
                    x.GetAttributeValue("data-aid", "") == location.ToString());
            if (div is not null) return div;

            // Standard Travian: XPath by position
            var tmpLocation = location - 18;
            div = doc.DocumentNode
                .SelectSingleNode($"//*[@id='villageContent']/div[{tmpLocation}]");
            return div;
        }
    }
}