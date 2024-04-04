namespace LIN.Inventory.Services;


public static class Developers
{

    /// <summary>
    /// Url del servicios
    /// </summary>
    private static string _baseUrl = string.Empty;



    /// <summary>
    /// Api key de LIN Services
    /// </summary>
    private static string _apikey = string.Empty;



    /// <summary>
    /// IA De nombres
    /// </summary>
    public static async Task<ReadOneResponse<Genders>> IAName(string name)
    {

        // Crear HttpClient
        using var httpClient = new HttpClient();

        // ApiServer de la solicitud GET
        string url = _baseUrl + "IA/Predict/Name";

        var json = JsonConvert.SerializeObject(name);

        // Crear HttpRequestMessage y agregar el encabezado
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json)
        };

        // Agregar header.
        request.Headers.Add("apikey", $"{_apikey}");

        try
        {

            HttpResponseMessage response = await httpClient.SendAsync(request);

            // Leer la respuesta como una cadena
            string responseBody = await response.Content.ReadAsStringAsync();

            // Convierte la respuesta
            var obj = JsonConvert.DeserializeObject<ReadOneResponse<Genders>>(responseBody);

            ServerLogger.LogError(obj?.Message ?? "NULL");
            return obj ?? new();

        }
        catch (Exception e)
        {
            ServerLogger.LogError("Error IA name" + e.Message);
        }

        return new();

    }



    /// <summary>
    /// IA Vision
    /// </summary>
    public static async Task<ReadOneResponse<ProductCategories>> IAVision(byte[] image)
    {

        // Crear HttpClient
        using var httpClient = new HttpClient();

        // ApiServer de la solicitud GET
        string url = _baseUrl + "IA/predict/Vision";

        // Crear HttpRequestMessage y agregar el encabezado
        httpClient.DefaultRequestHeaders.Add("apiKey", $"{_apikey}");

        // Serializa el objeto
        string json = JsonConvert.SerializeObject(image);

        // Contenido
        StringContent content = new(json, Encoding.UTF8, "application/json");

        try
        {

            // Hacer la solicitud GET
            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            // Leer la respuesta como una cadena
            string responseBody = await response.Content.ReadAsStringAsync();


            var obj = JsonConvert.DeserializeObject<ReadOneResponse<ProductCategories>>(responseBody);

            return obj ?? new();


        }
        catch (Exception e)
        {
            ServerLogger.LogError("Error IA Vision" + e.Message);
        }

        return new();

    }



    /// <summary>
    /// Establece la api key
    /// </summary>
    /// <param name="key">Key de LIN</param>
    public static void SetKey(string key) => _apikey = key;



    /// <summary>
    /// Establece la nueva URL
    /// </summary>
    /// <param name="url">URL Base</param>
    public static void SetUrl(string url) => _baseUrl = url;


}
