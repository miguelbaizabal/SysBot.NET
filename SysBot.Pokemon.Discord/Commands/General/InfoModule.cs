using Discord;
using Discord.Commands;
using SysBot.Pokemon.Helpers;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

// src: https://github.com/foxbot/patek/blob/master/src/Patek/Modules/InfoModule.cs
// ISC License (ISC)
// Copyright 2017, Christopher F. <foxbot@protonmail.com>
public class InfoModule : ModuleBase<SocketCommandContext>
{
    private const string detail = "Soy un bot de intercambio de Pokémon en Discord de código abierto desarrollado por hexbyt3 y traducido por miguelbaizabal.";

    private const string repo = "https://github.com/miguelbaizabal/SysBot.NET/tree/pokebot-es";

    [Command("info")]
    [Alias("about", "whoami", "owner")]
    public async Task InfoAsync()
    {
        var app = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);

        var builder = new EmbedBuilder
        {
            Color = new Color(114, 137, 218),
            Description = detail,
        };

        builder.AddField("Información",
            $"- [Código Fuente]({repo})\n" +
            $"- {Format.Bold("Propietario")}: {app.Owner} ({app.Owner.Id})\n" +
            $"- {Format.Bold("Librería")}: Discord.Net ({DiscordConfig.Version})\n" +
            $"- {Format.Bold("Tiempo en línea")}: {GetUptime()}\n" +
            $"- {Format.Bold("Runtime")}: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture} " +
            $"({RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture})\n" +
            $"- {Format.Bold("Fecha de compilación")}: {GetVersionInfo("SysBot.Base", false)}\n" +
            $"- {Format.Bold("Versión de SysBot+")}: {PokeBot.Version}\n" +
            $"- {Format.Bold("Versión del Core")}: {GetVersionInfo("PKHeX.Core")}\n" +
            $"- {Format.Bold("Versión de AutoLegality")}: {GetVersionInfo("PKHeX.Core.AutoMod")}\n"
        );

        builder.AddField("Estadísticas",
            $"- {Format.Bold("Tamaño del Heap")}: {GetHeapSize()}MiB\n" +
            $"- {Format.Bold("Servidores")}: {Context.Client.Guilds.Count}\n" +
            $"- {Format.Bold("Canales")}: {Context.Client.Guilds.Sum(g => g.Channels.Count)}\n" +
            $"- {Format.Bold("Usuarios")}: {Context.Client.Guilds.Sum(g => g.MemberCount)}\n"
        );

        await ReplyAsync("¡Aquí tienes un poco sobre mí!", embed: builder.Build()).ConfigureAwait(false);
    }

    private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.CurrentCulture);

    private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");

    private static string GetVersionInfo(string assemblyName, bool inclVersion = true)
    {
        const string _default = "Unknown";
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assembly = Array.Find(assemblies, x => x.GetName().Name == assemblyName);

        var attribute = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute is null)
            return _default;

        var info = attribute.InformationalVersion;
        var split = info.Split('+');
        if (split.Length >= 2)
        {
            var version = split[0];
            var revision = split[1];
            if (DateTime.TryParseExact(revision, "yyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var buildTime))
                return (inclVersion ? $"{version} " : "") + $@"{buildTime:yy-MM-dd\.hh\:mm}";
            return inclVersion ? version : _default;
        }
        return _default;
    }
}
