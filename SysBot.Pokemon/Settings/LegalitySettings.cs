using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SysBot.Pokemon;

public class LegalitySettings
{
    private string DefaultTrainerName = "SysBot";
    private const string Generate = nameof(Generate);
    private const string Misc = nameof(Misc);
    public override string ToString() => "Configuración de Generación de Legalidad";

    // Generate
    [Category(Generate), Description("Directorio MGDB para las Wonder Cards.")]
    public string MGDBPath { get; set; } = string.Empty;

    [Category(Generate), Description("Directorio para archivos PKM con datos de entrenador a usar para archivos PKM regenerados.")]
    public string GeneratePathTrainerInfo { get; set; } = string.Empty;

    [Category(Generate), Description("Nombre predeterminado del entrenador original para archivos PKM que no coincidan con ninguno de los archivos proporcionados.")]
    public string GenerateOT
    {
        get => DefaultTrainerName;
        set
        {
            if (!StringsUtil.IsSpammyString(value))
                DefaultTrainerName = value;
        }
    }

    [Category(Generate), Description("ID de Entrenador 16-bit predeterminado (TID) para solicitudes que no coincidan con ninguno de los archivos de datos de entrenador proporcionados. Esto debe ser un número de 5 dígitos.")]
    public ushort GenerateTID16 { get; set; } = 12345;

    [Category(Generate), Description("ID Secreto 16-bit predeterminado (SID) para solicitudes que no coincidan con ninguno de los archivos de datos de entrenador proporcionados. Esto debe ser un número de 5 dígitos.")]
    public ushort GenerateSID16 { get; set; } = 54321;

    [Category(Generate), Description("Idioma predeterminado para archivos PKM que no coincidan con ninguno de los archivos proporcionados.")]
    public LanguageID GenerateLanguage { get; set; } = LanguageID.English;

    [Category(Generate), Description("Método de búsqueda de encuentros al generar Pokémon. \"NativeOnly\" busca solo en la pareja de juegos actuales, \"NewestFirst\" busca desde el juego más reciente, y \"PriorityOrder\" usa el orden designado en la configuración \"GameVersionPriority\".")]
    public GameVersionPriorityType GameVersionPriority { get; set; } = GameVersionPriorityType.NativeOnly;

    [Category(Generate), Description("Especifica el orden de juegos a usar para generar encuentros. Establece PrioritizeGame en \"true\" para habilitar.")]
    public List<GameVersion> PriorityOrder { get; set; } = Enum.GetValues<GameVersion>().Where(GameUtil.IsValidSavedVersion).Reverse().ToList();

    [Category(Generate), Description("Establece todas las cintas legales posibles para cualquier Pokémon generado.")]
    public bool SetAllLegalRibbons { get; set; }

    [Category(Generate), Description("Establece una ball coincidente (basada en color) para cualquier Pokémon generado.")]
    public bool SetMatchingBalls { get; set; } = true;

    [Category(Generate), Description("Forzar la ball especificada si es legal.")]
    public bool ForceSpecifiedBall { get; set; } = true;

    [Category(Generate), Description("Asume que los conjuntos de nivel 50 son conjuntos competitivos de nivel 100.")]
    public bool ForceLevel100for50 { get; set; }

    [Category(Generate), Description("Requiere el rastreador HOME al intercambiar Pokémon que tuvieron que haber viajado entre los juegos de Switch.")]
    public bool EnableHOMETrackerCheck { get; set; }

    [Category(Generate), Description("El orden en el que se intentan los tipos de encuentros de Pokémon.")]
    public List<EncounterTypeGroup> PrioritizeEncounters { get; set; } =
    [
        EncounterTypeGroup.Egg, EncounterTypeGroup.Slot,
        EncounterTypeGroup.Static, EncounterTypeGroup.Mystery,
        EncounterTypeGroup.Trade,
    ];

    [Category(Generate), Description("Agrega la Versión de Batalla para juegos que lo soportan (SWSH solamente) para usar Pokémon de generaciones anteriores en juegos competitivos online.")]
    public bool SetBattleVersion { get; set; }

    [Category(Generate), Description("El bot creará un Pokémon de Huevo de Pascua si se le proporciona un conjunto ilegal.")]
    public bool EnableEasterEggs { get; set; }

    [Category(Generate), Description("Permite a los usuarios enviar datos de entrenador personalizados (OT, TID, SID y género de OT) en conjuntos de Showdown.")]
    public bool AllowTrainerDataOverride { get; set; }

    [Category(Generate), Description("Permite a los usuarios enviar personalización adicional con comandos del Editor por Lotes.")]
    public bool AllowBatchCommands { get; set; }

    [Category(Generate), Description("Tiempo máximo en segundos para gastar al generar un conjunto antes de cancelar. Esto evita que los conjuntos difíciles congelen el bot.")]
    public int Timeout { get; set; } = 15;

    // Misc

    [Category(Misc), Description("Reestablece los rastreadores HOME para archivos PKM clonados y solicitados por el usuario. Se recomienda dejar esto deshabilitado para evitar crear datos HOME inválidos.")]
    public bool ResetHOMETracker { get; set; }
}
