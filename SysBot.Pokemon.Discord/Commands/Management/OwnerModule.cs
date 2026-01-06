using AnimatedGif;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using PKHeX.Core;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using DiscordColor = Discord.Color;

namespace SysBot.Pokemon.Discord;

public class OwnerModule<T> : SudoModule<T> where T : PKM, new()
{
    [Command("listguilds")]
    [Alias("lg", "servers", "listservers")]
    [Summary("Lista todos los servidores en los que el bot está participando.")]
    [RequireSudo]
    public async Task ListGuilds(int page = 1)
    {
        const int guildsPerPage = 25; // Discord limit for fields in an embed
        int guildCount = Context.Client.Guilds.Count;
        int totalPages = (int)Math.Ceiling(guildCount / (double)guildsPerPage);
        page = Math.Max(1, Math.Min(page, totalPages));

        var guilds = Context.Client.Guilds
            .Skip((page - 1) * guildsPerPage)
            .Take(guildsPerPage);

        var embedBuilder = new EmbedBuilder()
            .WithTitle($"Lista de Servidores - Página {page}/{totalPages}")
            .WithDescription("Aquí están los servidores en los que estoy actualmente:")
            .WithColor((DiscordColor)Color.Blue);

        foreach (var guild in guilds)
        {
            embedBuilder.AddField(guild.Name, $"ID: {guild.Id}", inline: true);
        }
        var dmChannel = await Context.User.CreateDMChannelAsync();
        await dmChannel.SendMessageAsync(embed: embedBuilder.Build());

        await ReplyAsync($"{Context.User.Mention}, Te he enviado un DM con la lista de servidores (Página {page}).");

        if (Context.Message is IUserMessage userMessage)
        {
            await Task.Delay(2000);
            await userMessage.DeleteAsync().ConfigureAwait(false);
        }
    }

    [Command("blacklistserver")]
    [Alias("bls")]
    [Summary("Agrega un ID de servidor a la lista negra del bot.")]
    [RequireOwner]
    public async Task BlacklistServer(ulong serverId)
    {
        var settings = SysCord<T>.Runner.Hub.Config.Discord;

        if (settings.ServerBlacklist.Contains(serverId))
        {
            await ReplyAsync("Este servidor ya está en la lista negra.");
            return;
        }

        var server = Context.Client.GetGuild(serverId);
        if (server == null)
        {
            await ReplyAsync("No se puede encontrar un servidor con el ID proporcionado. Asegúrate de que el bot sea miembro del servidor que deseas incluir en la lista negra.");
            return;
        }

        var newServerAccess = new RemoteControlAccess { ID = serverId, Name = server.Name, Comment = "Blacklisted server" };

        settings.ServerBlacklist.AddIfNew([newServerAccess]);

        await server.LeaveAsync();
        await ReplyAsync($"Dejé el servidor '{server.Name}' y lo agregué a la lista negra.");
    }

    [Command("unblacklistserver")]
    [Alias("ubls")]
    [Summary("Elimina un ID de servidor de la lista negra del bot.")]
    [RequireOwner]
    public async Task UnblacklistServer(ulong serverId)
    {
        var settings = SysCord<T>.Runner.Hub.Config.Discord;

        if (!settings.ServerBlacklist.Contains(serverId))
        {
            await ReplyAsync("Este servidor no está actualmente en la lista negra.");
            return;
        }

        var wasRemoved = settings.ServerBlacklist.RemoveAll(x => x.ID == serverId) > 0;

        if (wasRemoved)
        {
            await ReplyAsync($"El servidor con ID {serverId} ha sido eliminado de la lista negra.");
        }
        else
        {
            await ReplyAsync("Ocurrió un error al intentar eliminar el servidor de la lista negra. Por favor, verifica el ID del servidor y vuelve a intentarlo.");
        }
    }

    [Command("addSudo")]
    [Summary("Agrega al usuario mencionado a la lista de usuarios sudo globales")]
    [RequireOwner]
    public async Task SudoUsers([Remainder] string _)
    {
        var users = Context.Message.MentionedUsers;
        var objects = users.Select(GetReference);
        SysCordSettings.Settings.GlobalSudoList.AddIfNew(objects);
        await ReplyAsync("Hecho.").ConfigureAwait(false);
    }

