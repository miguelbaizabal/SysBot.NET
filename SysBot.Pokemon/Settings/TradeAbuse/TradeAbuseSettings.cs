using System.ComponentModel;

namespace SysBot.Pokemon;

public class TradeAbuseSettings
{
    private const string Monitoring = nameof(Monitoring);
    public override string ToString() => "Configuración de Monitoreo de Abuso en Intercambios";

    [Category(Monitoring), Description("Cuando una persona aparece nuevamente en menos de este valor (minutos), se enviará una notificación.")]
    public double TradeCooldown { get; set; }

    [Category(Monitoring), Description("Cuando una persona ignora un tiempo de espera de intercambio, el mensaje de eco incluirá su ID de cuenta Nintendo.")]
    public bool EchoNintendoOnlineIDCooldown { get; set; } = true;

    [Category(Monitoring), Description("Si no está vacío, la cadena proporcionada se agregará a las alertas de eco para notificar a quien usted especifique cuando un usuario viola el tiempo de espera de intercambio. Para Discord, use <@userIDnumber> para mencionar.")]
    public string CooldownAbuseEchoMention { get; set; } = string.Empty;

    [Category(Monitoring), Description("Cuando una persona aparece con una cuenta diferente de Discord/Twitch en menos de este valor (minutos), se enviará una notificación.")]
    public double TradeAbuseExpiration { get; set; } = 120;

    [Category(Monitoring), Description("Cuando una persona usando múltiples cuentas de Discord/Twitch es detectada, el mensaje de eco incluirá su ID de cuenta Nintendo.")]
    public bool EchoNintendoOnlineIDMulti { get; set; } = true;

    [Category(Monitoring), Description("Cuando una persona que envía a múltiples cuentas en juego es detectada, el mensaje de eco incluirá su ID de cuenta Nintendo.")]
    public bool EchoNintendoOnlineIDMultiRecipients { get; set; } = true;

    [Category(Monitoring), Description("Cuando una persona usando múltiples cuentas de Discord/Twitch es detectada, esta acción se tomará.")]
    public TradeAbuseAction TradeAbuseAction { get; set; } = TradeAbuseAction.Quit;

    [Category(Monitoring), Description("Cuando una persona es bloqueada en el juego por múltiples cuentas, su ID en línea se agrega a BannedIDs.")]
    public bool BanIDWhenBlockingUser { get; set; } = true;

    [Category(Monitoring), Description("Si no está vacío, la cadena proporcionada se agregará a las alertas de eco para notificar a quien usted especifique cuando un usuario es encontrado usando múltiples cuentas. Para Discord, use <@userIDnumber> para mencionar.")]
    public string MultiAbuseEchoMention { get; set; } = string.Empty;

    [Category(Monitoring), Description("Si no está vacío, la cadena proporcionada se agregará a las alertas de eco para notificar a quien usted especifique cuando un usuario es encontrado enviando a múltiples jugadores en el juego. Para Discord, use <@userIDnumber> para mencionar.")]
    public string MultiRecipientEchoMention { get; set; } = string.Empty;

    [Category(Monitoring), Description("IDs baneados en línea que activarán la salida del intercambio o bloqueo in-game.")]
    public RemoteControlAccessList BannedIDs { get; set; } = new();

    [Category(Monitoring), Description("Cuando una persona es encontrada con un ID baneado, bloquearla en el juego antes de salir del intercambio.")]
    public bool BlockDetectedBannedUser { get; set; } = true;

    [Category(Monitoring), Description("Si no está vacío, la cadena proporcionada se agregará a las alertas de eco para notificar a quien usted especifique cuando un usuario coincide con un ID baneado. Para Discord, use <@userIDnumber> para mencionar.")]
    public string BannedIDMatchEchoMention { get; set; } = string.Empty;

    [Category(Monitoring), Description("Cuando se detecta abuso por parte de una persona que usa intercambios de apodo de Ledy, el mensaje de eco incluirá su ID de Cuenta Nintendo.")]
    public bool EchoNintendoOnlineIDLedy { get; set; } = true;

    [Category(Monitoring), Description("Si no está vacío, la cadena proporcionada se agregará a las alertas de eco para notificar a quien usted especifique cuando un usuario viola las reglas de intercambio de Ledy. Para Discord, use <@userIDnumber> para mencionar.")]
    public string LedyAbuseEchoMention { get; set; } = string.Empty;
}
