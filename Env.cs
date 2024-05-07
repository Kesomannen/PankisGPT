namespace PankisGPT;

public static class Env {
    public static string OpenAIKey { get; }
    public static string DiscordToken { get; }
    
    static Env() {
        OpenAIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.Machine) 
                    ?? throw new Exception("OPENAI_API_KEY is not set");
        
        DiscordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN", EnvironmentVariableTarget.Machine) 
                       ?? throw new Exception("DISCORD_TOKEN is not set");
    }
}