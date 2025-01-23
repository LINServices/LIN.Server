using System.Text.RegularExpressions;

namespace LIN.Inventory.Services.Reportes;

public abstract class BaseReport
{

    /// <summary>
    /// Contenido html del reporte.
    /// </summary>
    public string Html { get; set; } = string.Empty;


    /// <summary>
    /// Convertir el reporte html a PDF.
    /// </summary>
    /// <returns>Retorna la ruta publica del archivo pdf en LIN Cloud.</returns>
    public string ToPdf()
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// Comprimir el html.
    /// </summary>
    protected void CompressHtml()
    {
        if (string.IsNullOrWhiteSpace(Html))
            return;

        // Eliminar comentarios
        string result = Regex.Replace(Html, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);

        // Eliminar espacios en blanco entre etiquetas
        result = Regex.Replace(result, @">\s+<", "><");

        // Eliminar espacios en blanco al inicio y al final
        result = Regex.Replace(result, @"^\s+|\s+$", string.Empty);

        // Eliminar líneas en blanco adicionales
        result = Regex.Replace(result, @"\s{2,}", " ");

        Html = result;
    }

}