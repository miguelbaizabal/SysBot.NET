using Discord;
using Discord.Commands;
using Discord.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    public class HelpModule(CommandService commandService) : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService = commandService;

        [Command("help")]
        [Summary("Muestra los comandos disponibles.")]
        public async Task HelpAsync(int page = 1)
        {
            var mgr = SysCordSettings.Manager;
            var app = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
            var owner = app.Owner.Id;
            var uid = Context.User.Id;

            var modules = _commandService.Modules.ToList();
            var moduleList = new Dictionary<string, Dictionary<string, string>>();

            foreach (var module in modules)
            {
                var moduleName = module.Name;
                var commandDict = new Dictionary<string, string>();

                foreach (var command in module.Commands)
                {
                    if (command.CheckPreconditionsAsync(Context).GetAwaiter().GetResult().IsSuccess)
                    {
                        if (command.Attributes.Any(a => a is RequireOwnerAttribute) && owner != uid)
                            continue;
                        if (command.Attributes.Any(a => a is RequireSudoAttribute) && !mgr.CanUseSudo(uid))
                            continue;

                        var commandName = command.Name;
                        var commandSummary = command.Summary ?? "Sin descripción disponible.";

                        if (!commandDict.ContainsKey(commandName))
                            commandDict.Add(commandName, commandSummary);
                    }
                }

                if (commandDict.Count > 0)
                {
                    var moduleSanitizedName = moduleName.Split('`')[0];

                    var uniqueModuleName = moduleSanitizedName;
                    var count = 1;
                    while (moduleList.ContainsKey(uniqueModuleName))
                    {
                        uniqueModuleName = $"{moduleSanitizedName}_{count}";
                        count++;
                    }

                    moduleList.Add(uniqueModuleName, commandDict);
                }
            }

            var sortedModules = moduleList.OrderByDescending(x => x.Key.StartsWith("TradeModule")).ThenBy(x => x.Key).ToList();

            var pages = new List<string>();
            var currentPage = new StringBuilder();
            var lineCount = 0;

            foreach (var module in sortedModules)
            {
                currentPage.AppendLine($"**{module.Key}**");
                lineCount++;

                foreach (var command in module.Value)
                {
                    currentPage.AppendLine($"`{command.Key}` - {command.Value}");
                    lineCount++;

                    if (lineCount >= 45)
                    {
                        pages.Add(currentPage.ToString());
                        currentPage.Clear();
                        lineCount = 0;
                    }
                }

                if (lineCount > 0)
                {
                    currentPage.AppendLine();
                    lineCount++;
                }
            }

            if (currentPage.Length > 0)
                pages.Add(currentPage.ToString());

            var pageCount = pages.Count;
            if (page < 1 || page > pageCount)
            {
                await ReplyAsync($"Número de página inválido. Por favor, especifique un número entre 1 y {pageCount}.");
                return;
            }

            var footerText = $"Página {page}/{pageCount}";
            if (page < pageCount)
                footerText += $" | Escribe `help {page + 1}` para la siguiente página.";

            var embedBuilder = new EmbedBuilder()
                .WithTitle("Comandos Disponibles")
                .WithColor(Color.Blue)
                .WithDescription(pages[page - 1])
                .WithFooter(footerText);

            try
            {
                var dmChannel = await Context.User.CreateDMChannelAsync();
                await dmChannel.SendMessageAsync(embed: embedBuilder.Build());
                await ReplyAsync($"{Context.User.Mention}, ¡Te he enviado un DM con la información de ayuda!");
            }
            catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.Forbidden)
            {
                await ReplyAsync($"{Context.User.Mention}, No pude enviarte un DM porque tienes los DMs desactivados. Por favor, activa los DMs e inténtalo de nuevo.");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Ocurrió un error mientras se enviaba el DM: {ex.Message}");
            }

            if (Context.Message is IUserMessage userMessage)
                await userMessage.DeleteAsync().ConfigureAwait(false);
        }

        [Command("help")]
        [Summary("Muestra información sobre un comando específico.")]
        public async Task HelpAsync([Summary("El comando para el cual obtener información.")] string command)
        {
            var searchResult = _commandService.Search(Context, command);

            if (!searchResult.IsSuccess)
            {
                await ReplyAsync($"Lo siento, no pude encontrar un comando como **{command}**.");
                return;
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Ayuda para {command}")
                .WithColor(Color.Blue);

            foreach (var match in searchResult.Commands)
            {
                var cmd = match.Command;

                var parameters = cmd.Parameters.Select(p => $"`{p.Name}` - {p.Summary}");
                var parameterSummary = string.Join("\n", parameters);

                embedBuilder.AddField(cmd.Name, $"{cmd.Summary}\n\n**Parámetros:**\n{parameterSummary}", false);
            }

            try
            {
                var dmChannel = await Context.User.CreateDMChannelAsync();
                await dmChannel.SendMessageAsync(embed: embedBuilder.Build());
                await ReplyAsync($"{Context.User.Mention}, ¡Te he enviado un DM con la información de ayuda para el comando **{command}**!");
            }
            catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.Forbidden)
            {
                await ReplyAsync($"{Context.User.Mention}, No pude enviarte un DM porque tienes los DMs desactivados. Por favor, activa los DMs e inténtalo de nuevo.");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Ocurrió un error mientras se enviaba el DM: {ex.Message}");
            }

            if (Context.Message is IUserMessage userMessage)
                await userMessage.DeleteAsync().ConfigureAwait(false);
        }
    }
}
