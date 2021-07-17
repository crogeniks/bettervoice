using BetterVoice.Data;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterVoice.Commands
{
    [Group("bv")]
    public class VoiceCommands : ModuleBase<SocketCommandContext>
    {
        private readonly DataManager _dataManager;

        public VoiceCommands(DataManager dataManager)
        {
            _dataManager = dataManager;
        }

        [Command("name", RunMode = RunMode.Async)]
        [Summary("Change the channel's name")]
        public async Task ChangeChannelName([Remainder] string name)
        {
            var userChannel = await _dataManager.GetUserChannelFromOwnerId(Context.User.Id);
            var userSettings = await _dataManager.GetUserSettings(Context.User.Id);

            if (userChannel == null || userChannel.ChannelId == 0)
            {
                await Reply("You do not own a channel");
                return;
            }

            if (name.Length < 2 || name.Length > 32)
            {
                await Reply("Channel name must be between 2 and 32 characters");
                return;
            }

            var voiceChannel = await (Context.User as IGuildUser).Guild.GetChannelAsync(userChannel.ChannelId);
            if (voiceChannel == null)
            {
                //something bad happened. User is in a VC, but not the one saved in DB.
                //Delete DB channel, try to see if DB contains any other rows for user.

                userChannel = await _dataManager.GetUserChannelFromOwnerId(Context.User.Id);
            }

            try
            {
                voiceChannel = await (Context.User as IGuildUser).Guild.GetChannelAsync(userChannel.ChannelId);
                await voiceChannel.ModifyAsync(vc => vc.Name = name, new RequestOptions { RetryMode = RetryMode.AlwaysFail });
            }
            catch (RateLimitedException ex)
            {
                await Reply("Unable to change channel name. Retry in 10 mins");
                return;
            }

            userSettings.ChannelName = name;
            await _dataManager.SaveUserSettings(userSettings);

            await Reply("Channel name changed");
        }

        [Command("limit", RunMode = RunMode.Async)]
        [Summary("Change the channel's connection limit")]
        public async Task ChangeChannelLimit(int count)
        {
            var userChannel = await _dataManager.GetUserChannelFromOwnerId(Context.User.Id);

            if (userChannel == null || userChannel.ChannelId == 0)
            {
                await Reply("You do not own a channel");
                return;
            }

            if (count > 99)
            {
                await Reply("Limit must be below 99");
                return;
            }

            var voiceChannel = await (Context.User as IGuildUser).Guild.GetChannelAsync(userChannel.ChannelId);
            try
            {
                await ((IVoiceChannel)voiceChannel).ModifyAsync(vc =>
                {
                    if (count < 1)
                    {
                        vc.UserLimit = null;
                    }
                    else
                    {
                        vc.UserLimit = count;
                    }
                }
                , new RequestOptions { RetryMode = RetryMode.AlwaysFail });
                await Reply($"Changed channel limit to {count}");
            }
            catch (RateLimitedException ex)
            {
                await Reply("Unable to change channel limit. Retry in 10 mins");
                return;
            }

        }

        [Command("lock", RunMode = RunMode.Async)]
        [Summary("Locks the channel")]
        public async Task LockChannel()
        {
            var userChannel = await _dataManager.GetUserChannelFromOwnerId(Context.User.Id);
            var config = await _dataManager.GetConfigurationAsync();

            if (userChannel == null || userChannel.ChannelId == 0)
            {
                await Reply("You do not own a channel");
                return;
            }
            var voiceChannel = await (Context.User as IGuildUser).Guild.GetChannelAsync(userChannel.ChannelId);
            try
            {
                var user = Context.User as SocketGuildUser;
                var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(r => r.Id == config.MinimumRoleId);

                var perms = voiceChannel.GetPermissionOverwrite(role);

                var newperms = perms.Value.Modify(connect: PermValue.Deny);

                await ((IVoiceChannel)voiceChannel).AddPermissionOverwriteAsync(role, newperms);
                await Reply($"Locked Channel");
            }
            catch (RateLimitedException ex)
            {
                await Reply("Unable to change channel limit. Retry in 10 mins");
            }

        }

        [Command("unlock", RunMode = RunMode.Async)]
        [Summary("Unlocks the channel")]
        public async Task UnlockChannel()
        {
            var userChannel = await _dataManager.GetUserChannelFromOwnerId(Context.User.Id);
            var config = await _dataManager.GetConfigurationAsync();

            if (userChannel == null || userChannel.ChannelId == 0)
            {
                await Reply("You do not own a channel");
                return;
            }
            var voiceChannel = await (Context.User as IGuildUser).Guild.GetChannelAsync(userChannel.ChannelId);
            try
            {
                var user = Context.User as SocketGuildUser;
                var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(r => r.Id == config.MinimumRoleId);

                var perms = voiceChannel.GetPermissionOverwrite(role);

                var newperms = perms.Value.Modify(connect: PermValue.Allow);

                await ((IVoiceChannel)voiceChannel).AddPermissionOverwriteAsync(role, newperms);
                await Reply($"Channel unlocked");
            }
            catch (RateLimitedException ex)
            {
                await Reply("Unable to change channel limit. Retry in 10 mins");
                return;
            }

        }

        [Command("permit", RunMode = RunMode.Async)]
        [Summary("Allows the specified user to join")]
        public async Task PermitUser(IUser userToPermit)
        {
            var userChannel = await _dataManager.GetUserChannelFromOwnerId(Context.User.Id);

            if (userChannel == null || userChannel.ChannelId == 0)
            {
                await Reply("You do not own a channel");
                return;
            }

            var voiceChannel = await (Context.User as IGuildUser).Guild.GetChannelAsync(userChannel.ChannelId);

            try
            {
                var perms = new OverwritePermissions(connect: PermValue.Allow);

                await ((IVoiceChannel)voiceChannel).AddPermissionOverwriteAsync(userToPermit, perms);
                await Reply($"Added {userToPermit.Username}");

            }
            catch (RateLimitedException ex)
            {
                await Reply("Unable to change channel limit. Retry in 10 mins");
            }
        }

        [Command("reject", RunMode = RunMode.Async)]
        [Summary("Deny access and remove the specified user from the voice channel")]
        public async Task RejectUser(IUser userToReject)
        {
            var userChannel = await _dataManager.GetUserChannelFromOwnerId(Context.User.Id);

            if (userChannel == null || userChannel.ChannelId == 0)
            {
                await Reply("You do not own a channel");
                return;
            }

            var voiceChannel = await (Context.User as IGuildUser).Guild.GetChannelAsync(userChannel.ChannelId);

            try
            {
                var perms = new OverwritePermissions(connect: PermValue.Deny);

                await ((IVoiceChannel)voiceChannel).AddPermissionOverwriteAsync(userToReject, perms);

                var channelUser = await voiceChannel.GetUserAsync(userToReject.Id);

                if (channelUser != null)
                {
                    await channelUser.ModifyAsync(u => u.Channel = null);
                }

                await Reply($"Denied {userToReject.Username}");
            }
            catch (RateLimitedException ex)
            {
                await Reply("Unable to change channel limit. Retry in 10 mins");
                return;
            }
        }

        [Command("claim", RunMode = RunMode.Async)]
        [Summary("Claim the current temporary channel")]
        public async Task ClaimChannel()
        {
            var userChannel = await _dataManager.GetUserChannelFromOwnerId(Context.User.Id);

            if (userChannel != null && userChannel.ChannelId != 0)
            {
                await Reply("You already own a channel");
                return;
            }

            var voiceChannel = (Context.User as IGuildUser).VoiceChannel;
            var voiceChannelSettings = await _dataManager.GetVoiceChannelFromChannelId(voiceChannel.Id);

            if (voiceChannelSettings == null)
            {
                await Reply("You can only claim temporary Voice Channels");
                return;
            }

            var users = (await voiceChannel.GetUsersAsync().ToListAsync()).FirstOrDefault();

            var ownerUser = users.FirstOrDefault(u => u.Id == voiceChannelSettings.OwnerId);

            if (ownerUser?.VoiceChannel?.Id == voiceChannelSettings.ChannelId)
            {
                await Reply("You cannot claim a channel where the owner is still connected");
                return;
            }

            voiceChannelSettings.OwnerId = Context.User.Id;
            await _dataManager.SaveUserChannel(voiceChannelSettings);

            await Reply($"You successfully claimed the channel {voiceChannel.Name}");
            await voiceChannel.AddPermissionOverwriteAsync(Context.User, OverwritePermissions.AllowAll(voiceChannel), null);
        }

        private Task Reply(string text)
        {
#if DEBUG
            return Task.CompletedTask;
#else
            return ReplyAsync(text);
#endif
        }
    }
}
