using Discord;
using Discord.Commands;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

public class RecoveryModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static IPokeBotRunner? Runner => SysCord<T>.Runner;

    [Command("recovery")]
    [Alias("recover")]
    [Summary("Muestra el estado de recuperación de todos los bots.")]
    [RequireSudo]
    public async Task ShowRecoveryStatusAsync()
    {
        if (Runner == null)
        {
            await ReplyAsync("El bot runner no está inicializado.").ConfigureAwait(false);
            return;
        }

        if (Runner is not PokeBotRunner<T> runner)
        {
            await ReplyAsync("El servicio de recuperación no está disponible para este tipo de bot runner.").ConfigureAwait(false);
            return;
        }
        
        var recoveryService = runner.GetRecoveryService();
        
        if (recoveryService == null)
        {
            await ReplyAsync("El servicio de recuperación no está habilitado.").ConfigureAwait(false);
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("Estado de Recuperación del Bot")
            .WithColor(Color.Blue)
            .WithTimestamp(DateTimeOffset.Now);

        var hasRecoveryData = false;
        foreach (var bot in Runner.Bots)
        {
            var state = bot.GetRecoveryState();
            if (state != null && (state.ConsecutiveFailures > 0 || state.CrashHistory.Count > 0))
            {
                hasRecoveryData = true;
                var status = bot.IsRunning ? "🟢 En ejecución" : "🔴 Detenido";
                if (state.IsRecovering)
                    status = "🟠 Recuperando";

                var fieldValue = $"Estado: {status}\n" +
                                $"Crasheos: {state.CrashHistory.Count}\n" +
                                $"Intentos Fallidos: {state.ConsecutiveFailures}";
                
                if (state.LastRecoveryAttempt.HasValue)
                {
                    fieldValue += $"\nÚltima Recuperación: {state.LastRecoveryAttempt.Value:HH:mm:ss}";
                }
                
                embed.AddField(bot.Bot.Connection.Name, fieldValue, true);
            }
        }

        if (!hasRecoveryData)
        {
            embed.WithDescription("Todos los bots están funcionando normalmente sin historial de recuperación.");
        }

        await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
    }

    [Command("recoveryReset")]
    [Alias("resetRecovery")]
    [Summary("Reinicia el estado de recuperación para un bot específico.")]
    [RequireSudo]
    public async Task ResetRecoveryAsync([Remainder] string botName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(botName);
        
        if (Runner == null)
        {
            await ReplyAsync("El bot runner no está inicializado.").ConfigureAwait(false);
            return;
        }

        if (Runner is not PokeBotRunner<T> runner)
        {
            await ReplyAsync("El servicio de recuperación no está disponible para este tipo de bot runner.").ConfigureAwait(false);
            return;
        }
        
        var recoveryService = runner.GetRecoveryService();
        
        if (recoveryService == null)
        {
            await ReplyAsync("El servicio de recuperación no está habilitado.").ConfigureAwait(false);
            return;
        }

        var bot = Runner.Bots.FirstOrDefault(b => b.Bot.Connection.Name.Equals(botName, StringComparison.OrdinalIgnoreCase));
        if (bot == null)
        {
            await ReplyAsync($"Bot '{botName}' no encontrado.").ConfigureAwait(false);
            return;
        }

        recoveryService.ResetRecoveryState(bot.Bot.Connection.Name);
        await ReplyAsync($"El estado de recuperación para el bot '{bot.Bot.Connection.Name}' ha sido reiniciado.").ConfigureAwait(false);
    }

    [Command("recoveryToggle")]
    [Alias("toggleRecovery")]
    [Summary("Habilita o deshabilita el sistema de recuperación.")]
    [RequireSudo]
    public async Task ToggleRecoveryAsync()
    {
        if (Runner == null)
        {
            await ReplyAsync("El bot runner no está inicializado.").ConfigureAwait(false);
            return;
        }

        if (Runner is not PokeBotRunner<T> runner)
        {
            await ReplyAsync("El servicio de recuperación no está disponible para este tipo de bot runner.").ConfigureAwait(false);
            return;
        }
        
        var config = Runner.Config.Recovery;
        config.EnableRecovery = !config.EnableRecovery;

        var status = config.EnableRecovery ? "habilitado" : "deshabilitado";
        await ReplyAsync($"El sistema de recuperación ha sido {status}.").ConfigureAwait(false);
        
        // Update the recovery service state
        if (config.EnableRecovery)
            runner.RecoveryService?.EnableRecovery();
        else
            runner.RecoveryService?.DisableRecovery();
    }

    [Command("recoveryConfig")]
    [Alias("recoveryCfg")]
    [Summary("Muestra la configuración actual de recuperación.")]
    [RequireSudo]
    public async Task ShowRecoveryConfigAsync()
    {
        if (Runner == null)
        {
            await ReplyAsync("El bot runner no está inicializado.").ConfigureAwait(false);
            return;
        }

        var config = Runner.Config.Recovery;
        
        var embed = new EmbedBuilder()
            .WithTitle("Configuración de Recuperación del Bot")
            .WithColor(Color.Blue)
            .WithTimestamp(DateTimeOffset.Now)
            .AddField("Habilitado", config.EnableRecovery ? "✅ Sí" : "❌ No", true)
            .AddField("Intentos máximos", config.MaxRecoveryAttempts, true)
            .AddField("Retraso inicial", $"{config.InitialRecoveryDelaySeconds}s", true)
            .AddField("Retraso máximo", $"{config.MaxRecoveryDelaySeconds}s", true)
            .AddField("Multiplicador de retraso", $"{config.BackoffMultiplier}x", true)
            .AddField("Ventana de crasheos", $"{config.CrashHistoryWindowMinutes} min", true)
            .AddField("Máx. crasheos por ventana", config.MaxCrashesInWindow, true)
            .AddField("Recuperar detenciones intencionales", config.RecoverIntentionalStops ? "✅" : "❌", true)
            .AddField("Uptime estable mínimo", $"{config.MinimumStableUptimeSeconds}s", true);

        await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
    }
}