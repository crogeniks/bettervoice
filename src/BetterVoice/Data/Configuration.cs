using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterVoice.Data
{
    public class Configuration
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string AdminRoleIdStorage { get; set; }
        public string MinimumRoleIdStorage { get; set; }

        public string Creators { get; set; } = string.Empty;

        [Ignore]
        public ulong AdminRoleId
        {
            get
            {
                return string.IsNullOrWhiteSpace(AdminRoleIdStorage) ? 0 : ulong.Parse(AdminRoleIdStorage);
            }
            set
            {
                AdminRoleIdStorage = value.ToString();
            }
        }

        [Ignore]
        public ulong MinimumRoleId
        {
            get
            {
                return string.IsNullOrWhiteSpace(MinimumRoleIdStorage) ? 0 : ulong.Parse(MinimumRoleIdStorage);
            }
            set
            {
                MinimumRoleIdStorage = value.ToString();
            }
        }

        [Ignore]
        public List<ulong> CreatorIds => Creators?.Split(',').Where(id => !string.IsNullOrWhiteSpace(id)).Select(ulong.Parse).ToList() ?? new List<ulong>();
    }
}
