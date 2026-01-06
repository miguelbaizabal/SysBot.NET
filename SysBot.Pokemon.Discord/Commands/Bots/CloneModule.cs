using Discord;
using Discord.Commands;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

[Summary("Añade a la cola nuevos intercambios de clonación")]
public class CloneModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [Command("clone")]
    [Alias("c")]
    [Summary("Clona los Pokémon que muestras mediante un intercambio en línea.")]
    [RequireQueueRole(nameof(DiscordManager.RolesClone))]
    public async Task CloneAsync(int code)
    {
        // Check if the user is already in the queue
        var userID = Context.User.Id;
        if (Info.IsUserInQueue(userID))
        {
            await ReplyAsync("Ya tienes un intercambio existente en la cola. Por favor, espera hasta que sea procesado.").ConfigureAwait(false);
            return;
        }

        var sig = Context.User.GetFavor();
        var lgcode = Info.GetRandomLGTradeCode();

        // Add to queue asynchronously
        _ = QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, new T(), PokeRoutineType.Clone, PokeTradeType.Clone, Context.User, false, 1, 1, false, false, lgcode: lgcode);

        // Immediately send a confirmation message without waiting
        var confirmationMessage = await ReplyAsync("Procesando tu solicitud de clonación...").ConfigureAwait(false);

        // Use a fire-and-forget approach for the delay and deletion
        _ = Task.Delay(2000).ContinueWith(async _ =>
        {
            if (Context.Message is IUserMessage userMessage)
                await userMessage.DeleteAsync().ConfigureAwait(false);

            if (confirmationMessage != null)
                await confirmationMessage.DeleteAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    [Command("clone")]
    [Alias("c")]
    [Summary("Clona los Pokémon que muestras mediante un intercambio en línea.")]
    [RequireQueueRole(nameof(DiscordManager.RolesClone))]
    public async Task CloneAsync([Summary("Código de intercambio")][Remainder] string code)
    {
        // Check if the user is already in the queue
        var userID = Context.User.Id;
        if (Info.IsUserInQueue(userID))
        {
            await ReplyAsync("Ya tienes un intercambio existente en la cola. Por favor, espera hasta que sea procesado.").ConfigureAwait(false);
            return;
        }

        int tradeCode = Util.ToInt32(code);
        var sig = Context.User.GetFavor();
        var lgcode = Info.GetRandomLGTradeCode();

        // Add to queue asynchronously
        _ = QueueHelper<T>.AddToQueueAsync(Context, tradeCode == 0 ? Info.GetRandomTradeCode(userID) : tradeCode, Context.User.Username, sig, new T(), PokeRoutineType.Clone, PokeTradeType.Clone, Context.User, false, 1, 1, false, false, lgcode: lgcode);

        // Immediately send a confirmation message without waiting
        var confirmationMessage = await ReplyAsync("Procesando tu solicitud de clonación...").ConfigureAwait(false);

        // Use a fire-and-forget approach for the delay and deletion
        _ = Task.Delay(2000).ContinueWith(async _ =>
        {
            if (Context.Message is IUserMessage userMessage)
                await userMessage.DeleteAsync().ConfigureAwait(false);

            if (confirmationMessage != null)
                await confirmationMessage.DeleteAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    [Command("clone")]
    [Alias("c")]
    [Summary("Clona los Pokémon que muestras mediante un intercambio en línea.")]
    [RequireQueueRole(nameof(DiscordManager.RolesClone))]
    public Task CloneAsync()
    {
        var userID = Context.User.Id;
        var code = Info.GetRandomTradeCode(userID);
        return CloneAsync(code);
    }

    [Command("cloneList")]
    [Alias("cl", "cq")]
    [Summary("Imprime los usuarios en la cola de clonación.")]
    [RequireSudo]
    public async Task GetListAsync()
    {
        string msg = Info.GetTradeList(PokeRoutineType.Clone);
        var embed = new EmbedBuilder();
        embed.AddField(x =>
        {
            x.Name = "Intercambios Pendientes";
            x.Value = msg;
            x.IsInline = false;
        });
        await ReplyAsync("Estos son los usuarios que están actualmente esperando:", embed: embed.Build()).ConfigureAwait(false);
    }
}
