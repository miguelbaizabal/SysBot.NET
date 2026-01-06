using Discord;
using Discord.Net;
using Discord.Commands;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

[Summary("Añade a la cola nuevos intercambios de dumpeo")]
public class DumpModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [Command("dump")]
    [Alias("d")]
    [Summary("Dumpea los Pokémon que muestras mediante un intercambio en línea.")]
    [RequireQueueRole(nameof(DiscordManager.RolesDump))]
    public async Task DumpAsync(int code)
    {
        if (await CheckUserInQueueAsync())
            return;

        var sig = Context.User.GetFavor();
        var lgcode = Info.GetRandomLGTradeCode();
        await QueueHelper<T>.AddToQueueAsync(
            Context,
            code,
            Context.User.Username,
            sig,
            new T(),
            PokeRoutineType.Dump,
            PokeTradeType.Dump,
            Context.User,
            isBatchTrade: false,
            batchTradeNumber: 1,
            totalBatchTrades: 1,
            isMysteryEgg: false,
            lgcode: lgcode);

        _ = DeleteMessageAsync(Context.Message, 2000);
    }

    [Command("dump")]
    [Alias("d")]
    [Summary("Dumpea los Pokémon que muestras mediante un intercambio en línea.")]
    [RequireQueueRole(nameof(DiscordManager.RolesDump))]
    public async Task DumpAsync([Summary("Código de intercambio")][Remainder] string code)
    {
        if (await CheckUserInQueueAsync())
            return;

        int tradeCode = Util.ToInt32(code);
        var sig = Context.User.GetFavor();
        await QueueHelper<T>.AddToQueueAsync(Context, tradeCode == 0 ? Info.GetRandomTradeCode(Context.User.Id) : tradeCode, Context.User.Username, sig, new T(), PokeRoutineType.Dump, PokeTradeType.Dump);
    }

    [Command("dump")]
    [Alias("d")]
    [Summary("Dumpea los Pokémon que muestras mediante un intercambio en línea.")]
    [RequireQueueRole(nameof(DiscordManager.RolesDump))]
    public async Task DumpAsync()
    {
        if (await CheckUserInQueueAsync())
            return;

        var code = Info.GetRandomTradeCode(Context.User.Id);
        await DumpAsync(code);
    }

    [Command("dumpList")]
    [Alias("dl", "dq")]
    [Summary("Imprime los usuarios en la cola de dumpeo.")]
    [RequireSudo]
    public async Task GetListAsync()
    {
        string msg = Info.GetTradeList(PokeRoutineType.Dump);
        var embed = new EmbedBuilder();
        embed.AddField(x =>
        {
            x.Name = "Intercambios Pendientes";
            x.Value = msg;
            x.IsInline = false;
        });
        await ReplyAsync("Estos son los usuarios que están actualmente esperando:", embed: embed.Build()).ConfigureAwait(false);
    }

    private async Task<bool> CheckUserInQueueAsync()
    {
        var userID = Context.User.Id;
        if (Info.IsUserInQueue(userID))
        {
            await ReplyAsync("Ya tienes un intercambio existente en la cola. Por favor, espera hasta que sea procesado.").ConfigureAwait(false);
            return true;
        }
        return false;
    }

    private static async Task DeleteMessageAsync(IMessage message, int delay)
    {
        await Task.Delay(delay);
        try
        {
            await message.DeleteAsync();
        }
        catch (HttpException)
        {
            // Ignore exceptions if the message was already deleted or we don't have permission
        }
    }
}
