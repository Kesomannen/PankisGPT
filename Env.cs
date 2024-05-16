namespace PankisGPT;

public static class Env {
    public static string OpenAIKey { get; }
    public static string DiscordToken { get; }
    
    static Env() {
        DotNetEnv.Env.TraversePath().Load();
        OpenAIKey = DotNetEnv.Env.GetString("OPENAI_API_KEY");
        DiscordToken = DotNetEnv.Env.GetString("DISCORD_TOKEN");
    }
}