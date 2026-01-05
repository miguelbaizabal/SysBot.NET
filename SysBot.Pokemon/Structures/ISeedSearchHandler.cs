using PKHeX.Core;

namespace SysBot.Pokemon;

public interface ISeedSearchHandler<T> where T : PKM, new()
{
    void CalculateAndNotify(T pkm, PokeTradeDetail<T> detail, SeedCheckSettings settings, PokeRoutineExecutor<T> bot);
}

public class NoSeedSearchHandler<T> : ISeedSearchHandler<T> where T : PKM, new()
{
    public void CalculateAndNotify(T pkm, PokeTradeDetail<T> detail, SeedCheckSettings settings, PokeRoutineExecutor<T> bot)
    {
        const string msg = "Implementación de búsqueda de semillas no encontrada. " +
                           "Por favor, avise a la persona que está ejecutando el bot que necesita proporcionar los archivos Z3 requeridos.";
        detail.SendNotification(bot, msg);
    }
}
