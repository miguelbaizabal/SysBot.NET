using Discord;
using Discord.WebSocket;
using SysBot.Base;
using System;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

/// <summary>
/// Helper class for sending bot recovery notifications to Discord.
/// </summary>
public static class RecoveryNotificationHelper
{
    private static DiscordSocketClient? _client;
    private static ulong? _notificationChannelId;
    private static string _hubName = "Centro de control del Bot";

    /// <summary>
    /// Initializes the recovery notification system with Discord client and channel.
    /// </summary>
    public static void Initialize(DiscordSocketClient client, ulong? notificationChannelId, string hubName)
    {
        _client = client;
        _notificationChannelId = notificationChannelId;
        _hubName = hubName;
    }

    /// <summary>
    /// Hooks up recovery events to Discord notifications.
    /// </summary>
    public static void HookRecoveryEvents<T>(BotRecoveryService<T> recoveryService) where T : class, IConsoleBotConfig
    {
        if (recoveryService == null || _client == null)
            return;

        recoveryService.BotCrashed += async (sender, e) => await OnBotCrashed(e);
        recoveryService.RecoveryAttempted += async (sender, e) => await OnRecoveryAttempted(e);
        recoveryService.RecoverySucceeded += async (sender, e) => await OnRecoverySucceeded(e);
        recoveryService.RecoveryFailed += async (sender, e) => await OnRecoveryFailed(e);
    }

    private static async Task OnBotCrashed(BotCrashEventArgs e)
    {
        var embed = new EmbedBuilder()
            .WithTitle("⚠️ Crash del Bot Detectado")
            .WithDescription($"**Bot**: {e.BotName}\n**Fecha y Hora**: {e.CrashTime:yyyy-MM-dd HH:mm:ss} UTC")
            .WithColor(Color.Orange)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .AddField("Status", "Intentando recuperación automática...", false)
            .WithFooter($"{_hubName} Sistema de Recuperación")
            .Build();

        await SendNotificationAsync(embed);
    }

    private static async Task OnRecoveryAttempted(BotRecoveryEventArgs e)
    {
        if (!e.IsSuccess) // Only notify on attempts, not successes (handled separately)
        {
            var embed = new EmbedBuilder()
                .WithTitle("🔄 Intento de Recuperación")
                .WithDescription($"**Bot**: {e.BotName}\n**Intento**: {e.AttemptNumber}")
                .WithColor(Color.Blue)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter($"{_hubName} Sistema de Recuperación")
                .Build();

            await SendNotificationAsync(embed);
        }
    }

    private static async Task OnRecoverySucceeded(BotRecoveryEventArgs e)
    {
        var embed = new EmbedBuilder()
            .WithTitle("✅ Recuperación del Bot Exitosa")
            .WithDescription($"**Bot**: {e.BotName}\n**Intentos**: {e.AttemptNumber}")
            .WithColor(Color.Green)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .AddField("Status", "El bot ahora está funcionando normalmente", false)
            .WithFooter($"{_hubName} Sistema de Recuperación")
            .Build();

        await SendNotificationAsync(embed);
    }

    private static async Task OnRecoveryFailed(BotRecoveryEventArgs e)
    {
        var embed = new EmbedBuilder()
            .WithTitle("❌ Recuperación del Bot Fallida")
            .WithDescription($"**Bot**: {e.BotName}\n**Intentos**: {e.AttemptNumber}")
            .WithColor(Color.Red)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .AddField("Reason", e.FailureReason ?? "Error desconocido", false)
            .AddField("Action Required", "Se requiere intervención manual para reiniciar este bot", false)
            .WithFooter($"{_hubName} Sistema de Recuperación")
            .Build();

        await SendNotificationAsync(embed);
    }

    private static async Task SendNotificationAsync(Embed embed)
    {
        try
        {
            if (_client == null || !_notificationChannelId.HasValue)
                return;

            if (_client.GetChannel(_notificationChannelId.Value) is ISocketMessageChannel channel)
            {
                await channel.SendMessageAsync(embed: embed);
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error al enviar notificación de recuperación a Discord: {ex.Message}", "RecoveryNotification");
        }
    }

    /// <summary>
    /// Sends a custom recovery notification.
    /// </summary>
    public static async Task SendCustomNotificationAsync(string title, string description, Color color)
    {
        var embed = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithColor(color)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithFooter($"{_hubName} Sistema de Recuperación")
            .Build();

        await SendNotificationAsync(embed);
    }

    /// <summary>
    /// Sends a recovery summary report.
    /// </summary>
    public static async Task SendRecoverySummaryAsync<T>(BotRunner<T> runner, BotRecoveryService<T> recoveryService) 
        where T : class, IConsoleBotConfig
    {
        var embedBuilder = new EmbedBuilder()
            .WithTitle("📊 Resumen de Recuperación del Bot")
            .WithColor(Color.Blue)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithFooter($"{_hubName} Sistema de Recuperación");

        foreach (var bot in runner.Bots)
        {
            var state = bot.GetRecoveryState();
            if (state != null && (state.ConsecutiveFailures > 0 || state.CrashHistory.Count > 0))
            {
                var status = bot.IsRunning ? "🟢 En ejecución" : "🔴 Detenido";
                var fieldValue = $"Estado: {status}\n" +
                                $"Crasheos: {state.CrashHistory.Count}\n" +
                                $"Intentos Fallidos: {state.ConsecutiveFailures}";
                
                embedBuilder.AddField(bot.Bot.Connection.Name, fieldValue, true);
            }
        }

        if (embedBuilder.Fields.Count == 0)
        {
            embedBuilder.WithDescription("Todos los bots están funcionando normalmente sin crasheos recientes.");
        }

        await SendNotificationAsync(embedBuilder.Build());
    }
}