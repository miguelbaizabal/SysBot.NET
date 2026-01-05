using System.ComponentModel;
using System.IO;

namespace SysBot.Pokemon;

public class FolderSettings : IDumper
{
    private const string FeatureToggle = nameof(FeatureToggle);
    private const string Files = nameof(Files);
    public override string ToString() => "Configuración de Folder / Dumping";

    [Category(FeatureToggle), Description("Cuando está habilitado, dumpeará cualquier archivo PKM recibido (resultados de intercambio) a la carpeta DumpFolder.")]
    public bool Dump { get; set; }

    [Category(Files), Description("Source folder: donde se seleccionan los archivos PKM para distribuir.")]
    public string DistributeFolder { get; set; } = string.Empty;

    [Category(Files), Description("Destination folder: donde se dumpean todos los archivos PKM recibidos.")]
    public string DumpFolder { get; set; } = string.Empty;

    public void CreateDefaults(string path)
    {
        var dump = Path.Combine(path, "dump");
        Directory.CreateDirectory(dump);
        DumpFolder = dump;
        Dump = true;

        var distribute = Path.Combine(path, "distribute");
        Directory.CreateDirectory(distribute);
        DistributeFolder = distribute;
    }
}
