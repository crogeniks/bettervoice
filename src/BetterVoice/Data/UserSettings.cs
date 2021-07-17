using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterVoice.Data
{
    public class UserSettings
    {
        [PrimaryKey]
        public string OwnerIdForStorage { get; set; }

        public string ChannelName { get; set; }

        [Ignore]
        public ulong OwnerId
        {
            get
            {
                return string.IsNullOrWhiteSpace(OwnerIdForStorage) ? 0 : ulong.Parse(OwnerIdForStorage);
            }
            set
            {
                OwnerIdForStorage = value.ToString();
            }
        }
    }
}
