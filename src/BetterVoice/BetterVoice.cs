using BetterVoice.Commands;
using BetterVoice.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterVoice
{
    public class BetterVoice
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task StartAsync()
        {
            _client = new DiscordSocketClient();

            _commands = new CommandService(new CommandServiceConfig
            {
                // Again, log level:
                LogLevel = LogSeverity.Verbose,

                // There's a few more properties you can set,
                // for example, case-insensitive commands.
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,

            });

            _services = ConfigureServices();

            _client.Log += OnDiscordLog;
            _client.Connected += OnConnected;

            _client.UserVoiceStateUpdated += OnVoice;
            _client.MessageReceived += OnMessageReceived;

            _commands.Log += OnDiscordLog;

            await _commands.AddModuleAsync<AdminCommands>(_services);
            await _commands.AddModuleAsync<VoiceCommands>(_services);
            await _commands.AddModuleAsync<HelpCommands>(_services);

            await _client.LoginAsync(TokenType.Bot, "Your token here");

            await _client.StartAsync();

            while (true)
            {
                await Task.Delay(Int32.MaxValue);
            }
        }

        private Task OnConnected()
        {
            Task.Run(() => _client.DownloadUsersAsync(_client.Guilds));
            return Task.CompletedTask;
        }

        private async Task OnVoice(SocketUser user, SocketVoiceState currentState, SocketVoiceState newState)
        {
            if (user.IsBot)
            {
                return;
            }

            var dataManager = _services.GetRequiredService<DataManager>();

            var jtcChannels = await dataManager.GetJtcChannels();
            var userSettings = await dataManager.GetUserSettings(user.Id);

            var guildUser = user as IGuildUser;
            var guidId = guildUser.Guild.Id;

            //user joins JTC
            if (jtcChannels.FirstOrDefault(j => j.ChannelId == newState.VoiceChannel?.Id) != null)
            {
                //user joined a JTC channel handled by bot
                var category = newState.VoiceChannel.CategoryId;

                var channelName = userSettings?.ChannelName ?? $"{user.Username}'s channel";

                var userVC = await _client.GetGuild(guidId).CreateVoiceChannelAsync(channelName, vc => vc.CategoryId = category);

                await guildUser.ModifyAsync(g => g.Channel = userVC);

                await ((IVoiceChannel)userVC).AddPermissionOverwriteAsync(user, OverwritePermissions.AllowAll(userVC), null);

                if (userSettings == null)
                {
                    userSettings = new UserSettings
                    {
                        OwnerId = user.Id,
                        ChannelName = channelName
                    };
                }

                var vc = new VoiceChannel
                {
                    Id = Guid.NewGuid().ToString(),
                    ChannelId = userVC.Id,
                    OwnerId = user.Id
                };

                await dataManager.SaveUserSettings(userSettings);
                await dataManager.SaveUserChannel(vc);
            }

            //user leaves voice channel
            if (currentState.VoiceChannel != null && currentState.VoiceChannel != newState.VoiceChannel)
            {
                var userVC = await dataManager.GetVoiceChannelFromChannelId(currentState.VoiceChannel.Id);

                if (userVC != null)
                {
                    if (currentState.VoiceChannel.Users.Count == 0)
                    {
                        await _client.GetGuild(guidId).GetChannel(currentState.VoiceChannel.Id).DeleteAsync();

                        await dataManager.DesactivateUserChannel(currentState.VoiceChannel.Id);
                    }
                    else if(user.Id == userVC.OwnerId)
                    {
                        userVC.OwnerId = 0;
                        await dataManager.SaveUserChannel(userVC);

                        await ((IVoiceChannel)currentState.VoiceChannel).RemovePermissionOverwriteAsync(user);
                    }
                }
            }
        }

        private IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection()
                // Repeat this for all the service classes
                // and other dependencies that your commands might need.
                .AddSingleton<DataManager>();

            // When all your required services are in the collection, build the container.
            // Tip: There's an overload taking in a 'validateScopes' bool to make sure
            // you haven't made any mistakes in your dependency graph.
            return map.BuildServiceProvider();
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            // Bail out if it's a System Message.
            var msg = arg as SocketUserMessage;
            if (msg == null)
            {
                return;
            }

            // We don't want the bot to respond to itself or other bots.
            if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot)
            {
                return;
            }

            int pos = 0;
            if (msg.HasCharPrefix('?', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
            {
                // Create a Command Context.
                var context = new SocketCommandContext(_client, msg);

                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed successfully).
                var result = await _commands.ExecuteAsync(context, pos, _services);

                // Uncomment the following lines if you want the bot
                // to send a message if it failed.
                // This does not catch errors from commands with 'RunMode.Async',
                // subscribe a handler for '_commands.CommandExecuted' to see those.
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    await msg.Channel.SendMessageAsync(result.ErrorReason);
            }

        }

        private Task OnDiscordLog(Discord.LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();

            return Task.CompletedTask;
        }
    }
}
