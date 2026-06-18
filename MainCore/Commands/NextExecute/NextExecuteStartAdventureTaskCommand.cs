namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteStartAdventureTaskCommand
    {
        public sealed record Command(StartAdventureTask.Task Task) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser
            )
        {
            await Task.CompletedTask;
            var adventureDuration = AdventureParser.GetAdventureDuration(browser.Html);
            var delay = adventureDuration * 2;
            // Minimum 10-second delay to prevent immediate re-scheduling
            // when adventure duration is very short (1-2 seconds)
            if (delay < TimeSpan.FromSeconds(10)) delay = TimeSpan.FromSeconds(10);
            command.Task.ExecuteAt = DateTime.Now.Add(delay);
        }
    }
}