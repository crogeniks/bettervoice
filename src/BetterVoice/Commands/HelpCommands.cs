using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterVoice.Commands
{
    [Group("bv")]
    public class HelpCommands : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;
        private readonly DataManager _dataManager;

        public HelpCommands(CommandService commandService, DataManager dataManager)
        {
            _commandService = commandService;
            _dataManager = dataManager;
        }

        [Command("help")]
        [Summary("Show this embed")]
        public async Task Help()
        {
            List<CommandInfo> commands = _commandService.Commands.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();

            foreach (CommandInfo command in commands)
            {
                // Get the command Summary attribute information
                string embedFieldText = command.Summary ?? "No description available\n";

                embedBuilder.AddField($"?bv {command.Name} {string.Join(" ", command.Parameters.Select(p => p.Name))} ", embedFieldText);
            }

            await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        }
    }
}
