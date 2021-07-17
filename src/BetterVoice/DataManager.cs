using BetterVoice.Data;
using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace BetterVoice
{
    public class DataManager
    {
        SQLiteAsyncConnection _conn;

        public DataManager()
        {
        }

        public async Task<Configuration> GetConfigurationAsync()
        {
            var conn = await GetConnectionAsync();

            return await conn.Table<Configuration>().FirstOrDefaultAsync();
        }

        public async Task SaveConfiguration(Configuration config)
        {
            var conn = await GetConnectionAsync();

            await conn.DeleteAllAsync<Configuration>();

            await conn.InsertAsync(config);

        }

        public async Task SaveJtcChannel(JtcChannel vc)
        {
            var conn = await GetConnectionAsync();

            await conn.InsertAsync(vc);
        }

        public async Task SaveUserChannel(VoiceChannel vc)
        {
            var conn = await GetConnectionAsync();

            await conn.InsertOrReplaceAsync(vc);
        }

        public async Task SaveUserSettings(UserSettings settings)
        {
            var conn = await GetConnectionAsync();

            await conn.InsertOrReplaceAsync(settings);
        }

        public async Task<List<JtcChannel>> GetJtcChannels()
        {
            var conn = await GetConnectionAsync();

            return await conn.Table<JtcChannel>().ToListAsync();
        }

        public async Task<VoiceChannel> GetUserChannelFromOwnerId(ulong ownerId)
        {
            var conn = await GetConnectionAsync();
            var ownerIdForStorage = ownerId.ToString(CultureInfo.InvariantCulture);
            return await conn.Table<VoiceChannel>().Where(vc => vc.OwnerIdStorage == ownerIdForStorage).FirstOrDefaultAsync();
        }

        public async Task<VoiceChannel> GetVoiceChannelFromChannelId(ulong channelId)
        {
            var conn = await GetConnectionAsync();
            var channelIdForStorage = channelId.ToString(CultureInfo.InvariantCulture);

            var userChannel = await conn.Table<VoiceChannel>().Where(t => t.ChannelIdStorage == channelIdForStorage).FirstOrDefaultAsync();

            return userChannel;
        }

        public async Task<UserSettings> GetUserSettings(ulong userId)
        {
            var conn = await GetConnectionAsync();
            var userIdForStorage = userId.ToString(CultureInfo.InvariantCulture);

            return await conn.Table<UserSettings>().Where(t => t.OwnerIdForStorage == userIdForStorage).FirstOrDefaultAsync();
        }


        public async Task DesactivateUserChannel(ulong channelId)
        {
            var conn = await GetConnectionAsync();
            var channelIdForStorage = channelId.ToString(CultureInfo.InvariantCulture);

            await conn.Table<VoiceChannel>().DeleteAsync(t => t.ChannelIdStorage == channelIdForStorage);
        }

        private async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_conn == null)
            {
                _conn = new SQLiteAsyncConnection("voice.dat");
                await Setup(_conn);
            }

            return _conn;
        }

        private async Task Setup(SQLiteAsyncConnection conn)
        {
            await conn.CreateTableAsync<Configuration>();
            await conn.CreateTableAsync<VoiceChannel>();
            await conn.CreateTableAsync<UserSettings>();
            await conn.CreateTableAsync<JtcChannel>();
        }
    }
}
