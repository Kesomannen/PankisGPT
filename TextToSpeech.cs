using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace PankisGPT;

public class TextToSpeech {
    readonly string _voice;
    
    const string Url = "https://api.openai.com/v1/audio/speech";

    public TextToSpeech(string voice) {
        _voice = voice;
    }
    
    public async Task<Stream> Convert(string text) {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Env.OpenAIKey);

        var json = new {
            input = text,
            model = "tts-1",
            voice = _voice
        };

        var content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        var response = await http.PostAsync(Url, content);
        
        return await response.Content.ReadAsStreamAsync();
    }
}