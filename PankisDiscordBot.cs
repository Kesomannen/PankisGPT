using System.Diagnostics;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;

namespace PankisGPT;

public delegate bool MessageFilter(SocketMessage message, PankisDiscordBot bot);

public class PankisDiscordBot {
    readonly DiscordSocketClient _client;
    readonly TextToSpeech _tts;
    readonly Chat _chat;
    readonly MessageFilter _messageFilter;
    
    public SocketSelfUser User => _client.CurrentUser;

    public event Func<LogMessage, Task>? OnLog;
    
    PankisDiscordBot(DiscordSocketClient client, TextToSpeech tts, Chat chat, MessageFilter messageFilter) {
        _client = client;
        _tts = tts;
        _chat = chat;
        _messageFilter = messageFilter;

        _client.Log += message => {
            OnLog?.Invoke(message);
            return Task.CompletedTask;
        };
        
        _client.MessageReceived += OnMessageReceived;
        _client.UserJoined += OnUserJoined;
        _client.Ready += SetupCommands;

        _ = Task.Run(ActivityUpdateLoop);
    }

    async Task SetupCommands() {
        _client.SlashCommandExecuted += CaramellDansen;
        
        var guild = _client.GetGuild(1090980892115214418);

        var command = new SlashCommandBuilder()
            .WithName("caramelldansen")
            .WithDescription("Gör som vi gör, ta några steg åt vänster")
            .Build();
        
        await guild!.CreateApplicationCommandAsync(command);
    }

    async Task CaramellDansen(SocketSlashCommand command) {
        _ = Task.Run(async () => {
            var channel = (command.User as IGuildUser)?.VoiceChannel;

            if (channel == null) {
                await command.RespondAsync("Du måste vara i en röstkanal för att använda detta kommando");
                return;
            }

            await command.RespondAsync("Nu kör vi!!! 🎉");

            try {
                using var client = await channel.ConnectAsync();

                using var ffmpeg = Process.Start(new ProcessStartInfo {
                    FileName = "ffmpeg",
                    Arguments = "-hide_banner -loglevel panic -i \"caramelldansen.mp3\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                });

                await using var output = ffmpeg!.StandardOutput.BaseStream;
                await using var discord = client.CreatePCMStream(AudioApplication.Mixed);

                try {
                    await output.CopyToAsync(discord);
                } finally {
                    await discord.FlushAsync();
                }
            }
            catch (Exception e) {
                await command.RespondAsync($"Kunde inte spela caramelldansen 😭: {e.Message}");
            }
        });
    }

    void ActivityUpdateLoop() {
        while (true) {
            var day = DateTime.Today.DayOfWeek;
            var time = DateTime.Now.TimeOfDay;
            
            var lunchStart = new TimeSpan(11, 20, 0);
            var lunchEnd = new TimeSpan(12, 10, 0);
            
            if (day == DayOfWeek.Thursday && time >= lunchStart && time <= lunchEnd) {
                _client.SetCustomStatusAsync("Sörjer pannkakstorsdag 🥞😢");
            } else {
                var nextThursday = DateTime.Today.AddDays((int) DayOfWeek.Thursday - (int) DateTime.Today.DayOfWeek);
                var nextLunch = nextThursday.Add(lunchStart);
                var remaining = (nextLunch - DateTime.Now).ToString(@"d\d\ h\h\ m\m");
                
                _client.SetCustomStatusAsync($"Väntar på pannkakstorsdag ({remaining} kvar)");
            }
            
            Thread.Sleep(TimeSpan.FromMinutes(1));
        }
    }
    
    Task OnMessageReceived(SocketMessage message) {
        if (!_messageFilter(message, this)) return Task.CompletedTask;
        
        _ = Task.Run(async () => {
            try {
                using (message.Channel.EnterTypingState()) {
                    LogVerbose("Prompting ChatGPT");
                    var responseText = await _chat.Ask($"{message.Author.GlobalName}: {message.CleanContent}");
                    LogVerbose("Got response from ChatGPT");
                
                    await using var stream = await _tts.Convert(responseText);
                
                    await message.Channel.SendFileAsync(
                        new FileAttachment(stream, $"{string.Join(' ', responseText.Split(' ').Take(3))}.pcm"),
                        text: responseText,
                        messageReference: new MessageReference(message.Id)
                    );

                    if (responseText.Contains("sparka", StringComparison.CurrentCultureIgnoreCase)) {
                        await message.Author.SendMessageAsync("Passa dig... https://discord.gg/vRwa8BqpZ3");
                        var guildUser = message.Author as IGuildUser;
                        await guildUser!.KickAsync();
                    } else {
                        var channel = (message.Author as IGuildUser)?.VoiceChannel;

                        if (channel != null) {
                            using var client = await channel.ConnectAsync();
                            await using var discord = client.CreatePCMStream(AudioApplication.Mixed);

                            try {
                                await stream.CopyToAsync(discord);
                            } finally {
                                await discord.FlushAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                LogError($"Error processing message: {e.Message}");
                throw;
            }
        });
        
        return Task.CompletedTask;
    }

    static async Task OnUserJoined(SocketGuildUser user) {
        var role = user.Guild.GetRole(1090982368719941743);
        await user.AddRoleAsync(role);
    }

    public static async Task<PankisDiscordBot> Create(string token, TextToSpeech tts, Chat chat,  MessageFilter messageFilter) {
        var config = new DiscordSocketConfig {
            GatewayIntents = GatewayIntents.All
        };
        
        var client = new DiscordSocketClient(config);

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
        
        return new PankisDiscordBot(client, tts, chat, messageFilter);
    }
    
    void Log(object message, LogSeverity severity) {
        OnLog?.Invoke(new LogMessage(severity, "PankisDiscordBot", message.ToString()));
    }
    
    void LogDebug(object message) => Log(message, LogSeverity.Debug);
    void LogVerbose(object message) => Log(message, LogSeverity.Verbose);
    void LogInfo(object message) => Log(message, LogSeverity.Info);
    void LogWarning(object message) => Log(message, LogSeverity.Warning);
    void LogError(object message) => Log(message, LogSeverity.Error);
    void LogCritical(object message) => Log(message, LogSeverity.Critical);
}