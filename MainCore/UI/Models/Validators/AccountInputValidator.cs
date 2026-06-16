using MainCore.UI.Models.Input;

namespace MainCore.UI.Models.Validators
{
    public class AccountInputValidator : AbstractValidator<AccountInput>
    {
        public AccountInputValidator()
        {
            RuleFor(x => x.Server)
                .NotEmpty()
                .WithName("Server url");

            RuleFor(x => x.Server)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).When(x => !string.IsNullOrEmpty(x.Server))
                .WithMessage("Invalid Server url, please follow the pattern [https://ts1.x1.international.travian.com]");

            RuleFor(x => x.Accesses)
                .NotEmpty()
                .WithName("Access list");

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithName("Nick name");
        }
    }

    /// <summary>
    /// Helper class for detecting server type from URL.
    /// </summary>
    public static class ServerTypeDetector
    {
        /// <summary>
        /// Detects the server type based on the URL.
        /// TTWars URLs have domains ending with "ttwars.com".
        /// </summary>
        /// <param name="url">The server URL to analyze.</param>
        /// <returns>The detected server type.</returns>
        public static ServerType DetectFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return ServerType.Travian;

            // Try to parse the URL and check the host
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                // TTWars URLs end with "ttwars.com" in the domain
                var host = uri.Host.ToLowerInvariant();
                if (host == "ttwars.com" || host.EndsWith(".ttwars.com"))
                    return ServerType.TTWars;
            }

            // Default to standard Travian
            return ServerType.Travian;
        }
    }
}