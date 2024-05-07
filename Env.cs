namespace PankisGPT;

public static class Env {
    public static string OpenAIKey { get; }
    public static string DiscordToken { get; }
    
    static Env() {
        var variables = Environment.GetEnvironmentVariables();
        
        Console.WriteLine("Environment variables:");
        foreach (var variable in variables) {
            Console.WriteLine(variable);
        }
        
        OpenAIKey = variables["OPENAI_API_KEY"] as string ?? throw new Exception("OPENAI_API_KEY is not set");
        DiscordToken = variables["DISCORD_TOKEN"] as string ?? throw new Exception("DISCORD_TOKEN is not set");
    }
}