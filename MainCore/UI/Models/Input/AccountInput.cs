using DynamicData;
using System.Collections.ObjectModel;

namespace MainCore.UI.Models.Input
{
    public partial class AccountInput : ReactiveObject
    {
        public AccountId Id { get; set; }

        [Reactive]
        private string _username = "";

        [Reactive]
        private string _server = "";

        [Reactive]
        private ServerType _serverType = ServerType.Travian;

        public ObservableCollection<AccessInput> Accesses { get; } = new();

        public void SetAccesses(IEnumerable<AccessInput> accesses)
        {
            Accesses.Clear();
            Accesses.AddRange(accesses);
        }

        public void Clear()
        {
            Server = "";
            Username = "";
            ServerType = ServerType.Travian;
            Accesses.Clear();
        }
    }

    public static partial class AccountInputExtension
    {
        public static AccountInput ToInput(this AccountDto dto)
        {
            var input = new AccountInput
            {
                Id = dto.Id,
                Username = dto.Username,
                Server = dto.Server,
                ServerType = dto.ServerType,
            };
            input.SetAccesses(dto.Accesses.Select(a => a.ToInput()));
            return input;
        }

        public static AccountDto ToDto(this AccountInput input)
        {
            return new AccountDto
            {
                Id = input.Id,
                Username = input.Username.Sanitize(),
                Server = input.Server.GetServerUrl(),
                ServerType = input.ServerType,
                Accesses = input.Accesses.Select(a => a.ToDto()).ToList(),
            };
        }
    }
}