    [Command("removeSudo")]
    [Summary("Elimina al usuario mencionado de la lista de usuarios sudo globales")]
    [RequireOwner]
    public async Task RemoveSudoUsers([Remainder] string _)
    {
        var users = Context.Message.MentionedUsers;
        var objects = users.Select(GetReference);
        SysCordSettings.Settings.GlobalSudoList.RemoveAll(z => objects.Any(o => o.ID == z.ID));
        await ReplyAsync("Hecho.").ConfigureAwait(false);
    }

    [Command("addChannel")]
    [Summary("Agrega un canal a la lista de canales que aceptan comandos.")]
    [RequireOwner]
    public async Task AddChannel()
    {
        var obj = GetReference(Context.Message.Channel);
        SysCordSettings.Settings.ChannelWhitelist.AddIfNew([obj]);
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("syncChannels")]
    [Alias("sch", "syncchannels")]
    [Summary("Copia todos los canales de ChannelWhitelist a AnnouncementChannel.")]
    [RequireOwner]
    public async Task SyncChannels()
    {
        var whitelist = SysCordSettings.Settings.ChannelWhitelist.List;
        var announcementList = SysCordSettings.Settings.AnnouncementChannels.List;

        bool changesMade = false;

        foreach (var channel in whitelist)
        {
            if (!announcementList.Any(x => x.ID == channel.ID))
            {
                announcementList.Add(channel);
                changesMade = true;
            }
        }

        if (changesMade)
        {
            await ReplyAsync("La lista blanca de canales ha sido sincronizada con los canales de anuncio.").ConfigureAwait(false);
        }
        else
        {
            await ReplyAsync("Todos los canales de la lista blanca ya están en los canales de anuncio, no se realizaron cambios.").ConfigureAwait(false);
        }
    }

    [Command("removeChannel")]
    [Summary("Elimina un canal de la lista de canales que aceptan comandos.")]
    [RequireOwner]
    public async Task RemoveChannel()
    {
        var obj = GetReference(Context.Message.Channel);
        SysCordSettings.Settings.ChannelWhitelist.RemoveAll(z => z.ID == obj.ID);
        await ReplyAsync("Hecho.").ConfigureAwait(false);
    }

    [Command("leave")]
    [Alias("bye")]
    [Summary("Abandona el servidor actual.")]
    [RequireOwner]
    public async Task Leave()
    {
        await ReplyAsync("Adiós.").ConfigureAwait(false);
        await Context.Guild.LeaveAsync().ConfigureAwait(false);
    }

    [Command("leaveguild")]
    [Alias("lg")]
    [Summary("Abandona un servidor basado en el ID proporcionado.")]
    [RequireOwner]
    public async Task LeaveGuild(string userInput)
    {
        if (!ulong.TryParse(userInput, out ulong id))
        {
            await ReplyAsync("Por favor, proporciona un ID de servidor válido.").ConfigureAwait(false);
            return;
        }

        var guild = Context.Client.Guilds.FirstOrDefault(x => x.Id == id);
        if (guild is null)
        {
            await ReplyAsync($"La entrada proporcionada ({userInput}) no es un ID de servidor válido o el bot no está en el servidor especificado.").ConfigureAwait(false);
            return;
        }

        await ReplyAsync($"Abandonando {guild}.").ConfigureAwait(false);
        await guild.LeaveAsync().ConfigureAwait(false);
    }

    [Command("leaveall")]
    [Summary("Abandona todos los servidores en los que el bot está actualmente.")]
    [RequireOwner]
    public async Task LeaveAll()
    {
        await ReplyAsync("Abandonando todos los servidores.").ConfigureAwait(false);
        foreach (var guild in Context.Client.Guilds)
        {
            await guild.LeaveAsync().ConfigureAwait(false);
        }
    }

