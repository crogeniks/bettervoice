using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterVoice.Data
{
    public class VoiceChannel
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string ChannelIdStorage { get; set; }
        public string OwnerIdStorage { get; set; }


        [Ignore]
        public ulong ChannelId
        {
            get
            {
                return string.IsNullOrWhiteSpace(ChannelIdStorage) ? 0 : ulong.Parse(ChannelIdStorage);
            }
            set
            {
                ChannelIdStorage = value.ToString();
            }
        }


        [Ignore]
        public ulong OwnerId
        {
            get
            {
                return string.IsNullOrWhiteSpace(OwnerIdStorage) ? 0 : ulong.Parse(OwnerIdStorage);
            }
            set
            {
                OwnerIdStorage = value.ToString();
            }
        }
    }
}
