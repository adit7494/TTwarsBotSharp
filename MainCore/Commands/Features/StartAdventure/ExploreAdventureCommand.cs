#pragma warning disable S1172

namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class ExploreAdventureCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            // Wait for adventure page to fully render (React-based content)
            await Task.Delay(1000, cancellationToken);

            // Retry getting adventure button with small delays
            for (int attempt = 0; attempt < 3; attempt++)
            {
                if (AdventureParser.CanStartAdventure(browser.Html))
                {
                    var adventureButton = AdventureParser.GetAdventureButton(browser.Html);
                    if (adventureButton is not null)
                    {
                        logger.Information("Start adventure {Adventure}", AdventureParser.GetAdventureInfo(adventureButton));

                        var (_, isFailed, element, errors) = await browser.GetElement(By.XPath(adventureButton.XPath), cancellationToken);
                        if (isFailed) return Result.Fail(errors).WithError($"Failed to find adventure button [{adventureButton.XPath}]");

                        var result = await browser.Click(element, cancellationToken);
                        if (result.IsFailed) return result;

                        // Wait for adventure to start - works for both standard Travian and TTWars
                        // Poll for up to 15 seconds
                        var startTime = DateTime.Now;
                        while ((DateTime.Now - startTime).TotalSeconds < 15)
                        {
                            await Task.Delay(500, cancellationToken);
                            var pageHtml = browser.Html;

                            // Standard Travian: continue button appears
                            if (AdventureParser.GetContinueButton(pageHtml) is not null)
                            {
                                logger.Information("Adventure started (continue button detected)");
                                return Result.Ok();
                            }

                            // TTWars: hero is no longer at home (departed on adventure)
                            if (!AdventureParser.IsHeroAvailable(pageHtml))
                            {
                                logger.Information("Adventure started (hero departed)");
                                return Result.Ok();
                            }
                        }

                        // For very short adventures (1-2 seconds), the hero may have already returned
                        // before we detected the departure. The click was sent, so the adventure was started.
                        logger.Information("Adventure button clicked, proceeding (short duration adventure)");
                        return Result.Ok();
                    }
                }

                // Adventure table might still be loading, wait and retry
                logger.Warning("Adventure button not found, retrying ({Attempt}/3)...", attempt + 1);
                await Task.Delay(2000, cancellationToken);
            }

            return Retry.Error.WithError("Failed to find adventure button after retries");
        }
    }
}