    [Command("repeek")]
    [Alias("peek")]
    [Summary("Toma y envía una captura de pantalla desde la Switch configurada actualmente.")]
    [RequireSudo]
    public async Task RePeek()
    {
        string ip = OwnerModule<T>.GetBotIPFromJsonConfig();
        var source = new CancellationTokenSource();
        var token = source.Token;

        var bot = SysCord<T>.Runner.GetBot(ip);
        if (bot == null)
        {
            await ReplyAsync($"No se encontró ningún bot con la dirección IP especificada ({ip}).").ConfigureAwait(false);
            return;
        }

        _ = Array.Empty<byte>();
        byte[]? bytes;
        try
        {
            bytes = await bot.Bot.Connection.PixelPeek(token).ConfigureAwait(false) ?? [];
        }
        catch (Exception ex)
        {
            await ReplyAsync($"Error al obtener los píxeles: {ex.Message}");
            return;
        }

        if (bytes.Length == 0)
        {
            await ReplyAsync("No se recibieron datos de captura de pantalla.");
            return;
        }

        await using MemoryStream ms = new(bytes);
        const string img = "cap.jpg";
        var embed = new EmbedBuilder { ImageUrl = $"attachment://{img}", Color = (DiscordColor?)Color.Purple }
            .WithFooter(new EmbedFooterBuilder { Text = "Aquí está tu captura de pantalla." });

        await Context.Channel.SendFileAsync(ms, img, embed: embed.Build());
    }

    [Command("video")]
    [Alias("video")]
    [Summary("Toma y envía un GIF desde la Switch configurada actualmente.")]
    [RequireSudo]
    public async Task RePeekGIF()
    {
        await Context.Channel.SendMessageAsync("Procesando solicitud de GIF...").ConfigureAwait(false);
        try
        {
            string ip = OwnerModule<T>.GetBotIPFromJsonConfig();
            var source = new CancellationTokenSource();
            var token = source.Token;
            var bot = SysCord<T>.Runner.GetBot(ip);

            if (bot == null)
            {
                await ReplyAsync($"No se encontró ningún bot con la dirección IP especificada ({ip}).").ConfigureAwait(false);
                return;
            }

            const int screenshotCount = 10;
            var screenshotInterval = TimeSpan.FromSeconds(0.1 / 10);
            var gifFrames = new List<byte[]>();

            for (int i = 0; i < screenshotCount; i++)
            {
                byte[] bytes;
                try
                {
                    bytes = await bot.Bot.Connection.PixelPeek(token).ConfigureAwait(false) ?? Array.Empty<byte>();
                }
                catch (Exception ex)
                {
                    await ReplyAsync($"Error al obtener los píxeles: {ex.Message}").ConfigureAwait(false);
                    return;
                }

                if (bytes.Length == 0)
                {
                    await ReplyAsync("No se recibieron datos de captura de pantalla.").ConfigureAwait(false);
                    return;
                }

                gifFrames.Add(bytes);

                if (i < screenshotCount - 1)
                {
                    await Task.Delay(screenshotInterval).ConfigureAwait(false);
                }
            }

            await using (var ms = new MemoryStream())
            {
                await CreateGifAsync(ms, gifFrames).ConfigureAwait(false);

                ms.Position = 0;
                const string gifFileName = "screenshot.gif";
                var embed = new EmbedBuilder { ImageUrl = $"attachment://{gifFileName}", Color = (DiscordColor?)Color.Red }
                    .WithFooter(new EmbedFooterBuilder { Text = "Aquí está tu GIF." });

                await Context.Channel.SendFileAsync(ms, gifFileName, embed: embed.Build()).ConfigureAwait(false);
            }

            gifFrames.Clear();
        }
        catch (Exception ex)
        {
            await ReplyAsync($"Error al procesar el GIF: {ex.Message}").ConfigureAwait(false);
        }
    }

