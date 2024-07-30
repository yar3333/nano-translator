using System.Text.Json;
using System.Text.Json.Nodes;

namespace NanoTranslator;

public static class GoogleTranslator
{    
    /// <param name="fromLang">"auto", "en", "ru"</param>
    /// <param name="toLang">"en", "ru"</param>
    /// <param name="text"></param>
    public static async Task<string> TranslateAsync(string fromLang, string toLang, string text)
    {
        var http = new HttpClient();

        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("client", "gtx");
        queryString.Add("dt", "t");         // t = return translated text
        queryString.Add("sl", fromLang);    // from language
        queryString.Add("tl", toLang);      // to language
        queryString.Add("q", text);

        var uri = new UriBuilder("https://translate.googleapis.com/translate_a/single")
        {
            Query = queryString.ToString()
        };

        var jsonStr = await http.GetStringAsync(uri.Uri);
        var jsonDoc = JsonSerializer.Deserialize<JsonArray>(jsonStr);

        return jsonDoc?[0]?[0]?[0]?.ToString() ?? "";
    }
}