using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ContosoHelpdeskBot
{
    public class BotAccessors
    {
        public BotAccessors(ConversationState conversationState)
        {
            this.ConversationState = conversationState;
        }
        
        public ConversationState ConversationState { get; }

        public IStatePropertyAccessor<BotDataBag> ConversationData { get; set; }
        public IStatePropertyAccessor<DialogState> DialogData { get; set; }
    }
}
