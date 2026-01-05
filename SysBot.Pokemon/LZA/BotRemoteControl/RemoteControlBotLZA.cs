using SysBot.Base;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon;

public class RemoteControlBotLZA(PokeBotState Config) : PokeRoutineExecutor9LZA(Config)
{
    public override async Task MainLoop(CancellationToken token)
    {
        try
        {
            Log("Identificando datos del entrenador del host.");
            await IdentifyTrainer(token).ConfigureAwait(false);

            Log("Iniciando bucle principal, esperando comandos.");
            Config.IterateNextRoutine();
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1_000, token).ConfigureAwait(false);
                ReportStatus();
            }
        }
        catch (Exception e)
        {
            Log(e.Message);
        }

        Log($"Finalizando bucle de {nameof(RemoteControlBotLZA)}.");
        await HardStop().ConfigureAwait(false);
    }

    public override async Task HardStop()
    {
        await SetStick(SwitchStick.LEFT, 0, 0, 0_500, CancellationToken.None).ConfigureAwait(false); // reset
        await CleanExit(CancellationToken.None).ConfigureAwait(false);
    }

    private class DummyReset : IBotStateSettings
    {
        public bool ScreenOff => true;
    }
}
