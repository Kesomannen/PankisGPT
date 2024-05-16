using OpenAI_API;
using OpenAI_API.Chat;

namespace PankisGPT;

public class Chat {
    Conversation _conversation;
    readonly string _systemMessage;
    readonly string _modelString;
    static readonly OpenAIAPI _api = new(Env.Get("OPENAI_API_KEY"));

    public Chat(string systemMessage, Model model) {
        _systemMessage = systemMessage;
        _modelString = model switch {
            Model.Gpt35Turbo => "gpt-3.5-turbo",
            Model.Gpt4Turbo => "gpt-4-turbo",
            Model.Gpt4o => "gpt-4o",
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
        };
        
        Reset();
    }
    
    public async Task<string> Ask(string input) {
        _conversation.AppendUserInput(input);
        return await _conversation.GetResponseFromChatbotAsync();
    }

    public enum Model {
        Gpt35Turbo,
        Gpt4Turbo,
        Gpt4o
    }

    public void Reset() {
        _conversation = _api.Chat.CreateConversation(new ChatRequest { Model = _modelString });
        _conversation.AppendSystemMessage(_systemMessage);
    }
}