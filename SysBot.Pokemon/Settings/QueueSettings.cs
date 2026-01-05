using System;
using System.ComponentModel;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SysBot.Pokemon;

public class QueueSettings
{
    private const string FeatureToggle = nameof(FeatureToggle);
    private const string UserBias = nameof(UserBias);
    private const string TimeBias = nameof(TimeBias);
    private const string QueueToggle = nameof(QueueToggle);
    public override string ToString() => "Configuración de Unión a Cola";

    // General

    [Category(FeatureToggle), Description("Determina si los usuarios pueden unirse a la cola.")]
    public bool CanQueue { get; set; } = true;

    [Category(FeatureToggle), Description("Previene agregar usuarios si ya hay este número de usuarios en la cola.")]
    public int MaxQueueCount { get; set; } = 999;

    [Category(FeatureToggle), Description("Permite a los usuarios salir de la cola mientras son intercambiados.")]
    public bool CanDequeueIfProcessing { get; set; }

    [Category(FeatureToggle), Description("Determina cómo se procesarán las colas en el modo flexible.")]
    public FlexYieldMode FlexMode { get; set; } = FlexYieldMode.Weighted;

    [Category(FeatureToggle), Description("Determina cuándo se enciende y apaga la cola.")]
    public QueueOpening QueueToggleMode { get; set; } = QueueOpening.Threshold;

    // Queue Toggle

    [Category(QueueToggle), Description("Threshold Mode: Conteo de usuarios que causarán que la cola se abra.")]
    public int ThresholdUnlock { get; set; }

    [Category(QueueToggle), Description("Threshold Mode: Conteo de usuarios que causarán que la cola se cierre.")]
    public int ThresholdLock { get; set; } = 30;

    [Category(QueueToggle), Description("Scheduled Mode: Segundos de estar abierta antes de que la cola se cierre.")]
    public int IntervalOpenFor { get; set; } = 5 * 60;

    [Category(QueueToggle), Description("Scheduled Mode: Segundos de estar cerrada antes de que la cola se abra.")]
    public int IntervalCloseFor { get; set; } = 15 * 60;

    // Flex Users

    [Category(UserBias), Description("Ajusta el peso de la cola de intercambios según cuántos usuarios haya en la cola.")]
    public int YieldMultCountTrade { get; set; } = 100;

    [Category(UserBias), Description("Ajusta el peso de la cola de verificación de semillas según cuántos usuarios haya en la cola.")]
    public int YieldMultCountSeedCheck { get; set; } = 100;

    [Category(UserBias), Description("Ajusta el peso de la cola de clonación según cuántos usuarios haya en la cola.")]
    public int YieldMultCountClone { get; set; } = 100;

    [Category(UserBias), Description("Ajusta el peso de la cola de dumpeo según cuántos usuarios haya en la cola.")]
    public int YieldMultCountDump { get; set; } = 100;

    // Flex Time

    [Category(TimeBias), Description("Determina si el peso debe ser sumado o multiplicado al peso total.")]
    public FlexBiasMode YieldMultWait { get; set; } = FlexBiasMode.Multiply;

    [Category(TimeBias), Description("Verifica el tiempo transcurrido desde que el usuario se unió a la cola de intercambio, y aumenta el peso de la cola en consecuencia.")]
    public int YieldMultWaitTrade { get; set; } = 1;

    [Category(TimeBias), Description("Verifica el tiempo transcurrido desde que el usuario se unió a la cola de verificación de semillas, y aumenta el peso de la cola en consecuencia.")]
    public int YieldMultWaitSeedCheck { get; set; } = 1;

    [Category(TimeBias), Description("Verifica el tiempo transcurrido desde que el usuario se unió a la cola de clonación, y aumenta el peso de la cola en consecuencia.")]
    public int YieldMultWaitClone { get; set; } = 1;

    [Category(TimeBias), Description("Verifica el tiempo transcurrido desde que el usuario se unió a la cola de dumpeo, y aumenta el peso de la cola en consecuencia.")]
    public int YieldMultWaitDump { get; set; } = 1;

    [Category(TimeBias), Description("Multiplica la cantidad de usuarios en la cola para dar una estimación del tiempo que tomará hasta que el usuario sea procesado.")]
    public float EstimatedDelayFactor { get; set; } = 1.1f;

    private int GetCountBias(PokeTradeType type) => type switch
    {
        PokeTradeType.Seed => YieldMultCountSeedCheck,
        PokeTradeType.Clone => YieldMultCountClone,
        PokeTradeType.Dump => YieldMultCountDump,
        _ => YieldMultCountTrade,
    };

    private int GetTimeBias(PokeTradeType type) => type switch
    {
        PokeTradeType.Seed => YieldMultWaitSeedCheck,
        PokeTradeType.Clone => YieldMultWaitClone,
        PokeTradeType.Dump => YieldMultWaitDump,
        _ => YieldMultWaitTrade,
    };

    /// <summary>
    /// Gets the weight of a <see cref="PokeTradeType"/> based on the count of users in the queue and time users have waited.
    /// </summary>
    /// <param name="count">Count of users for <see cref="type"/></param>
    /// <param name="time">Next-to-be-processed user's time joining the queue</param>
    /// <param name="type">Queue type</param>
    /// <returns>Effective weight for the trade type.</returns>
    public long GetWeight(int count, DateTime time, PokeTradeType type)
    {
        var now = DateTime.Now;
        var seconds = (now - time).Seconds;

        var cb = GetCountBias(type) * count;
        var tb = GetTimeBias(type) * seconds;

        return YieldMultWait switch
        {
            FlexBiasMode.Multiply => cb * tb,
            _ => cb + tb,
        };
    }

    /// <summary>
    /// Estimates the amount of time (minutes) until the user will be processed.
    /// </summary>
    /// <param name="position">Position in the queue</param>
    /// <param name="botct">Amount of bots processing requests</param>
    /// <returns>Estimated time in Minutes</returns>
    public float EstimateDelay(int position, int botct) => (EstimatedDelayFactor * position) / botct;
}

public enum FlexBiasMode
{
    Add,
    Multiply,
}

public enum FlexYieldMode
{
    LessCheatyFirst,
    Weighted,
}

public enum QueueOpening
{
    Manual,
    Threshold,
    Interval,
}
