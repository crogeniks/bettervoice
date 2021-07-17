using BetterVoice.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterVoice.Commands
{
    [Group("bv")]
    public class AdminCommands : ModuleBase<SocketCommandContext>
    {
        private readonly DataManager _dataManager;

        public AdminCommands(DataManager dataManager)
        {
            _dataManager = dataManager;
        }

        [Command("setup", RunMode = RunMode.Async)]
        [Summary("Admin command. Initial bot setup")]
        public async Task SetupAsync(IRole adminRole, IRole minimumRole)
        {
            var config = await _dataManager.GetConfigurationAsync();

            if (config == null)
            {
                config = new Configuration();
            }
            else
            {
                var user = Context.User as SocketGuildUser;
                var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == config.AdminRoleId);

                if (!user.Roles.Contains(role))
                {
                    await ReplyAsync("You do not have the permission to update the bot's config");
                    return;
                }
            }

            config.AdminRoleId = adminRole.Id;
            config.MinimumRoleId = minimumRole.Id;

            await _dataManager.SaveConfiguration(config);

            await ReplyAsync("Config successfully updated");
        }

        [Command("add", RunMode = RunMode.Async)]
        [Summary("Admin command. Add role that can add JTC channels")]
        public async Task AddCreators(IRole creatorRole)
        {
            var config = await _dataManager.GetConfigurationAsync();

            if (config == null)
            {
                await ReplyAsync("call configuration first.");
                return;
            }
            else
            {
                var user = Context.User as SocketGuildUser;
                var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == config.AdminRoleId);

                if (!user.Roles.Contains(role))
                {
                    await ReplyAsync("You do not have the permission to update the bot's config");
                    return;
                }
            }

            var creators = config.CreatorIds;

            if (!creators.Contains(creatorRole.Id))
            {
                creators.Add(creatorRole.Id);

                config.Creators = string.Join(',', creators);

                await _dataManager.SaveConfiguration(config);

                await ReplyAsync("Config successfully updated");
            }
            else
            {
                await ReplyAsync("Role can already create VCs");
            }
        }

        [Command("remove", RunMode = RunMode.Async)]
        [Summary("Admin command. Remove role that can add JTC channels")]
        public async Task RemoveCreators(IRole creatorRole)
        {
            var config = await _dataManager.GetConfigurationAsync();

            if (config == null)
            {
                await ReplyAsync("call configuration first.");
                return;
            }
            else
            {
                var user = Context.User as SocketGuildUser;
                var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == config.AdminRoleId);

                if (!user.Roles.Contains(role))
                {
                    await ReplyAsync("You do not have the permission to update the bot's config");
                    return;
                }
            }

            var creators = config.CreatorIds;

            if (creators.Contains(creatorRole.Id))
            {
                var newCreators = creators.Where(c => c != creatorRole.Id);

                config.Creators = string.Join(',', newCreators);

                await _dataManager.SaveConfiguration(config);
                await ReplyAsync("Config successfully updated");
            }
        }

        [Command("create", RunMode = RunMode.Async)]
        [Summary("Admin command. Create a new JTC channel")]
        public async Task CreateAsync()
        {
            var config = await _dataManager.GetConfigurationAsync();

            if (config == null)
            {
                await ReplyAsync("Please configure bot before creating channels");
                return;
            }

            var creators = config.CreatorIds;
            creators.Add(config.AdminRoleId);

            var user = Context.User as SocketGuildUser;

            if (!user.Roles.Any(r => creators.Contains(r.Id)))
            {
                await ReplyAsync("You do not have the permission to create channels");
                return;
            }

            var vc = await Context.Guild.CreateVoiceChannelAsync("Join to Create");

            await _dataManager.SaveJtcChannel(new JtcChannel
            {
                ChannelId = vc.Id
            });

            await ReplyAsync("New Join To Create Channel created");
        }

        [Command("create", RunMode = RunMode.Async)]
        [Summary("Admin command. Create a new JTC channel under the specified category")]
        public async Task CreateAsync(ICategoryChannel categoryId)
        {
            var config = await _dataManager.GetConfigurationAsync();

            if (config == null)
            {
                await ReplyAsync("Please configure bot before creating channels");
                return;
            }

            var creators = config.CreatorIds;
            creators.Add(config.AdminRoleId);

            var user = Context.User as SocketGuildUser;

            if (!user.Roles.Any(r => creators.Contains(r.Id)))
            {
                await ReplyAsync("You do not have the permission to create channels");
                return;
            }

            var vc = await Context.Guild.CreateVoiceChannelAsync("Join to Create", c => c.CategoryId = categoryId.Id);

            await _dataManager.SaveJtcChannel(new JtcChannel
            {
                ChannelId = vc.Id
            });

            await ReplyAsync("New Join To Create Channel created");
        }
    }
}
