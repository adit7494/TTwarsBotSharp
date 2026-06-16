using MainCore.Enums;
using StronglyTypedIds;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace MainCore.Entities
{
    public class Account
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Username { get; set; }
        public string Server { get; set; }

        /// <summary>
        /// The type of Travian server (Travian or TTWars).
        /// TTWars servers have different HTML structure and faster gameplay.
        /// </summary>
        public ServerType ServerType { get; set; } = ServerType.Travian;

        public AccountInfo Info { get; set; }

        public ICollection<AccountSetting> Settings { get; set; }
        public ICollection<Access> Accesses { get; set; }
        public ICollection<Village> Villages { get; set; }

        public ICollection<HeroItem> HeroItems { get; set; }
        public ICollection<Farm> FarmLists { get; set; }
    }

    [StronglyTypedId]
    public partial struct AccountId
    { }
}