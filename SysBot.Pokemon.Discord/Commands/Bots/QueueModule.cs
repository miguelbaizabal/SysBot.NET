using Discord;
using Discord.Commands;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

[Summary("Limpia y alterna las funciones de cola.")]
public class QueueModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [Command("queueMode")]
    [Alias("qm")]
    [Summary("Cambia cómo se controla el sistema de cola (manual/threshold/interval).")]
    [RequireSudo]
    public async Task ChangeQueueModeAsync([Summary("Modo de cola")] QueueOpening mode)
    {
        SysCord<T>.Runner.Hub.Config.Queues.QueueToggleMode = mode;
        await ReplyAsync($"Modo de cola cambiado a {mode}.").ConfigureAwait(false);
    }

    [Command("queueClearAll")]
    [Alias("qca", "tca")]
    [Summary("Limpia todos los usuarios de las colas de intercambio.")]
    [RequireSudo]
    public async Task ClearAllTradesAsync()
    {
        Info.ClearAllQueues();
        await ReplyAsync("Se ha limpiado la cola de intercambio.").ConfigureAwait(false);
    }

    [Command("queueClear")]
    [Alias("qc", "tc")]
    [Summary("Limpia al usuario de las colas de intercambio. No eliminará al usuario si está siendo procesado.")]
    public async Task ClearTradeAsync()
    {
        string msg = ClearTrade(Context.User.Id);
        await ReplyAndDeleteAsync(msg, 5, Context.Message).ConfigureAwait(false);
    }

    [Command("queueClearUser")]
    [Alias("qcu", "tcu")]
    [Summary("Limpia al usuario de las colas de intercambio. No eliminará al usuario si está siendo procesado.")]
    [RequireSudo]
    public async Task ClearTradeUserAsync([Summary("Discord user ID")] ulong id)
    {
        string msg = ClearTrade(id);
        await ReplyAsync(msg).ConfigureAwait(false);
    }

    [Command("queueClearUser")]
    [Alias("qcu", "tcu")]
    [Summary("Limpia al usuario de las colas de intercambio. No eliminará al usuario si está siendo procesado.")]
    [RequireSudo]
    public async Task ClearTradeUserAsync([Summary("Nombre de usuario de la persona a limpiar")] string _)
    {
        foreach (var user in Context.Message.MentionedUsers)
        {
            string msg = ClearTrade(user.Id);
            await ReplyAsync(msg).ConfigureAwait(false);
        }
    }

    [Command("queueClearUser")]
    [Alias("qcu", "tcu")]
    [Summary("Limpia al usuario de las colas de intercambio. No eliminará al usuario si está siendo procesado.")]
    [RequireSudo]
    public async Task ClearTradeUserAsync()
    {
        var users = Context.Message.MentionedUsers;
        if (users.Count == 0)
        {
            await ReplyAsync("Ningún usuario mencionado").ConfigureAwait(false);
            return;
        }
        foreach (var u in users)
            await ClearTradeUserAsync(u.Id).ConfigureAwait(false);
    }

    [Command("deleteTradeCode")]
    [Alias("dtc")]
    [Summary("Elimina el código de intercambio almacenado para el usuario.")]
    public async Task DeleteTradeCodeAsync()
    {
        var userID = Context.User.Id;
        string msg = QueueModule<T>.DeleteTradeCode(userID);
        await ReplyAsync(msg).ConfigureAwait(false);
    }

    [Command("queueStatus")]
    [Alias("qs", "ts")]
    [Summary("Verifica la posición del usuario en la cola.")]
    public async Task GetTradePositionAsync()
    {
        var userID = Context.User.Id;
        var tradeEntry = Info.GetDetail(userID);

        string msg;
        if (tradeEntry != null)
        {
            var uniqueTradeID = tradeEntry.UniqueTradeID;
            msg = Context.User.Mention + " - " + Info.GetPositionString(userID, uniqueTradeID, tradeEntry.Type);
        }
        else
        {
            msg = Context.User.Mention + " - No estás actualmente en la cola.";
        }

        await ReplyAndDeleteAsync(msg, 5, Context.Message).ConfigureAwait(false);
    }

    [Command("queueList")]
    [Alias("ql")]
    [Summary("Muestra una lista embebida de la cola actual con especie, tipo de intercambio y nombre de usuario.")]
    [RequireSudo]
    public async Task ListUserQueue()
    {
        var queue = SysCord<T>.Runner.Hub.Queues.Info.GetUserList("{4}|{2}|{3}"); // Species|Type|Username

        if (!queue.Any())
        {
            await ReplyAsync("La lista de cola está vacía.").ConfigureAwait(false);
            return;
        }

        var embedBuilder = new EmbedBuilder()
            .WithTitle($"📋 Cola de intercambio actual ({queue.Count()} usuarios)")
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();

        var queueList = queue.Select((entry, index) =>
        {
            var parts = entry.Split('|');
            var species = parts[0];
            var tradeType = parts[1];
            var username = parts[2];

            return $"`{index + 1}.` **{species}** - {tradeType} - *{username}*";
        });

        var description = string.Join("\n", queueList);

        // Discord embeds have a 4096 character limit for description
        if (description.Length > 4000)
        {
            description = description.Substring(0, 4000) + "\n... (lista truncada)";
        }

        embedBuilder.WithDescription(description);

        await ReplyAsync(embed: embedBuilder.Build()).ConfigureAwait(false);
    }

    [Command("queueToggle")]
    [Alias("qt", "tt")]
    [Summary("Activa o desactiva la capacidad de unirse a la cola de intercambio.")]
    [RequireSudo]
    public Task ToggleQueueTradeAsync()
    {
        var state = Info.ToggleQueue();
        var msg = state
            ? "Los usuarios ahora pueden unirse a la cola de intercambio."
            : "Configuración de cola cambiada: **Los usuarios NO pueden unirse a la cola hasta que se active de nuevo.**";

        return Context.Channel.EchoAndReply(msg);
    }

    private static string ClearTrade(ulong userID)
    {
        var result = Info.ClearTrade(userID);
        return GetClearTradeMessage(result);
    }

    private static string DeleteTradeCode(ulong userID)
    {
        var tradeCodeStorage = new TradeCodeStorage();
        bool success = tradeCodeStorage.DeleteTradeCode(userID);

        if (success)
            return "Tu código de intercambio almacenado ha sido eliminado con éxito.";
        else
            return "No se encontró un código de intercambio almacenado para tu ID de usuario.";
    }

    private static string GetClearTradeMessage(QueueResultRemove result)
    {
        return result switch
        {
            QueueResultRemove.Removed => "Eliminaste tus intercambios pendientes de la cola.",
            QueueResultRemove.CurrentlyProcessing => "¡Parece que tienes intercambios actualmente en proceso! No se eliminaron esos de la cola.",
            QueueResultRemove.CurrentlyProcessingRemoved => "¡Parece que tienes intercambios actualmente en proceso! Se eliminaron otros intercambios pendientes de la cola.",
            QueueResultRemove.NotInQueue => "Lo siento, no estás actualmente en la cola.",
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
        };
    }

    private async Task DeleteMessagesAfterDelayAsync(IMessage sentMessage, IMessage? messageToDelete, int delaySeconds)
    {
        try
        {
            // Don't attempt to delete messages in DM channels - Discord doesn't allow it
            if (sentMessage.Channel is IDMChannel)
                return;

            await Task.Delay(delaySeconds * 1000);
            await sentMessage.DeleteAsync();
            if (messageToDelete != null)
                await messageToDelete.DeleteAsync();
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(QueueModule<T>));
        }
    }

    private async Task ReplyAndDeleteAsync(string message, int delaySeconds, IMessage? messageToDelete = null)
    {
        try
        {
            var sentMessage = await ReplyAsync(message).ConfigureAwait(false);
            _ = DeleteMessagesAfterDelayAsync(sentMessage, messageToDelete, delaySeconds);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(QueueModule<T>));
        }
    }

    [Command("changeTradeCode")]
    [Alias("ctc")]
    [Summary("Cambia el código de intercambio del usuario si el almacenamiento de códigos está activo.")]
    public async Task ChangeTradeCodeAsync([Summary("Nuevo código de intercambio de 8 dígitos")] string newCode)
    {
        // Delete user's message immediately to protect the trade code
        await Context.Message.DeleteAsync().ConfigureAwait(false);

        var userID = Context.User.Id;
        var tradeCodeStorage = new TradeCodeStorage();

        if (!ValidateTradeCode(newCode, out string errorMessage))
        {
            await SendTemporaryMessageAsync(errorMessage).ConfigureAwait(false);
            return;
        }

        try
        {
            int code = int.Parse(newCode);
            if (tradeCodeStorage.UpdateTradeCode(userID, code))
            {
                await SendTemporaryMessageAsync("Tu código de intercambio ha sido actualizado con éxito.").ConfigureAwait(false);
            }
            else
            {
                await SendTemporaryMessageAsync("No tienes un código de intercambio establecido. Usa el comando trade para generar uno primero.").ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error al cambiar el código de intercambio para el usuario {userID}: {ex.Message}", nameof(QueueModule<T>));
            await SendTemporaryMessageAsync("Ocurrió un error al cambiar tu código de intercambio. Por favor, inténtalo de nuevo más tarde.").ConfigureAwait(false);
        }
    }

    private async Task SendTemporaryMessageAsync(string message)
    {
        var sentMessage = await ReplyAsync(message).ConfigureAwait(false);
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            await sentMessage.DeleteAsync().ConfigureAwait(false);
        });
    }

    private static bool ValidateTradeCode(string code, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (code.Length != 8)
        {
            errorMessage = "El código de intercambio debe tener exactamente 8 dígitos.";
            return false;
        }

        if (!Regex.IsMatch(code, @"^\d{8}$"))
        {
            errorMessage = "El código de intercambio debe contener solo dígitos.";
            return false;
        }

        if (QueueModule<T>.IsEasilyGuessableCode(code))
        {
            errorMessage = "El código de intercambio es demasiado fácil de adivinar. Por favor, elige un código más complejo.";
            return false;
        }

        return true;
    }

    private static bool IsEasilyGuessableCode(string code)
    {
        string[] easyPatterns = [
                @"^(\d)\1{7}$",           // All same digits (e.g., 11111111)
                @"^12345678$",            // Ascending sequence
                @"^87654321$",            // Descending sequence
                @"^(?:01234567|12345678|23456789)$" // Other common sequences
            ];

        foreach (var pattern in easyPatterns)
        {
            if (Regex.IsMatch(code, pattern))
            {
                return true;
            }
        }

        return false;
    }
}
