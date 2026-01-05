using System.ComponentModel;

namespace SysBot.Pokemon;

public class DiscordSettings
{
    private const string Startup = nameof(Startup);
    private const string Operation = nameof(Operation);
    private const string Channels = nameof(Channels);
    private const string Roles = nameof(Roles);
    private const string Users = nameof(Users);
    public override string ToString() => "Configuración de Integración de Discord";

    // Startup

    [Category(Startup), Description("Token de autenticación del bot.")]
    public string Token { get; set; } = string.Empty;

    [Category(Startup), Description("Prefijo de comandos del bot.")]
    public string CommandPrefix { get; set; } = "$";

    [Category(Startup), Description("Lista de módulos que no se cargarán cuando se inicie el bot (separados por comas).")]
    public string ModuleBlacklist { get; set; } = string.Empty;

    [Category(Startup), Description("Alternar para manejar comandos de forma asíncrona o sincrónica.")]
    public bool AsyncCommands { get; set; }

    [Category(Startup), Description("Estado personalizado de juego del bot de Discord.")]
    public string BotGameStatus { get; set; } = "SysBot.NET: Pokémon";

    [Category(Startup), Description("Indica el color del estado de presencia de Discord considerando solo bots tipo Trade.")]
    public bool BotColorStatusTradeOnly { get; set; } = true;

    [Category(Operation), Description("Mensaje personalizado que el bot responderá cuando un usuario le diga hola. Usa formato de cadena para mencionar al usuario en la respuesta.")]
    public string HelloResponse { get; set; } = "¡Hola {0}!";
    // Whitelists

    [Category(Roles), Description("Usuarios con este rol pueden entrar a la cola de Trade.")]
    public RemoteControlAccessList RoleCanTrade { get; set; } = new() { AllowIfEmpty = false };

    [Category(Roles), Description("Usuarios con este rol pueden entrar a la cola de Seed Check.")]
    public RemoteControlAccessList RoleCanSeedCheck { get; set; } = new() { AllowIfEmpty = false };

    [Category(Roles), Description("Usuarios con este rol pueden entrar a la cola de Clone.")]
    public RemoteControlAccessList RoleCanClone { get; set; } = new() { AllowIfEmpty = false };

    [Category(Roles), Description("Usuarios con este rol pueden entrar a la cola de Dump.")]
    public RemoteControlAccessList RoleCanDump { get; set; } = new() { AllowIfEmpty = false };

    [Category(Roles), Description("Usuarios con este rol pueden controlar remotamente la consola (si se ejecuta como Bot de Control Remoto).")]
    public RemoteControlAccessList RoleRemoteControl { get; set; } = new() { AllowIfEmpty = false };

    [Category(Roles), Description("Usuarios con este rol pueden ignorar restricciones de comandos.")]
    public RemoteControlAccessList RoleSudo { get; set; } = new() { AllowIfEmpty = false };

    // Operation

    [Category(Roles), Description("Usuarios con este rol pueden entrar a la cola con una posición mejor.")]
    public RemoteControlAccessList RoleFavored { get; set; } = new() { AllowIfEmpty = false };

    [Category(Users), Description("Usuarios con estos IDs de usuario no pueden usar el bot.")]
    public RemoteControlAccessList UserBlacklist { get; set; } = new();

    [Category(Channels), Description("Canales con estos IDs son los únicos canales donde el bot reconoce comandos.")]
    public RemoteControlAccessList ChannelWhitelist { get; set; } = new();

    [Category(Users), Description("IDs de usuarios de Discord separados por comas que tendrán acceso sudo al Bot Hub.")]
    public RemoteControlAccessList GlobalSudoList { get; set; } = new();

    [Category(Users), Description("Desactivar esto eliminará el soporte global sudo.")]
    public bool AllowGlobalSudo { get; set; } = true;

    [Category(Channels), Description("IDs de canales que mostrarán los datos del registro del bot.")]
    public RemoteControlAccessList LoggingChannels { get; set; } = new();

    [Category(Channels), Description("Canales de registro que mostrarán mensajes de inicio de intercambio.")]
    public RemoteControlAccessList TradeStartingChannels { get; set; } = new();

    [Category(Channels), Description("Canales de eco que registrarán mensajes especiales.")]
    public RemoteControlAccessList EchoChannels { get; set; } = new();

    [Category(Operation), Description("Devuelve PKMs de Pokémon mostrados en el intercambio al usuario.")]
    public bool ReturnPKMs { get; set; } = true;

    [Category(Operation), Description("Responde a los usuarios si no tienen permiso para usar un comando en el canal. Cuando es falso, el bot lo ignorará silenciosamente.")]
    public bool ReplyCannotUseCommandInChannel { get; set; } = true;

    [Category(Operation), Description("El bot escucha los mensajes del canal para responder con un ShowdownSet cada vez que se adjunta un archivo PKM (no con un comando).")]
    public bool ConvertPKMToShowdownSet { get; set; } = true;

    [Category(Operation), Description("El bot puede responder con un ShowdownSet en cualquier canal que el bot pueda ver, en lugar de solo los canales en los que ha sido autorizado a ejecutarse. Solo habilítelo si desea que el bot sirva más utilidad en canales no-bots.")]
    public bool ConvertPKMReplyAnyChannel { get; set; }
}
