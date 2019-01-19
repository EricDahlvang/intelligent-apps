using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace ContosoHelpdeskBot
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
