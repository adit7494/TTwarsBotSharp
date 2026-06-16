using System.Net.Http.Json;
using System.Text.Json;

namespace MainCore.Services
{
    [RegisterSingleton<IUseragentManager, UseragentManager>]
    public sealed class UseragentManager(ILogger logger) : IUseragentManager
    {
        private readonly ILogger _logger = logger.ForContext<UseragentManager>();
        private List<string> _userAgentList = [];
        private DateTime _dateTime;
        private readonly object _lock = new();

        private const string _userAgentUrl = "https://raw.githubusercontent.com/TTwarsBotSharp/user-agent/main/user-agent.json";
        private readonly HttpClient _httpClient = new();

        private async Task Update()
        {
            try
            {
                var useragents = await _httpClient.GetFromJsonAsync<List<string>>(_userAgentUrl);
                if (useragents is null || useragents.Count == 0)
                {
                    _logger.Error("User agent list is empty or null.");
                }
                _userAgentList = useragents ?? new List<string>();
                _logger.Information("User agent list loaded, count: {Count}", _userAgentList.Count);
                _dateTime = DateTime.Now.AddMonths(1); // need update after 1 month, thought so
                Save();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to update user agent list from remote.");
                if (_userAgentList.Count == 0)
                {
                    _logger.Warning("User agent list is empty. Bot may not function correctly.");
                }
            }
        }

        private void Save()
        {
            var userAgentJsonString = JsonSerializer.Serialize(new Model
            {
                UserAgentList = _userAgentList,
                DateTime = _dateTime,
            });
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "useragent.json");

            File.WriteAllText(path, userAgentJsonString);
        }

        public async Task Load()
        {
            var pathFolder = Path.Combine(AppContext.BaseDirectory, "Data");
            if (!Directory.Exists(pathFolder)) Directory.CreateDirectory(pathFolder);
            var pathFile = Path.Combine(pathFolder, "useragent.json");
            if (!File.Exists(pathFile))
            {
                _logger.Information("User agent file not found, creating new one.");
                await Update();
                return;
            }

            var userAgentJsonString = await File.ReadAllTextAsync(pathFile);
            var modelLoaded = JsonSerializer.Deserialize<Model>(userAgentJsonString);
            if (modelLoaded is null)
            {
                _logger.Warning("User agent file is corrupted, creating new one.");
                await Update();
                return;
            }
            _userAgentList = modelLoaded.UserAgentList;
            _dateTime = modelLoaded.DateTime;

            if (_dateTime < DateTime.Now || _userAgentList.Count < 100)
            {
                _logger.Information("User agent file is outdated, updating.");
                await Update();
            }
        }

        public string Get()
        {
            lock (_lock)
            {
                if (_userAgentList.Count == 0)
                {
                    return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";
                }
                var index = rnd.Next(0, _userAgentList.Count);
                var result = _userAgentList[index];
                _userAgentList.RemoveAt(index);
                Save();
                return result;
            }
        }

        private readonly Random rnd = new();

        private sealed class Model
        {
            public List<string> UserAgentList { get; set; } = [];
            public DateTime DateTime { get; set; }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}