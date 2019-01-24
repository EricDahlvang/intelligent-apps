using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ContosoHelpdeskChatBot
{
    public static class ClassicExtensions
    {
        public static async Task PostAsync(this DialogContext dc, string message)
        {
            await dc.Context.SendActivityAsync(message).ConfigureAwait(false);
        }

        public static async Task PostAsync(this DialogContext dc, IMessageActivity message)
        {
            await dc.Context.SendActivityAsync(message).ConfigureAwait(false);
        }
    }
}