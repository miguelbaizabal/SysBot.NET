using PKHeX.Core;

namespace SysBot.Pokemon;

/// <summary>
/// Stores data for indicating how a queue position/presence check resulted.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record QueueCheckResult<T>(
    bool InQueue = false,
    TradeEntry<T>? Detail = null,
    int Position = -1,
    int QueueCount = -1)
    where T : PKM, new()
{
    public static readonly QueueCheckResult<T> None = new();

    public string GetMessage()
    {
        if (!InQueue || Detail is null)
            return "No estás en la cola.";
        var position = $"{Position}/{QueueCount}";
        var msg = $"Estás en la cola de {Detail.Type}! Posición: {position} (ID {Detail.Trade.ID})";
        var pk = Detail.Trade.TradeData;
        if (pk.Species != 0)
            msg += $", Recibiendo: {GameInfo.GetStrings("es").Species[pk.Species]}";
        return msg;
    }
}
