using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterVoice.Data
{
    public class JtcChannel
    {
        [PrimaryKey]
        public string ChannelIdStorage { get; set; }

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
    }
}
