using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Microsoft.Bot.Schema;
using Microsoft.Rest.Serialization;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using System.Text;
using Bot.Builder.Community.Dialogs.FormFlow;
using ContosoHelpdeskChatBot.Dialogs;

namespace ContosoHelpdeskBot.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        BotAccessors Accessors { get; set; }
        DialogSet Dialogs { get; set; }
        ICredentialProvider _credentialProvider;

        public MessagesController(BotAccessors accessors, ICredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
            this.Accessors = accessors;
            Dialogs = new DialogSet(accessors.DialogData);
            Dialogs.Add(new RootDialog(Accessors));
        }

        public static readonly JsonSerializer BotMessageSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new ReadOnlyJsonContractResolver(),
            Converters = new List<JsonConverter> { new Iso8601TimeSpanConverter() },
        });

        [HttpPost]
        public async Task PostAsync()
        {
            var activity = default(Activity);

            using (var bodyReader = new JsonTextReader(new StreamReader(Request.Body, Encoding.UTF8)))
            {
                activity = BotMessageSerializer.Deserialize<Activity>(bodyReader);
            }
            
            var botFrameworkAdapter = new BotFrameworkAdapter(_credentialProvider, middleware: new AutoSaveStateMiddleware(Accessors.ConversationState));
            
            var invokeResponse = await botFrameworkAdapter.ProcessActivityAsync(
                Request.Headers["Authorization"],
                activity,
                OnTurnAsync,
                default(CancellationToken));

            if (invokeResponse == null)
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                // Return the exception in the body of the response
                Response.ContentType = "application/json";
                Response.StatusCode = invokeResponse.Status;

                using (var writer = new StreamWriter(Response.Body))
                {
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        BotMessageSerializer.Serialize(jsonWriter, invokeResponse.Body);
                    }
                }
            }
        }

        private async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dc = await Dialogs.CreateContextAsync(turnContext, cancellationToken);
                try
                {
                    bool cancelled = false;
                    // Globally interrupt the dialog stack if the user sent 'cancel'
                    if (turnContext.Activity.Text.Equals("cancel", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var reply = turnContext.Activity.CreateReply($"Ok restarting conversation.");
                        await turnContext.SendActivityAsync(reply);
                        await dc.CancelAllDialogsAsync();
                        cancelled = true;
                    }

                    var dialogResult = await dc.ContinueDialogAsync();                    
                    if (!dc.Context.Responded || cancelled)
                    {
                        // examine results from active dialog
                        switch (dialogResult.Status)
                        {
                            case DialogTurnStatus.Empty:
                                await dc.BeginDialogAsync(nameof(RootDialog));
                                break;

                            case DialogTurnStatus.Waiting:
                                // The active dialog is waiting for a response from the user, so do nothing.
                                break;

                            case DialogTurnStatus.Complete:
                                await dc.EndDialogAsync();
                                break;

                            default:
                                await dc.CancelAllDialogsAsync();
                                break;
                        }
                    }
                }
                catch (FormCanceledException ex)
                {
                    await turnContext.SendActivityAsync("Cancelled.");
                    await dc.CancelAllDialogsAsync();
                    await dc.BeginDialogAsync(nameof(RootDialog));
                }
            }
        }
    }
}
