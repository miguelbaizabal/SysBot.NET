using System.ComponentModel;

namespace SysBot.Pokemon;

public class TimingSettings
{
    private const string OpenGame = nameof(OpenGame);
    private const string CloseGame = nameof(CloseGame);
    private const string Raid = nameof(Raid);
    private const string Misc = nameof(Misc);
    public override string ToString() => "Configuración de Tiempo Extra";

    // Opening the game.
    [Category(OpenGame), Description("Tiempo extra en milisegundos para esperar a que se carguen los perfiles al iniciar el juego.")]
    public int ExtraTimeLoadProfile { get; set; }

    [Category(OpenGame), Description("Tiempo extra en milisegundos para esperar antes de hacer clic en A en la pantalla de título.")]
    public int ExtraTimeLoadGame { get; set; } = 5000;

    [Category(OpenGame), Description("Tiempo extra en milisegundos para esperar a que se cargue el mundo exterior después de la pantalla de título.")]
    public int ExtraTimeLoadOverworld { get; set; } = 3000;

    // Closing the game.
    [Category(CloseGame), Description("Tiempo extra en milisegundos para esperar después de presionar HOME para minimizar el juego.")]
    public int ExtraTimeReturnHome { get; set; }

    [Category(CloseGame), Description("Tiempo extra en milisegundos para esperar después de hacer clic para cerrar el juego.")]
    public int ExtraTimeCloseGame { get; set; }

    // Raid-specific timings.
    [Category(Raid), Description("[RaidBot] Tiempo extra en milisegundos para esperar a que se cargue la raid después de hacer clic en el den.")]
    public int ExtraTimeLoadRaid { get; set; }

    [Category(Raid), Description("[RaidBot] Tiempo extra en milisegundos para esperar después de hacer clic en \"Invitar a otros\" antes de bloquearse en un Pokémon.")]
    public int ExtraTimeOpenRaid { get; set; }

    [Category(Raid), Description("[RaidBot] Tiempo extra en milisegundos para esperar antes de cerrar el juego para reiniciar la raid.")]
    public int ExtraTimeEndRaid { get; set; }

    [Category(Raid), Description("[RaidBot] Tiempo extra en milisegundos para esperar después de aceptar un amigo.")]
    public int ExtraTimeAddFriend { get; set; }

    [Category(Raid), Description("[RaidBot] Tiempo extra en milisegundos para esperar después de eliminar un amigo.")]
    public int ExtraTimeDeleteFriend { get; set; }

    // Miscellaneous settings.
    [Category(Misc), Description("[SWSH/SV] Tiempo extra en milisegundos para esperar después de hacer clic en + para conectarse a Y-Comm (SWSH) o L para conectarse en línea (SV).")]
    public int ExtraTimeConnectOnline { get; set; }

    [Category(Misc), Description("Número de veces que se intentará reconectar a una conexión socket después de perder la conexión. Establezca esto en -1 para intentar indefinidamente.")]
    public int ReconnectAttempts { get; set; } = 30;

    [Category(Misc), Description("Tiempo extra en milisegundos para esperar entre intentos de reconexión. El tiempo base es 30 segundos.")]
    public int ExtraReconnectDelay { get; set; }

    [Category(Misc), Description("[BDSP] Tiempo extra en milisegundos para esperar a que se cargue el mundo tras salir de la Sala Unión.")]
    public int ExtraTimeLeaveUnionRoom { get; set; } = 1000;

    [Category(Misc), Description("[BDSP] Tiempo extra en milisegundos para esperar a que se cargue el menú Y al inicio de cada bucle de intercambio.")]
    public int ExtraTimeOpenYMenu { get; set; } = 500;

    [Category(Misc), Description("[BDSP] Tiempo extra en milisegundos para esperar a que se cargue la Sala Unión antes de intentar llamar a un intercambio.")]
    public int ExtraTimeJoinUnionRoom { get; set; } = 500;

    [Category(Misc), Description("[SV] Tiempo extra en milisegundos para esperar a que se cargue el Poké Portal.")]
    public int ExtraTimeLoadPortal { get; set; } = 1000;

    [Category(Misc), Description("Tiempo extra en milisegundos para esperar a que se cargue la caja después de encontrar un intercambio.")]
    public int ExtraTimeOpenBox { get; set; } = 1000;

    [Category(Misc), Description("Tiempo extra en milisegundos para esperar después de abrir el teclado para ingresar códigos durante los intercambios.")]
    public int ExtraTimeOpenCodeEntry { get; set; } = 1000;

    [Category(Misc), Description("Tiempo a esperar después de cada pulsación de tecla al navegar por los menús de la consola o ingresar el código de enlace.")]
    public int KeypressTime { get; set; }

    [Category(Misc), Description("Habilita esto para rechazar actualizaciones del sistema entrantes.")]
    public bool AvoidSystemUpdate { get; set; }
}
