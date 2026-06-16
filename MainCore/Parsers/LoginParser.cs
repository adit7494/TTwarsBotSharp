namespace MainCore.Parsers
{
    public static class LoginParser
    {
        private static readonly Serilog.ILogger _logger = Serilog.Log.ForContext(typeof(LoginParser));
        public static HtmlNode? GetLoginButton(HtmlDocument doc)
        {
            var loginScene = doc.GetElementbyId("loginScene");
            if (loginScene is null) return null;

            var loginButton = loginScene
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("green"));

            return loginButton;
        }

        public static HtmlNode? GetUsernameInput(HtmlDocument doc)
        {
            var usernameInput = doc.DocumentNode
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "").Equals("name"));
            return usernameInput;
        }

        public static HtmlNode? GetPasswordInput(HtmlDocument doc)
        {
            var passwordInput = doc.DocumentNode
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "").Equals("password"));
            return passwordInput;
        }

        public static bool IsIngamePage(HtmlDocument doc)
        {
            // First check if this is a login page - if so, it's NOT ingame
            // TTWars login pages have #loginScene element
            var loginScene = doc.GetElementbyId("loginScene");
            if (loginScene is not null)
            {
                _logger.Information("IsIngamePage: Found #loginScene element, this is a login page, returning false");
                return false;
            }

            // Standard Travian: check for #servertime element
            var serverTime = doc.GetElementbyId("servertime");
            if (serverTime is not null)
            {
                _logger.Information("IsIngamePage: Found #servertime element, returning true");
                return true;
            }

            // TTWars: check for #content element with ingame classes
            // TTWars pages have <div id="content" class="village1|village2|build|...">
            var contentNode = doc.GetElementbyId("content");
            if (contentNode is null)
            {
                _logger.Information("IsIngamePage: No #content element found, returning false");
                return false;
            }

            var contentClass = contentNode.GetAttributeValue("class", "");
            _logger.Information("IsIngamePage: Found #content with class='{ContentClass}'", contentClass);

            // Check for known ingame content classes
            var isIngame = contentClass.Contains("village1") ||
                   contentClass.Contains("village2") ||
                   contentClass.Contains("build") ||
                   contentClass.Contains("map") ||
                   contentClass.Contains("statistics") ||
                   contentClass.Contains("profile") ||
                   contentClass.Contains("report") ||
                   contentClass.Contains("messages") ||
                   contentClass.Contains("options") ||
                   contentClass.Contains("tasks") ||
                   contentClass.Contains("hero");

            _logger.Information("IsIngamePage: IsIngame = {IsIngame}", isIngame);
            return isIngame;
        }

        public static bool IsLoginPage(HtmlDocument doc)
        {
            var loginButton = GetLoginButton(doc);
            return loginButton is not null;
        }

        /// <summary>
        /// Checks if the page is a TTWars login page.
        /// TTWars login pages have a React-based login form with specific structure.
        /// </summary>
        public static bool IsTTWarsLoginPage(HtmlDocument doc)
        {
            var loginScene = doc.GetElementbyId("loginScene");
            if (loginScene is null) return false;

            // TTWars login pages have a dialog overlay with specific classes
            var dialogOverlay = loginScene.Descendants("div")
                .FirstOrDefault(x => x.HasClass("dialogOverlay"));
            return dialogOverlay is not null;
        }

        /// <summary>
        /// Gets the login form element, handling both standard Travian and TTWars.
        /// </summary>
        public static HtmlNode? GetLoginForm(HtmlDocument doc)
        {
            // Standard Travian: form directly in loginScene
            var loginScene = doc.GetElementbyId("loginScene");
            if (loginScene is null) return null;

            // Try to find form directly
            var form = loginScene.Descendants("form").FirstOrDefault();
            if (form is not null) return form;

            // TTWars: form inside dialog wrapper
            var dialogWrapper = loginScene.Descendants("div")
                .FirstOrDefault(x => x.HasClass("dialogWrapper"));
            if (dialogWrapper is not null)
            {
                form = dialogWrapper.Descendants("form").FirstOrDefault();
            }

            return form;
        }
    }
}