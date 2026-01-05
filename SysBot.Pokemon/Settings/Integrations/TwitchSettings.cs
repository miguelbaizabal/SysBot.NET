using System;
using System.ComponentModel;
using System.Linq;

namespace SysBot.Pokemon;

public class TwitchSettings
{
    private const string Startup = nameof(Startup);
    private const string Operation = nameof(Operation);
    private const string Messages = nameof(Messages);
    public override string ToString() => "Configuración de Integración de Twitch";

    // Startup

    [Category(Startup), Description("Token de autenticación del bot")]
    public string Token { get; set; } = string.Empty;

    [Category(Startup), Description("Nombre de usuario del bot")]
    public string Username { get; set; } = string.Empty;

    [Category(Startup), Description("Canal al que enviar mensajes")]
    public string Channel { get; set; } = string.Empty;

    [Category(Startup), Description("Prefijo de comandos del bot")]
    public char CommandPrefix { get; set; } = '$';

    [Category(Operation), Description("Mensaje enviado cuando se libera la barrera.")]
    public string MessageStart { get; set; } = string.Empty;

    // Messaging

    [Category(Operation), Description("Limitar el bot de enviar mensajes si X mensajes han sido enviados en los últimos Y segundos.")]
    public int ThrottleMessages { get; set; } = 100;

    [Category(Operation), Description("Limitar el bot de enviar mensajes si X mensajes han sido enviados en los últimos Y segundos.")]
    public double ThrottleSeconds { get; set; } = 30;

    [Category(Operation), Description("Limitar el bot de enviar susurros si X mensajes han sido enviados en los últimos Y segundos.")]
    public int ThrottleWhispers { get; set; } = 100;

    [Category(Operation), Description("Limitar el bot de enviar susurros si X mensajes han sido enviados en los últimos Y segundos.")]
    public double ThrottleWhispersSeconds { get; set; } = 60;

    // Operation

    [Category(Operation), Description("Nombre de usuarios Sudo")]
    public string SudoList { get; set; } = string.Empty;

    [Category(Operation), Description("Usuarios con estos nombres de usuario no pueden usar el bot.")]
    public string UserBlacklist { get; set; } = string.Empty;

    [Category(Operation), Description("Cuando está habilitado, el bot procesará los comandos enviados al canal.")]
    public bool AllowCommandsViaChannel { get; set; } = true;

    [Category(Operation), Description("Cuando está habilitado, el bot permitirá a los usuarios enviar comandos mediante susurro (ignora el modo lento)")]
    public bool AllowCommandsViaWhisper { get; set; }

    // Message Destinations

    [Category(Messages), Description("Determina donde se envían las notificaciones genéricas.")]
    public TwitchMessageDestination NotifyDestination { get; set; }

    [Category(Messages), Description("Determina donde se envían las notificaciones de inicio de intercambio.")]
    public TwitchMessageDestination TradeStartDestination { get; set; } = TwitchMessageDestination.Channel;

    [Category(Messages), Description("Determina donde se envían las notificaciones de búsqueda de intercambio.")]
    public TwitchMessageDestination TradeSearchDestination { get; set; }

    [Category(Messages), Description("Determina donde se envían las notificaciones de finalización de intercambio.")]
    public TwitchMessageDestination TradeFinishDestination { get; set; }

    [Category(Messages), Description("Determina donde se envían las notificaciones de cancelación de intercambio.")]
    public TwitchMessageDestination TradeCanceledDestination { get; set; } = TwitchMessageDestination.Channel;

    [Category(Messages), Description("Determina si los intercambios de distribución cuentan hacia atrás antes de comenzar.")]
    public bool DistributionCountDown { get; set; } = true;

    public bool IsSudo(string username)
    {
        var sudos = SudoList.Split([ ",", ", ", " " ], StringSplitOptions.RemoveEmptyEntries);
        return sudos.Contains(username);
    }
}

public enum TwitchMessageDestination
{
    Disabled,
    Channel,
    Whisper,
}
