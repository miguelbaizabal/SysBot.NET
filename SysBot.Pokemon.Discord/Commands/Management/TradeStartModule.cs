using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using DiscordColor = Discord.Color;

namespace SysBot.Pokemon.Discord;

public class TradeStartModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private class TradeStartAction(ulong ChannelId, Action<PokeRoutineExecutorBase, PokeTradeDetail<T>> messager, string channel)
        : ChannelAction<PokeRoutineExecutorBase, PokeTradeDetail<T>>(ChannelId, messager, channel);

    private static DiscordSocketClient? _discordClient;

    private static readonly Dictionary<ulong, TradeStartAction> Channels = [];

    private static void Remove(TradeStartAction entry)
    {
        Channels.Remove(entry.ChannelID);
        SysCord<T>.Runner.Hub.Queues.Forwarders.Remove(entry.Action);
    }

#pragma warning disable RCS1158 // Static member in generic type should use a type parameter.

    public static void RestoreTradeStarting(DiscordSocketClient discord)
    {
        _discordClient = discord; // Store the DiscordSocketClient instance

        var cfg = SysCordSettings.Settings;
        foreach (var ch in cfg.TradeStartingChannels)
        {
            if (discord.GetChannel(ch.ID) is ISocketMessageChannel c)
                AddLogChannel(c, ch.ID);
        }

        LogUtil.LogInfo("Discord", "Se agregó la notificación de inicio de intercambio a los canales de Discord al iniciar el bot.");
    }

    public static bool IsStartChannel(ulong cid)
