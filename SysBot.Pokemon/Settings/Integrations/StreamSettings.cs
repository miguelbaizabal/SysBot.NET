using PKHeX.Core;
using SysBot.Base;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SysBot.Pokemon;

public class StreamSettings
{
    private const string Operation = nameof(Operation);

    public override string ToString() => "Configuración de Streaming";
    public static Action<PKM, string>? CreateSpriteFile { get; set; }

    [Category(Operation), Description("Generar assets de streaming; desactivar evitará la generación de assets.")]
    public bool CreateAssets { get; set; }

    [Category(Operation), Description("Generar detalles de inicio de intercambio, indicando con quién está intercambiando el bot.")]
    public bool CreateTradeStart { get; set; } = true;

    [Category(Operation), Description("Generar detalles de inicio de intercambio, indicando qué está intercambiando el bot.")]
    public bool CreateTradeStartSprite { get; set; } = true;

    [Category(Operation), Description("Formato para mostrar los detalles de intercambio en curso. {0} = ID, {1} = Usuario")]
    public string TrainerTradeStart { get; set; } = "(ID {0}) {1}";

    // On Deck

    [Category(Operation), Description("Generar una lista de personas actualmente en espera.")]
    public bool CreateOnDeck { get; set; } = true;

    [Category(Operation), Description("Número de usuarios a mostrar en la lista de espera.")]
    public int OnDeckTake { get; set; } = 5;

    [Category(Operation), Description("Número de usuarios en espera a omitir en la parte superior. Si quieres ocultar a las personas que están siendo procesadas, establece esto al número de consolas que tienes.")]
    public int OnDeckSkip { get; set; }

    [Category(Operation), Description("Separador para dividir los usuarios de la lista de espera.")]
    public string OnDeckSeparator { get; set; } = "\n";

    [Category(Operation), Description("Formato para mostrar los usuarios de la lista de espera. {0} = ID, {3} = Usuario")]
    public string OnDeckFormat { get; set; } = "(ID {0}) - {3}";

    // On Deck 2

    [Category(Operation), Description("Generar una lista de personas actualmente en espera #2.")]
    public bool CreateOnDeck2 { get; set; } = true;

    [Category(Operation), Description("Número de usuarios a mostrar en la lista de espera #2.")]
    public int OnDeckTake2 { get; set; } = 5;

    [Category(Operation), Description("Número de usuarios en espera #2 a omitir en la parte superior. Si quieres ocultar a las personas que están siendo procesadas, establece esto al número de consolas que tienes.")]
    public int OnDeckSkip2 { get; set; }

    [Category(Operation), Description("Separador para dividir los usuarios de la lista de espera #2.")]
    public string OnDeckSeparator2 { get; set; } = "\n";

    [Category(Operation), Description("Formato para mostrar los usuarios de la lista de espera #2. {0} = ID, {3} = Usuario")]
    public string OnDeckFormat2 { get; set; } = "(ID {0}) - {3}";

    // User List

    [Category(Operation), Description("Generar una lista de personas actualmente intercambiando.")]
    public bool CreateUserList { get; set; } = true;

    [Category(Operation), Description("Número de usuarios a mostrar en la lista.")]
    public int UserListTake { get; set; } = -1;

    [Category(Operation), Description("Número de usuarios a omitir en la parte superior. Si quieres ocultar a las personas que están siendo procesadas, establece esto al número de consolas que tienes.")]
    public int UserListSkip { get; set; }

    [Category(Operation), Description("Separador para dividir los usuarios de la lista.")]
    public string UserListSeparator { get; set; } = ", ";

    [Category(Operation), Description("Formato para mostrar los usuarios de la lista. {0} = ID, {3} = Usuario")]
    public string UserListFormat { get; set; } = "(ID {0}) - {3}";

    // TradeCodeBlock

    [Category(Operation), Description("Copia el archivo TradeBlockFile si existe, de lo contrario, se copia una imagen de marcador de posición.")]
    public bool CopyImageFile { get; set; } = true;

    [Category(Operation), Description("Nombre del archivo fuente de la imagen a copiar cuando se ingresa un código de intercambio. Si se deja vacío, se creará una imagen de marcador de posición.")]
    public string TradeBlockFile { get; set; } = string.Empty;

