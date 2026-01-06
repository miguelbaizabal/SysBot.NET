using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

public class PingModule : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    [Summary("Hace que el bot responda, indicando que está funcionando.")]
    public async Task PingAsync()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Respuesta de Ping")
            .WithDescription("¡Pong! El bot está funcionando sin problemas.")
            .WithImageUrl("https://i.gifer.com/QgxJ.gif")
            .WithColor(Color.Green)
            .Build();

        await ReplyAsync(embed: embed).ConfigureAwait(false);
    }
}
