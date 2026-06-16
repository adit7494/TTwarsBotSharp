using System.Net;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class GetValidAccessCommand
    {
        public sealed record Command(AccountId AccountId, bool IgnoreSleepTime = false) : IAccountCommand;

        private static async ValueTask<Result<AccessDto>> HandleAsync(
            Command command,
            ILogger logger,
            AppDbContext context
            )
        {
            var (accountId, ignoreSleepTime) = command;

            var accesses = context.Accesses
               .Where(x => x.AccountId == accountId.Value)
               .OrderBy(x => x.LastUsed) // get oldest one
               .ToDto()
               .ToList();

            async Task<AccessDto?> GetValidAccess(List<AccessDto> proxies)
            {
                foreach (var proxy in proxies)
                {
                    using var client = CreateHttpClient(proxy);
                    logger.Information("Checking proxy {Proxy}, last used {LastUsed}", proxy.Proxy, proxy.LastUsed);
                    try
                    {
                        var response = await client.GetAsync(TRAVIAN_PAGE);
                        if (response.IsSuccessStatusCode)
                        {
                            logger.Information("Access {Proxy} is good", proxy.Proxy);
                            return proxy;
                        }

                        logger.Warning("Access {Proxy} is not working, status code: {StatusCode}", proxy.Proxy, response.StatusCode);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "{Message}", ex.Message);
                    }
                }
                return null;
            }

            var access = await GetValidAccess(accesses);
            if (access is null) return Stop.Error.WithError("All accesses not working");

            if (accesses.Count == 1) return access;
            if (ignoreSleepTime) return access;

            var minSleep = context.ByName(accountId, AccountSettingEnums.SleepTimeMin);
            var timeValid = DateTime.Now.AddMinutes(-minSleep);
            if (access.LastUsed > timeValid) return Stop.Error.WithError("Last access is reused, it may get MH's attention");
            return access;
        }

        private const string TRAVIAN_PAGE = "https://www.travian.com/international";

        private static HttpClient CreateHttpClient(AccessDto access)
        {
            if (string.IsNullOrEmpty(access.ProxyHost))
            {
                return new HttpClient(new HttpClientHandler { UseProxy = false });
            }

            if (string.IsNullOrEmpty(access.ProxyUsername))
            {
                var proxy = new WebProxy(new Uri($"http://{access.ProxyHost}:{access.ProxyPort}"));
                return new HttpClient(new HttpClientHandler { Proxy = proxy, UseProxy = true });
            }

            var credentials = new NetworkCredential(access.ProxyUsername, access.ProxyPassword);
            var authProxy = new WebProxy(new Uri($"http://{access.ProxyHost}:{access.ProxyPort}"))
            {
                Credentials = credentials,
            };
            return new HttpClient(new HttpClientHandler { Proxy = authProxy, UseProxy = true });
        }
    }
}