    private async Task CreateGifAsync(Stream outputStream, List<byte[]> frames)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        using var gif = new AnimatedGifCreator(outputStream, 200);
        foreach (var frameBytes in frames)
        {
            using (var ms = new MemoryStream(frameBytes))
            using (var bitmap = new Bitmap(ms))
            using (var frame = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                gif.AddFrame(frame);
            }
            await Task.Yield(); // Allow other tasks to run
        }
#pragma warning restore CA1416 // Validate platform compatibility
    }

    private static string GetBotIPFromJsonConfig()
    {
        try
        {
            var jsonData = File.ReadAllText(PokeBot.ConfigPath);
            var config = JObject.Parse(jsonData);

            var botsArray = config["Bots"] as JArray;
            if (botsArray == null || botsArray.Count == 0)
                return "192.168.1.1";
            
            var firstBot = botsArray[0] as JObject;
            var connection = firstBot?["Connection"] as JObject;
            var ip = connection?["IP"]?.ToString();
            
            return ip ?? "192.168.1.1";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al leer el archivo de configuración: {ex.Message}");
            return "192.168.1.1";
        }
    }

    [Command("kill")]
    [Alias("shutdown")]
    [Summary("¡Ocasiona la finalización del proceso completo!")]
    [RequireOwner]
    public async Task ExitProgram()
    {
        await Context.Channel.EchoAndReply("Apagando... ¡adiós! **Los servicios del bot están apagándose.**").ConfigureAwait(false);
        Environment.Exit(0);
    }

    [Command("dm")]
    [Summary("Envía un mensaje directo a un usuario especificado.")]
    [RequireOwner]
    public async Task DMUserAsync(SocketUser user, [Remainder] string message)
    {
        var attachments = Context.Message.Attachments;
        var hasAttachments = attachments.Count != 0;

        var embed = new EmbedBuilder
        {
            Title = "Mensaje privado del propietario del bot",
            Description = message,
            Color = (DiscordColor?)Color.Gold,
            Timestamp = DateTimeOffset.Now,
            ThumbnailUrl = "https://raw.githubusercontent.com/hexbyt3/sprites/main/pikamail.png"
        };

        try
        {
            var dmChannel = await user.CreateDMChannelAsync();

            if (hasAttachments)
            {
                foreach (var attachment in attachments)
                {
                    using var httpClient = new HttpClient();
                    var stream = await httpClient.GetStreamAsync(attachment.Url);
                    var file = new FileAttachment(stream, attachment.Filename);
                    await dmChannel.SendFileAsync(file, embed: embed.Build());
                }
            }
            else
            {
                await dmChannel.SendMessageAsync(embed: embed.Build());
            }

            var confirmationMessage = await ReplyAsync($"Mensaje enviado correctamente a {user.Username}.");
            await Context.Message.DeleteAsync();
            await Task.Delay(TimeSpan.FromSeconds(10));
            await confirmationMessage.DeleteAsync();
        }
        catch (Exception ex)
        {
            await ReplyAsync($"Error al enviar el mensaje a {user.Username}. Error: {ex.Message}");
        }
    }

    [Command("say")]
    [Summary("Envía un mensaje a un canal especificado.")]
    [RequireSudo]
    public async Task SayAsync([Remainder] string message)
    {
        var attachments = Context.Message.Attachments;
        var hasAttachments = attachments.Count != 0;

        var indexOfChannelMentionStart = message.LastIndexOf('<');
        var indexOfChannelMentionEnd = message.LastIndexOf('>');
        if (indexOfChannelMentionStart == -1 || indexOfChannelMentionEnd == -1)
        {
            await ReplyAsync("Por favor menciona un canal correctamente usando #canal.");
            return;
        }

        var channelMention = message.Substring(indexOfChannelMentionStart, indexOfChannelMentionEnd - indexOfChannelMentionStart + 1);
        var actualMessage = message.Substring(0, indexOfChannelMentionStart).TrimEnd();

        var channel = Context.Guild.Channels.FirstOrDefault(c => $"<#{c.Id}>" == channelMention);

        if (channel == null)
        {
            await ReplyAsync("Canal no encontrado.");
            return;
        }

        if (channel is not IMessageChannel messageChannel)
        {
            await ReplyAsync("El canal mencionado no es un canal de texto.");
            return;
        }

        // If there are attachments, send them to the channel
        if (hasAttachments)
        {
            foreach (var attachment in attachments)
            {
                using var httpClient = new HttpClient();
                var stream = await httpClient.GetStreamAsync(attachment.Url);
                var file = new FileAttachment(stream, attachment.Filename);
                await messageChannel.SendFileAsync(file, actualMessage);
            }
        }
        else
        {
            await messageChannel.SendMessageAsync(actualMessage);
        }

        // Send confirmation message to the user
        await ReplyAsync($"Mensaje enviado correctamente en {channelMention}.");
    }

    private RemoteControlAccess GetReference(IUser channel) => new()
    {
        ID = channel.Id,
        Name = channel.Username,
        Comment = $"Añadido por {Context.User.Username} el {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };

    private RemoteControlAccess GetReference(IChannel channel) => new()
    {
        ID = channel.Id,
        Name = channel.Name,
        Comment = $"Añadido por {Context.User.Username} el {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };
}
