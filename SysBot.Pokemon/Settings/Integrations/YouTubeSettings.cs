using System;
using System.ComponentModel;
using System.Linq;

namespace SysBot.Pokemon;

public class YouTubeSettings
{
    private const string Startup = nameof(Startup);
    private const string Operation = nameof(Operation);
    private const string Messages = nameof(Messages);
    public override string ToString() => "Configuración de Integración de YouTube";

    // Startup

    [Category(Startup), Description("ClientID del Bot")]
    public string ClientID { get; set; } = string.Empty;

    [Category(Startup), Description("Client Secret del Bot")]
    public string ClientSecret { get; set; } = string.Empty;

    [Category(Startup), Description("ID del Canal al que enviar mensajes")]
    public string ChannelID { get; set; } = string.Empty;

    [Category(Startup), Description("Prefijo de comandos del bot")]
    public char CommandPrefix { get; set; } = '$';

    [Category(Operation), Description("Mensaje enviado cuando se libera la barrera.")]
    public string MessageStart { get; set; } = string.Empty;

    // Operation

    [Category(Operation), Description("Nombres de usuarios Sudo")]
    public string SudoList { get; set; } = string.Empty;

    [Category(Operation), Description("Usuarios con estos nombres de usuario no pueden usar el bot.")]
    public string UserBlacklist { get; set; } = string.Empty;

    public bool IsSudo(string username)
    {
        var sudos = SudoList.Split([ ",", ", ", " " ], StringSplitOptions.RemoveEmptyEntries);
        return sudos.Contains(username);
    }
}

public enum YouTubeMessageDestination
{
    Disabled,
    Channel,
}
