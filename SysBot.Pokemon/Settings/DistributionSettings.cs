using PKHeX.Core;
using SysBot.Base;
using System.ComponentModel;

namespace SysBot.Pokemon;

public class DistributionSettings : ISynchronizationSetting
{
    private const string Distribute = nameof(Distribute);
    private const string Synchronize = nameof(Synchronize);
    public override string ToString() => "Configuración de Intercambio de Distribución";

    // Distribute

    [Category(Distribute), Description("Cuando está habilitado, los bots de intercambio en línea distribuirán aleatoriamente archivos PKM desde la carpeta de Distribución.")]
    public bool DistributeWhileIdle { get; set; } = true;

    [Category(Distribute), Description("Cuando está habilitado, la carpeta de Distribución producirá resultados aleatorios en lugar de seguir una secuencia fija.")]
    public bool Shuffled { get; set; }

    [Category(Distribute), Description("Cuando se establece en algo distinto de None, los intercambios aleatorios requerirán esta especie además de la coincidencia del apodo.")]
    public Species LedySpecies { get; set; } = Species.None;

    [Category(Distribute), Description("Cuando se establece en true, los intercambios aleatorios de cambio de apodo de Ledy se cancelarán en lugar de intercambiar una entidad aleatoria del conjunto.")]
    public bool LedyQuitIfNoMatch { get; set; }

    [Category(Distribute), Description("Código de intercambio de distribución.")]
    public int TradeCode { get; set; } = 7196;

    [Category(Distribute), Description("Código de intercambio de distribución usa el rango Min y Max en lugar del código de intercambio fijo.")]
    public bool RandomCode { get; set; }

    [Category(Distribute), Description("Para BDSP, el bot de distribución irá a una habitación específica y permanecerá allí hasta que se detenga el bot.")]
    public bool RemainInUnionRoomBDSP { get; set; } = true;

    // Synchronize

    [Category(Synchronize), Description("Link Trade: Al usar múltiples bots de distribución, todos los bots confirmarán su código de intercambio al mismo tiempo. En modo Local, los bots continuarán cuando todos estén en la barrera. En modo Remoto, algo externo debe indicar a los bots que continúen.")]
    public BotSyncOption SynchronizeBots { get; set; } = BotSyncOption.LocalSync;

    [Category(Synchronize), Description("Link Trade: Al usar múltiples bots de distribución, una vez que todos los bots estén listos para confirmar el código de intercambio, el Hub esperará X milisegundos antes de liberar a todos los bots.")]
    public int SynchronizeDelayBarrier { get; set; }

    [Category(Synchronize), Description("Link Trade: Al usar múltiples bots de distribución, cuánto tiempo (en segundos) esperará un bot para sincronizarse antes de continuar de todos modos.")]
    public double SynchronizeTimeout { get; set; } = 90;
}
