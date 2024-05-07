using OpenAI_API;
using OpenAI_API.Chat;

namespace PankisGPT;

public class Chat {
    readonly Conversation _conversation;
    static readonly OpenAIAPI _api = new(Env.OpenAIKey);

    public Chat(string systemMessage, Model model) {
        var modelString = model switch {
            Model.Gpt35Turbo => "gpt-3.5-turbo",
            Model.Gpt4Turbo => "gpt-4-turbo",
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
        };

        _conversation = _api.Chat.CreateConversation(new ChatRequest { Model = modelString });
        _conversation.AppendSystemMessage(systemMessage);
    }
    
    public async Task<string> Ask(string input) {
        _conversation.AppendUserInput(input);
        return await _conversation.GetResponseFromChatbotAsync();
    }

    public enum Model {
        Gpt35Turbo,
        Gpt4Turbo
    }
}