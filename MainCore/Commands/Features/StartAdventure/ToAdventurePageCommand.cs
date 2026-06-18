#pragma warning disable S1172

namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class ToAdventurePageCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var (_, isFailed, element, errors) = await browser.GetElement(doc => AdventureParser.GetHeroAdventureButton(doc), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            var result = await browser.Click(element, cancellationToken);
            if (result.IsFailed) return result;

            // Small delay to let TTWars React dialog render
            await Task.Delay(1000, cancellationToken);

            static bool TableShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return AdventureParser.IsAdventurePage(doc);
            }
            result = await browser.Wait(TableShow, cancellationToken);
            if (result.IsFailed) return result;

            // Additional wait to ensure adventure table content is fully rendered
            await Task.Delay(1000, cancellationToken);

            return Result.Ok();
        }
    }
}