#pragma warning restore RCS1158 // Static member in generic type should use a type parameter.
    {
        return Channels.TryGetValue(cid, out _);
    }

    [Command("startHere")]
    [Summary("Hace que el bot registre los inicios de intercambio en el canal.")]
    [RequireSudo]
    public async Task AddLogAsync()
    {
        var c = Context.Channel;
        var cid = c.Id;
        if (Channels.TryGetValue(cid, out _))
        {
            await ReplyAsync("Ya está registrando aquí.").ConfigureAwait(false);
            return;
        }

        AddLogChannel(c, cid);

        // Add to discord global loggers (saves on program close)
        SysCordSettings.Settings.TradeStartingChannels.AddIfNew([GetReference(Context.Channel)]);
        await ReplyAsync("Se agregó la notificación de inicio de intercambio a este canal!").ConfigureAwait(false);
    }

    private static void AddLogChannel(ISocketMessageChannel c, ulong cid)
    {
        async void Logger(PokeRoutineExecutorBase bot, PokeTradeDetail<T> detail)
        {
            if (detail.Type == PokeTradeType.Random) return;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var user = _discordClient.GetUser(detail.Trainer.ID);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if (user == null) { Console.WriteLine($"Ningún usuario encontrado para el ID {detail.Trainer.ID}."); return; }

            string speciesName = detail.TradeData != null ? GameInfo.Strings.Species[detail.TradeData.Species] : "";
            string ballImgUrl = "https://raw.githubusercontent.com/hexbyt3/sprites/36e891cc02fe283cd70d9fc8fef2f3c490096d6c/imgs/difficulty.png";

            if (detail.TradeData != null && detail.Type != PokeTradeType.Clone && detail.Type != PokeTradeType.Dump && detail.Type != PokeTradeType.Seed && detail.Type != PokeTradeType.FixOT)
            {
                var ballName = GameInfo.GetStrings("en").balllist[detail.TradeData.Ball]
                    .Replace(" ", "").Replace("(LA)", "").ToLower();
                ballName = ballName == "pokéball" ? "pokeball" : (ballName.Contains("(la)") ? "la" + ballName : ballName);
                ballImgUrl = $"https://raw.githubusercontent.com/hexbyt3/sprites/main/AltBallImg/28x28/{ballName}.png";
            }

            string tradeTitle = detail.IsMysteryEgg ? "✨ Huevo Misterioso" : detail.Type switch
            {
                PokeTradeType.Clone => "Pokémon Clonado",
                PokeTradeType.Dump => "Pokémon Dumpeado",
                PokeTradeType.FixOT => "Pokémon Clonado (Corrigiendo Información de OT)",
                PokeTradeType.Seed => "Pokémon Clonado (Solicitud Especial)",
                _ => speciesName
            };

            string embedImageUrl = detail.IsMysteryEgg ? "https://raw.githubusercontent.com/hexbyt3/sprites/main/mysteryegg3.png" : detail.Type switch
            {
                PokeTradeType.Clone => "https://raw.githubusercontent.com/hexbyt3/sprites/main/clonepod.png",
                PokeTradeType.Dump => "https://raw.githubusercontent.com/hexbyt3/sprites/main/AltBallImg/128x128/dumpball.png",
                PokeTradeType.FixOT => "https://raw.githubusercontent.com/hexbyt3/sprites/main/AltBallImg/128x128/rocketball.png",
                PokeTradeType.Seed => "https://raw.githubusercontent.com/hexbyt3/sprites/main/specialrequest.png",
                _ => detail.TradeData != null ? TradeExtensions<T>.PokeImg(detail.TradeData, false, true) : ""
            };

            var (r, g, b) = await GetDominantColorAsync(embedImageUrl);

            string footerText = detail.Type == PokeTradeType.Clone || detail.Type == PokeTradeType.Dump || detail.Type == PokeTradeType.Seed || detail.Type == PokeTradeType.FixOT
                ? "Iniciando intercambio ahora."
                : $"Iniciando intercambio ahora. ¡Disfruta tu {(detail.IsMysteryEgg ? "✨ Huevo Misterioso" : speciesName)}!";

            var embed = new EmbedBuilder()
                .WithColor(new DiscordColor(r, g, b))
                .WithThumbnailUrl(embedImageUrl)
                .WithAuthor($"Siguiente: {user.Username}", user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithDescription($"**Recibiendo**: {tradeTitle}\n**ID de intercamnio**: {detail.ID}")
                .WithFooter($"{footerText}\u200B", ballImgUrl)
                .WithTimestamp(DateTime.Now)
                .Build();

            await c.SendMessageAsync(embed: embed);
        }

        SysCord<T>.Runner.Hub.Queues.Forwarders.Add(Logger);
        Channels.Add(cid, new TradeStartAction(cid, Logger, c.Name));
    }

    [Command("startInfo")]
    [Summary("Dumpea los ajustes de notificación de inicio.")]
    [RequireSudo]
    public async Task DumpLogInfoAsync()
    {
        foreach (var c in Channels)
            await ReplyAsync($"{c.Key} - {c.Value}").ConfigureAwait(false);
    }

    [Command("startClear")]
    [Summary("Limpia los ajustes de notificación de inicio en ese canal específico.")]
    [RequireSudo]
    public async Task ClearLogsAsync()
    {
        var cfg = SysCordSettings.Settings;
        if (Channels.TryGetValue(Context.Channel.Id, out var entry))
            Remove(entry);
        cfg.TradeStartingChannels.RemoveAll(z => z.ID == Context.Channel.Id);
        await ReplyAsync($"Notificaciones de inicio limpiadas del canal: {Context.Channel.Name}").ConfigureAwait(false);
    }

    [Command("startClearAll")]
    [Summary("Limpia todos los ajustes de notificación de inicio.")]
    [RequireSudo]
    public async Task ClearLogsAllAsync()
    {
        foreach (var l in Channels)
        {
            var entry = l.Value;
            await ReplyAsync($"¡Registro limpiado de {entry.ChannelName} ({entry.ChannelID}!").ConfigureAwait(false);
            SysCord<T>.Runner.Hub.Queues.Forwarders.Remove(entry.Action);
        }
        Channels.Clear();
        SysCordSettings.Settings.TradeStartingChannels.Clear();
        await ReplyAsync("¡Notificaciones de inicio limpiadas de todos los canales!").ConfigureAwait(false);
    }

    private RemoteControlAccess GetReference(IChannel channel) => new()
    {
        ID = channel.Id,
        Name = channel.Name,
        Comment = $"Agregado por {Context.User.Username} el {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };

    public static async Task<(int R, int G, int B)> GetDominantColorAsync(string imagePath)
    {
        try
        {
            Bitmap image = await LoadImageAsync(imagePath);

            var colorCount = new Dictionary<Color, int>();
#pragma warning disable CA1416 // Validate platform compatibility
            for (int y = 0; y < image.Height; y++)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                for (int x = 0; x < image.Width; x++)
                {
#pragma warning disable CA1416 // Validate platform compatibility
                    var pixelColor = image.GetPixel(x, y);
#pragma warning restore CA1416 // Validate platform compatibility

                    if (pixelColor.A < 128 || pixelColor.GetBrightness() > 0.9) continue;

                    var brightnessFactor = (int)(pixelColor.GetBrightness() * 100);
                    var saturationFactor = (int)(pixelColor.GetSaturation() * 100);
                    var combinedFactor = brightnessFactor + saturationFactor;

                    var quantizedColor = Color.FromArgb(
                        pixelColor.R / 10 * 10,
                        pixelColor.G / 10 * 10,
                        pixelColor.B / 10 * 10
                    );

                    if (colorCount.ContainsKey(quantizedColor))
                    {
                        colorCount[quantizedColor] += combinedFactor;
                    }
                    else
                    {
                        colorCount[quantizedColor] = combinedFactor;
                    }
                }
#pragma warning restore CA1416 // Validate platform compatibility
            }
#pragma warning restore CA1416 // Validate platform compatibility

#pragma warning disable CA1416 // Validate platform compatibility
            image.Dispose();
#pragma warning restore CA1416 // Validate platform compatibility

            if (colorCount.Count == 0)
                return (255, 255, 255);

            var dominantColor = colorCount.Aggregate((a, b) => a.Value > b.Value ? a : b).Key;
            return (dominantColor.R, dominantColor.G, dominantColor.B);
        }
        catch (Exception ex)
        {
            // Log or handle exceptions as needed
            Console.WriteLine($"Error al procesar imagen desde {imagePath}. Error: {ex.Message}");
            return (255, 255, 255);  // Default to white if an exception occurs
        }
    }

    private static async Task<Bitmap> LoadImageAsync(string imagePath)
    {
        if (imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(imagePath);
            await using var stream = await response.Content.ReadAsStreamAsync();
#pragma warning disable CA1416 // Validate platform compatibility
            return new Bitmap(stream);
#pragma warning restore CA1416 // Validate platform compatibility
        }
        else
        {
#pragma warning disable CA1416 // Validate platform compatibility
            return new Bitmap(imagePath);
#pragma warning restore CA1416 // Validate platform compatibility
        }
    }
}
