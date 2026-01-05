using PKHeX.Core;
using SysBot.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SysBot.Pokemon;

public class TradeSettings : IBotStateSettings, ICountSettings
{
    private const string TradeCode = nameof(TradeCode);
    private const string TradeConfig = nameof(TradeConfig);
    private const string Dumping = nameof(Dumping);
    private const string Counts = nameof(Counts);
    public override string ToString() => "Configuración de Trade Bot";

    [Category(TradeConfig), Description("Tiempo en segundos que se espera a un compañero de intercambio.")]
    public int TradeWaitTime { get; set; } = 30;

    [Category(TradeConfig), Description("Tiempo máximo en segundos presionando A para esperar a que se procese un intercambio.")]
    public int MaxTradeConfirmTime { get; set; } = 25;

    [Category(TradeCode), Description("Código de intercambio mínimo.")]
    public int MinTradeCode { get; set; } = 8180;

    [Category(TradeCode), Description("Código de intercambio máximo.")]
    public int MaxTradeCode { get; set; } = 8199;

    [Category(Dumping), Description("Dump Trade: La rutina de Dumping se detendrá después de un número máximo de dumps de un solo usuario.")]
    public int MaxDumpsPerTrade { get; set; } = 20;

    [Category(Dumping), Description("Dump Trade: La rutina de Dumping se detendrá después de pasar x segundos en el intercambio.")]
    public int MaxDumpTradeTime { get; set; } = 180;

    [Category(Dumping), Description("Dump Trade: Si está habilitado, la rutina de Dumping mostrará información de verificación de legalidad al usuario.")]
    public bool DumpTradeLegalityCheck { get; set; } = true;

    [Category(TradeConfig), Description("Cuando está habilitado, la pantalla se apagará durante la operación normal del bot para ahorrar energía.")]
    public bool ScreenOff { get; set; }

    [Category(TradeConfig), Description("Cuando está habilitado, no se permite solicitar Pokémon fuera de su contexto original.")]
    public bool DisallowNonNatives { get; set; } = true;

    [Category(TradeConfig), Description("Cuando está habilitado, no se permite solicitar Pokémon si tienen un rastreador de HOME.")]
    public bool DisallowTracked { get; set; } = true;

    [Category(TradeConfig), Description("Cuando está habilitado, el bot cancelará automáticamente un intercambio si se le ofrece un Pokémon que evolucionará.")]
    public bool DisallowTradeEvolve { get; set; } = true;

    /// <summary>
    /// Gets a random trade code based on the range settings.
    /// </summary>
    public int GetRandomTradeCode() => Util.Rand.Next(MinTradeCode, MaxTradeCode + 1);

    private int _completedSurprise;
    private int _completedDistribution;
    private int _completedTrades;
    private int _completedSeedChecks;
    private int _completedClones;
    private int _completedDumps;

    [Category(Counts), Description("Intercambios Sorpresa completados")]
    public int CompletedSurprise
    {
        get => _completedSurprise;
        set => _completedSurprise = value;
    }

    [Category(Counts), Description("Intercambios completados (Distribución)")]
    public int CompletedDistribution
    {
        get => _completedDistribution;
        set => _completedDistribution = value;
    }

    [Category(Counts), Description("Intercambios completados (Usuario específico)")]
    public int CompletedTrades
    {
        get => _completedTrades;
        set => _completedTrades = value;
    }

    [Category(Counts), Description("Verificaciones de Semilla completadas")]
    public int CompletedSeedChecks
    {
        get => _completedSeedChecks;
        set => _completedSeedChecks = value;
    }

    [Category(Counts), Description("Clonaciones completadas (Usuario específico)")]
    public int CompletedClones
    {
        get => _completedClones;
        set => _completedClones = value;
    }

    [Category(Counts), Description("Dumps completados (Usuario específico)")]
    public int CompletedDumps
    {
        get => _completedDumps;
        set => _completedDumps = value;
    }

    [Category(Counts), Description("Cuando está habilitado, las estadísticas se mostrarán cuando se solicite una verificación de estado.")]
    public bool EmitCountsOnStatusCheck { get; set; }

    public void AddCompletedTrade() => Interlocked.Increment(ref _completedTrades);
    public void AddCompletedSeedCheck() => Interlocked.Increment(ref _completedSeedChecks);
    public void AddCompletedSurprise() => Interlocked.Increment(ref _completedSurprise);
    public void AddCompletedDistribution() => Interlocked.Increment(ref _completedDistribution);
    public void AddCompletedDumps() => Interlocked.Increment(ref _completedDumps);
    public void AddCompletedClones() => Interlocked.Increment(ref _completedClones);

    public IEnumerable<string> GetNonZeroCounts()
    {
        if (!EmitCountsOnStatusCheck)
            yield break;
        if (CompletedSeedChecks != 0)
            yield return $"Verificaciones de Semilla: {CompletedSeedChecks}";
        if (CompletedClones != 0)
            yield return $"Clonaciones: {CompletedClones}";
        if (CompletedDumps != 0)
            yield return $"Dumps: {CompletedDumps}";
        if (CompletedTrades != 0)
            yield return $"Intercambios: {CompletedTrades}";
        if (CompletedDistribution != 0)
            yield return $"Intercambios de Distribución: {CompletedDistribution}";
        if (CompletedSurprise != 0)
            yield return $"Intercambios Sorpresa: {CompletedSurprise}";
    }
}
