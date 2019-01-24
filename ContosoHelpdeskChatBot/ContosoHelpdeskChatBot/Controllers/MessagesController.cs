using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using Microsoft.Bot.Connector.Authentication;
using ContosoHelpdeskChatBot.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration;
using System.Threading;
using Bot.Builder.Community.Dialogs.FormFlow;

namespace ContosoHelpdeskChatBot
{
    //[BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly BotAccessors _accessors;
        private readonly ICredentialProvider _credentialProvider;
        private readonly DialogSet _dialogs;

        public MessagesController(BotAccessors accessors, ICredentialProvider credentialProvider)
        {
            _accessors = accessors;
            _credentialProvider = credentialProvider;
            _dialogs = new DialogSet(accessors.DialogData);
            _dialogs.Add(new RootDialog(accessors));
        }

        protected IAdapterIntegration CreateAdapter()
        {
            var adapter = new BotFrameworkAdapter(_credentialProvider, middleware: new DataBagsMiddleware(_accessors));
            adapter.Use(new AutoSaveStateMiddleware(_accessors.BotStates));
            return adapter;
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            var botFrameworkAdapter = CreateAdapter();

            var invokeResponse = await botFrameworkAdapter.ProcessActivityAsync(
                Request.Headers.Authorization?.ToString(),
                activity,
                OnTurnAsync,
                default(CancellationToken));

            if (invokeResponse == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(invokeResponse.Status);
            }

        }


        protected async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {

                var dc = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
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
                    if (dialogResult != null || cancelled)
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