namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class ToDorfCommand
    {
        public sealed record Command(int Dorf) : ICommand;

        private static async ValueTask<Result> HandleAsync(
           Command command,
           IChromeBrowser browser,
           ILogger logger,
           CancellationToken cancellationToken
           )
        {
            var dorf = command.Dorf;
            logger.Information("ToDorfCommand: Target dorf={Dorf}", dorf);

            var currentUrl = browser.CurrentUrl;
            logger.Information("ToDorfCommand: Current URL={Url}", currentUrl);

            var currentDorf = GetCurrentDorf(currentUrl);
            logger.Information("ToDorfCommand: Current dorf={CurrentDorf}", currentDorf);

            if (dorf == 0)
            {
                if (currentDorf == 0) dorf = 2;
                else dorf = currentDorf;
                logger.Information("ToDorfCommand: Resolved dorf to {Dorf}", dorf);
            }

            if (currentDorf != 0 && dorf == currentDorf)
            {
                logger.Information("ToDorfCommand: Already on dorf{Dorf}, skipping", dorf);
                return Result.Ok();
            }

            logger.Information("ToDorfCommand: Looking for dorf{Dorf} button...", dorf);
            var (_, isFailed, element, errors) = await browser.GetElement(doc => NavigationBarParser.GetDorfButton(doc, dorf), cancellationToken);
            if (isFailed)
            {
                logger.Error("ToDorfCommand: Failed to find dorf{Dorf} button. Errors: {Errors}", dorf, string.Join("; ", errors.Select(e => e.Message)));
                return Result.Fail(errors);
            }

            logger.Information("ToDorfCommand: Found button, clicking...");
            Result result;
            result = await browser.Click(element, cancellationToken);
            if (result.IsFailed)
            {
                logger.Error("ToDorfCommand: Failed to click button");
                return result;
            }

            logger.Information("ToDorfCommand: Waiting for page change to dorf{Dorf}.php...", dorf);
            result = await browser.WaitPageChanged($"dorf{dorf}.php", cancellationToken);
            if (result.IsFailed)
            {
                logger.Error("ToDorfCommand: Failed to wait for page change");
                return result;
            }

            logger.Information("ToDorfCommand: Navigation successful!");
            return Result.Ok();
        }

        private static int GetCurrentDorf(string url)
        {
            if (url.Contains("dorf1")) return 1;
            if (url.Contains("dorf2")) return 2;
            return 0;
        }
    }
}