namespace PankisGPT;

public static class Env {
    static Env() {
        DotNetEnv.Env.TraversePath().Load();
    }
    
    public static string Get(string key) => DotNetEnv.Env.GetString(key);
}