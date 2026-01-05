using PKHeX.Core;
using SysBot.Base;
using System;
using System.Linq;

namespace SysBot.Pokemon;

public class PokeTradeLogNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
{
    public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        LogUtil.LogInfo($"Iniciando ciclo de intercambio para {info.Trainer.TrainerName}, enviando {routine.GetSpeciesName(info.TradeData.Species)}", routine.Connection.Label);
    }

    public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        LogUtil.LogInfo($"Buscando intercambio con {info.Trainer.TrainerName}, enviando {routine.GetSpeciesName(info.TradeData.Species)}", routine.Connection.Label);
    }

    public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
    {
        LogUtil.LogInfo($"Cancelando intercambio con {info.Trainer.TrainerName}, porque {msg}.", routine.Connection.Label);
        OnFinish?.Invoke(routine);
    }

    public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
    {
        // Print the nickname for Ledy trades so we can see what was requested.
        var ledyname = string.Empty;
        if (info.Trainer.TrainerName == "Random Distribution" && result.IsNicknamed)
            ledyname = $" (Nickname: \"{result.Nickname}\")";

        LogUtil.LogInfo($"Intercambio finalizado: {info.Trainer.TrainerName} {routine.GetSpeciesName(info.TradeData.Species)} por {routine.GetSpeciesName(result.Species)}{ledyname}", routine.Connection.Label);
        OnFinish?.Invoke(routine);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
    {
        LogUtil.LogInfo(message, routine.Connection.Label);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
    {
        var msg = message.Summary;
        if (message.Details.Count > 0)
            msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
        LogUtil.LogInfo(msg, routine.Connection.Label);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
    {
        LogUtil.LogInfo($"Notificando a {info.Trainer.TrainerName} sobre su {routine.GetSpeciesName(result.Species)}", routine.Connection.Label);
        LogUtil.LogInfo(message, routine.Connection.Label);
    }

    public Action<PokeRoutineExecutor<T>>? OnFinish { get; set; }
}
