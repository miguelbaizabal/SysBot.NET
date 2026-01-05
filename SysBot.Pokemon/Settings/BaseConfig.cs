using System.ComponentModel;

namespace SysBot.Pokemon;

/// <summary>
/// Console agnostic settings
/// </summary>
public abstract class BaseConfig
{
    protected const string FeatureToggle = nameof(FeatureToggle);
    protected const string Operation = nameof(Operation);
    private const string Debug = nameof(Debug);

    [Category(FeatureToggle), Description("Cuando está habilitado, el bot presionará el botón B ocasionalmente cuando no esté procesando nada (para evitar que se duerma).")]
    public bool AntiIdle { get; set; }

    [Category(FeatureToggle), Description("Habilita los registros de texto. Reinicie para aplicar los cambios.")]
    public bool LoggingEnabled { get; set; } = true;

    [Category(FeatureToggle), Description("Número máximo de archivos de registro antiguos a conservar. Establezca este valor en <= 0 para deshabilitar la limpieza de registros. Reinicie para aplicar los cambios.")]
    public int MaxArchiveFiles { get; set; } = 14;

    [Category(Debug), Description("Omite la creación de bots cuando se inicia el programa; útil para probar integraciones.")]
    public bool SkipConsoleBotCreation { get; set; }

    [Category(Operation)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public LegalitySettings Legality { get; set; } = new();

    [Category(Operation)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public FolderSettings Folder { get; set; } = new();

    public abstract bool Shuffled { get; }
}