    [Category(Operation), Description("Nombre del archivo de destino de la imagen de bloqueo del código de intercambio. {0} se reemplaza con la dirección IP local.")]
    public string TradeBlockFormat { get; set; } = "block_{0}.png";

    // Waited Time

    [Category(Operation), Description("Crear un archivo que liste el tiempo que ha esperado el usuario más recientemente desencolado.")]
    public bool CreateWaitedTime { get; set; } = true;

    [Category(Operation), Description("Formato para mostrar el tiempo de espera del usuario más recientemente desencolado.")]
    public string WaitedTimeFormat { get; set; } = @"hh\:mm\:ss";

    // Estimated Time

    [Category(Operation), Description("Crear un archivo que liste el tiempo estimado que tendrá que esperar un usuario si se une a la cola.")]
    public bool CreateEstimatedTime { get; set; } = true;

    [Category(Operation), Description("Formato para mostrar el tiempo de espera estimado.")]
    public string EstimatedTimeFormat { get; set; } = "Tiempo estimado: {0:F1} minutos";
    [Category(Operation), Description("Formato para mostrar la marca de tiempo estimada de cumplimiento.")]
    public string EstimatedFulfillmentFormat { get; set; } = @"hh\:mm\:ss";

    // Users in Queue

    [Category(Operation), Description("Crear un archivo indicando la cantidad de usuarios en la cola.")]
    public bool CreateUsersInQueue { get; set; } = true;

    [Category(Operation), Description("Formato para mostrar los usuarios en cola. {0} = Cantidad")]
    public string UsersInQueueFormat { get; set; } = "Usuarios en cola: {0}";
    // Completed Trades

    [Category(Operation), Description("Crear un archivo indicando la cantidad de intercambios completados cuando comienza un nuevo intercambio.")]
    public bool CreateCompletedTrades { get; set; } = true;

    [Category(Operation), Description("Formato para mostrar los intercambios completados. {0} = Cantidad")]
    public string CompletedTradesFormat { get; set; } = "Intercambios completados: {0}";
    public void StartTrade<T>(PokeRoutineExecutorBase b, PokeTradeDetail<T> detail, PokeTradeHub<T> hub) where T : PKM, new()
    {
        if (!CreateAssets)
            return;

        try
        {
            if (CreateTradeStart)
                GenerateBotConnection(b, detail);
            if (CreateWaitedTime)
                GenerateWaitedTime(detail.Time);
            if (CreateEstimatedTime)
                GenerateEstimatedTime(hub);
            if (CreateUsersInQueue)
                GenerateUsersInQueue(hub.Queues.Info.Count);
            if (CreateOnDeck)
                GenerateOnDeck(hub);
            if (CreateOnDeck2)
                GenerateOnDeck2(hub);
            if (CreateUserList)
                GenerateUserList(hub);
            if (CreateCompletedTrades)
                GenerateCompletedTrades(hub);
            if (CreateTradeStartSprite)
                GenerateBotSprite(b, detail);
        }
        catch (Exception e)
        {
            LogUtil.LogError(e.Message, nameof(StreamSettings));
        }
    }

