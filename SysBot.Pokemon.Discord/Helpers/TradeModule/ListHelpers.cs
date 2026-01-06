using Discord;
using Discord.Commands;
using Discord.Net;
using PKHeX.Core;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

public static class ListHelpers<T> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    public static async Task HandleListCommandAsync(SocketCommandContext context, string folderPath, string itemType,
        string commandPrefix, string args)
    {
        const int itemsPerPage = 20;
        var botPrefix = SysCord<T>.Runner.Config.Discord.CommandPrefix;

        if (string.IsNullOrEmpty(folderPath))
        {
            await Helpers<T>.ReplyAndDeleteAsync(context, "Este bot no tiene esta función configurada.", 2);
            return;
        }

        var (filter, page) = Helpers<T>.ParseListArguments(args);

        var allFiles = Directory.GetFiles(folderPath)
            .Select(Path.GetFileNameWithoutExtension)
            .Where(file => file != null)
            .OrderBy(file => file)
            .ToList()!;

        var filteredFiles = allFiles
            .Where(file => file != null && (string.IsNullOrWhiteSpace(filter) ||
                   file.Contains(filter, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (filteredFiles.Count == 0)
        {
            var replyMessage = await context.Channel.SendMessageAsync($"No se encontraron {itemType} que coincidan con el filtro '{filter}'.");
            _ = Helpers<T>.DeleteMessagesAfterDelayAsync(replyMessage, context.Message, 10);
            return;
        }

        var pageCount = (int)Math.Ceiling(filteredFiles.Count / (double)itemsPerPage);
        page = Math.Clamp(page, 1, pageCount);

        var pageItems = filteredFiles.Skip((page - 1) * itemsPerPage).Take(itemsPerPage);

        var embed = new EmbedBuilder()
            .WithTitle($"Disponible {char.ToUpper(itemType[0]) + itemType[1..]} - Filtro: '{filter}'")
            .WithDescription($"Página {page} de {pageCount}")
            .WithColor(Color.Blue);

        foreach (var item in pageItems)
        {
            var index = allFiles.IndexOf(item) + 1;
            embed.AddField($"{index}. {item}", $"Usa `{botPrefix}{commandPrefix} {index}` para solicitar este {itemType.TrimEnd('s')}.");
        }

        await SendDMOrReplyAsync(context, embed.Build());
    }

    public static async Task SendDMOrReplyAsync(SocketCommandContext context, Embed embed)
    {
        IUserMessage replyMessage;

        if (context.User is IUser user)
        {
            try
            {
                var dmChannel = await user.CreateDMChannelAsync();
                await dmChannel.SendMessageAsync(embed: embed);
                replyMessage = await context.Channel.SendMessageAsync($"{context.User.Mention}, Te he enviado un DM con la lista.");
            }
            catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.Forbidden)
            {
                replyMessage = await context.Channel.SendMessageAsync($"{context.User.Mention}, No puedo enviarte un DM. Por favor, revisa tus **Configuraciones de Privacidad del Servidor**.");
            }
        }
        else
        {
            replyMessage = await context.Channel.SendMessageAsync("**Error**: No pude enviar un DM. Por favor, revisa tus **Configuraciones de Privacidad del Servidor**.");
        }

        _ = Helpers<T>.DeleteMessagesAfterDelayAsync(replyMessage, context.Message, 10);
    }

    public static async Task HandleRequestCommandAsync(SocketCommandContext context, string folderPath, int index,
        string itemType, string listCommand)
    {
        var userID = context.User.Id;
        if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
        {
            await Helpers<T>.ReplyAndDeleteAsync(context,
                "Ya tienes un intercambio en cola que no se puede borrar. Por favor, espera hasta que sea procesado.", 2);
            return;
        }

        try
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                await Helpers<T>.ReplyAndDeleteAsync(context, "Este bot no tiene esta función configurada.", 2);
                return;
            }

            var files = Directory.GetFiles(folderPath)
                .Select(Path.GetFileName)
                .Where(x => x != null)
                .OrderBy(x => x)
                .ToList()!;

            if (index < 1 || index > files.Count)
            {
                await Helpers<T>.ReplyAndDeleteAsync(context,
                    $"Índice de {itemType} inválido. Por favor, usa un número válido en el comando `.{listCommand}`.", 2);
                return;
            }

            var selectedFile = files[index - 1];
            var fileData = await File.ReadAllBytesAsync(Path.Combine(folderPath, selectedFile!));
            var download = new Download<PKM>
            {
                Data = EntityFormat.GetFromBytes(fileData),
                Success = true
            };

            var pk = Helpers<T>.GetRequest(download);
            if (pk == null)
            {
                await Helpers<T>.ReplyAndDeleteAsync(context,
                    $"Error al convertir el archivo {itemType} al tipo PKM requerido.", 2);
                return;
            }

            var code = Info.GetRandomTradeCode(userID);
            var lgcode = Info.GetRandomLGTradeCode();
            var sig = context.User.GetFavor();

            await context.Channel.SendMessageAsync($"Solicitud de {char.ToUpper(itemType[0]) + itemType[1..]} añadida a la cola.").ConfigureAwait(false);
            await Helpers<T>.AddTradeToQueueAsync(context, code, context.User.Username, pk, sig,
                context.User, lgcode: lgcode).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Helpers<T>.ReplyAndDeleteAsync(context, $"Ocurrió un error: {ex.Message}", 2);
        }
        finally
        {
            if (context.Message is IUserMessage userMessage)
                _ = Helpers<T>.DeleteMessagesAfterDelayAsync(userMessage, null, 2);
        }
    }
}
