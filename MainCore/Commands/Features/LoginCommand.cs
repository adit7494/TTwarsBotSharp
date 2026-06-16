namespace MainCore.Commands.Features
{
    [Handler]
    public static partial class LoginCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var currentUrl = browser.CurrentUrl;
            logger.Information("LoginCommand: Current URL = {Url}", currentUrl);

            var isIngame = LoginParser.IsIngamePage(browser.Html);
            logger.Information("LoginCommand: IsIngamePage = {IsIngame}", isIngame);

            if (isIngame)
            {
                logger.Information("LoginCommand: Already ingame, skipping login");
                return Result.Ok();
            }

            var (username, password) = GetLoginInfo(command.AccountId, context);
            logger.Information("LoginCommand: Username = {Username}, Password length = {PasswordLen}", username, password?.Length ?? 0);
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                logger.Error("LoginCommand: Username or password is empty!");
                return Result.Fail("Username or password is empty");
            }

            Result result;

            var (_, isFailed, element, errors) = await browser.GetElement(doc => LoginParser.GetUsernameInput(doc), cancellationToken);
            if (isFailed)
            {
                logger.Error("LoginCommand: Failed to find username input. Errors: {Errors}", string.Join("; ", errors.Select(e => e.Message)));
                return Result.Fail(errors);
            }
            logger.Information("LoginCommand: Found username input, typing...");
            result = await browser.Input(element, username, cancellationToken);
            if (result.IsFailed) return result;

            (_, isFailed, element, errors) = await browser.GetElement(doc => LoginParser.GetPasswordInput(doc), cancellationToken);
            if (isFailed)
            {
                logger.Error("LoginCommand: Failed to find password input. Errors: {Errors}", string.Join("; ", errors.Select(e => e.Message)));
                return Result.Fail(errors);
            }
            logger.Information("LoginCommand: Found password input, typing...");
            result = await browser.Input(element, password, cancellationToken);
            if (result.IsFailed) return result;

            (_, isFailed, element, errors) = await browser.GetElement(doc => LoginParser.GetLoginButton(doc), cancellationToken);
            if (isFailed)
            {
                logger.Error("LoginCommand: Failed to find login button. Errors: {Errors}", string.Join("; ", errors.Select(e => e.Message)));
                return Result.Fail(errors);
            }
            logger.Information("LoginCommand: Found login button, clicking...");
            result = await browser.Click(element, cancellationToken);
            if (result.IsFailed) return result;

            logger.Information("LoginCommand: Waiting for page change to dorf...");
            result = await browser.WaitPageChanged("dorf", cancellationToken);
            if (result.IsFailed) return result;

            logger.Information("LoginCommand: Login successful!");
            return Result.Ok();
        }

        private static (string username, string password) GetLoginInfo(AccountId accountId, AppDbContext context)
        {
            var data = context.Accesses
                .Where(x => x.AccountId == accountId.Value)
                .OrderByDescending(x => x.LastUsed)
                .Select(x => new { x.Username, x.Password })
                .FirstOrDefault();

            if (data is null) return ("", "");

            return (data.Username, data.Password);
        }
    }
}