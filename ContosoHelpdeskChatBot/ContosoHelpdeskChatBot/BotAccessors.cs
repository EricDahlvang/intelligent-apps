using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoHelpdeskChatBot
{
    public class BotAccessors
    {
        public const string UserDataPropertyName = "UserData";
        public const string ConversationDataPropertyName = "ConversationData";
        public const string PrivateConversationDataPropertyName = "PrivateConversationData";

        public BotAccessors(ConversationState conversationState)
        {
            this.ConversationState = conversationState;
         //   this.PrivateConversationState = privateConversationState;
        //    this.UserState = userState;

            BotStates = new BotState[] { ConversationState};
        }

        public BotState[] BotStates { get; }

        public ConversationState ConversationState { get; }
      //  public PrivateConversationState PrivateConversationState { get; }
      //  public UserState UserState { get; }

      //  public IStatePropertyAccessor<BotDataBag> UserData { get; set; }
        public IStatePropertyAccessor<BotDataBag> ConversationData { get; set; }
     //   public IStatePropertyAccessor<BotDataBag> PrivateConversationData { get; set; }
        public IStatePropertyAccessor<DialogState> DialogData { get; set; }
    }

    public class BotDataBag : Dictionary<string, object>, IBotDataBag
    {
        public bool RemoveValue(string key)
        {
            return base.Remove(key);
        }

        public void SetValue<T>(string key, T value)
        {
            this[key] = value;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            if (!ContainsKey(key))
            {
                value = default(T);
                return false;
            }

            value = (T)this[key];

            return true;
        }
    }

    public interface IBotDataBag
    {
        int Count { get; }

        void Clear();

        bool ContainsKey(string key);

        bool RemoveValue(string key);

        void SetValue<T>(string key, T value);

        bool TryGetValue<T>(string key, out T value);
    }
}