    public void IdleAssets(PokeRoutineExecutorBase b)
    {
        if (!CreateAssets)
            return;

        try
        {
            var files = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                if (file.Contains(b.Connection.Name))
                    File.Delete(file);
            }

            if (CreateWaitedTime)
                File.WriteAllText("waited.txt", "00:00:00");
            if (CreateEstimatedTime)
            {
                File.WriteAllText("estimatedTime.txt", "Tiempo estimado: 0 minutos");
                File.WriteAllText("estimatedTimestamp.txt", "");
            }
            if (CreateOnDeck)
                File.WriteAllText("ondeck.txt", "Esperando...");
            if (CreateOnDeck2)
                File.WriteAllText("ondeck2.txt", "¡La cola está vacía!");
            if (CreateUserList)
                File.WriteAllText("users.txt", "Ninguno");
            if (CreateUsersInQueue)
                File.WriteAllText("queuecount.txt", "Usuarios en cola: 0");
        }
        catch (Exception e)
        {
            LogUtil.LogError(e.Message, nameof(StreamSettings));
        }
    }

    private void GenerateUsersInQueue(int count)
    {
        var value = string.Format(UsersInQueueFormat, count);
        File.WriteAllText("queuecount.txt", value);
    }

    private void GenerateWaitedTime(DateTime time)
    {
        var now = DateTime.Now;
        var difference = now - time;
        var value = difference.ToString(WaitedTimeFormat);
        File.WriteAllText("waited.txt", value);
    }

    private void GenerateEstimatedTime<T>(PokeTradeHub<T> hub) where T : PKM, new()
    {
        var count = hub.Queues.Info.Count;
        var estimate = hub.Config.Queues.EstimateDelay(count, hub.Bots.Count);

        // Minutes
        var wait = string.Format(EstimatedTimeFormat, estimate);
        File.WriteAllText("estimatedTime.txt", wait);

        // Expected to be fulfilled at this time
        var now = DateTime.Now;
        var difference = now.AddMinutes(estimate);
        var date = difference.ToString(EstimatedFulfillmentFormat);
        File.WriteAllText("estimatedTimestamp.txt", date);
    }

    public void StartEnterCode(PokeRoutineExecutorBase b)
    {
        if (!CreateAssets)
            return;

        try
        {
            var file = GetBlockFileName(b);
            if (CopyImageFile && File.Exists(TradeBlockFile))
                File.Copy(TradeBlockFile, file);
            else
                File.WriteAllBytes(file, BlackPixel);
        }
        catch (Exception e)
        {
            LogUtil.LogError(e.Message, nameof(StreamSettings));
        }
    }

    private static readonly byte[] BlackPixel = // 1x1 black pixel
    [
        0x42, 0x4D, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00,
    ];

    public void EndEnterCode(PokeRoutineExecutorBase b)
    {
        try
        {
            var file = GetBlockFileName(b);
            if (File.Exists(file))
                File.Delete(file);
        }
        catch (Exception e)
        {
            LogUtil.LogError(e.Message, nameof(StreamSettings));
        }
    }

    private string GetBlockFileName(PokeRoutineExecutorBase b) => string.Format(TradeBlockFormat, b.Connection.Name);

    private void GenerateBotConnection<T>(PokeRoutineExecutorBase b, PokeTradeDetail<T> detail) where T : PKM, new()
    {
        var file = b.Connection.Name;
        var name = string.Format(TrainerTradeStart, detail.ID, detail.Trainer.TrainerName, (Species)detail.TradeData.Species);
        File.WriteAllText($"{file}.txt", name);
    }

    private static void GenerateBotSprite<T>(PokeRoutineExecutorBase b, PokeTradeDetail<T> detail) where T : PKM, new()
    {
        var func = CreateSpriteFile;
        if (func == null)
            return;
        var file = b.Connection.Name;
        var pk = detail.TradeData;
        func.Invoke(pk, $"sprite_{file}.png");
    }

    private void GenerateOnDeck<T>(PokeTradeHub<T> hub) where T : PKM, new()
    {
        var ondeck = hub.Queues.Info.GetUserList(OnDeckFormat);
        ondeck = ondeck.Skip(OnDeckSkip).Take(OnDeckTake); // filter down
        File.WriteAllText("ondeck.txt", string.Join(OnDeckSeparator, ondeck));
    }

    private void GenerateOnDeck2<T>(PokeTradeHub<T> hub) where T : PKM, new()
    {
        var ondeck = hub.Queues.Info.GetUserList(OnDeckFormat2);
        ondeck = ondeck.Skip(OnDeckSkip2).Take(OnDeckTake2); // filter down
        File.WriteAllText("ondeck2.txt", string.Join(OnDeckSeparator2, ondeck));
    }

    private void GenerateUserList<T>(PokeTradeHub<T> hub) where T : PKM, new()
    {
        var users = hub.Queues.Info.GetUserList(UserListFormat);
        users = users.Skip(UserListSkip);
        if (UserListTake > 0)
            users = users.Take(UserListTake); // filter down
        File.WriteAllText("users.txt", string.Join(UserListSeparator, users));
    }

    private void GenerateCompletedTrades<T>(PokeTradeHub<T> hub) where T : PKM, new()
    {
        var msg = string.Format(CompletedTradesFormat, hub.Config.Trade.CompletedTrades);
        File.WriteAllText("completed.txt", msg);
    }
}
