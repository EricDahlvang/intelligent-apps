namespace ContosoHelpdeskChatBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class RootDialog : ComponentDialog
    {
        private readonly BotAccessors _botAccessors;
        public RootDialog(BotAccessors accessors)
            :base(nameof(RootDialog))
        {
            _botAccessors = accessors;

            AddDialog(new WaterfallDialog("choiceswaterfall", new WaterfallStep[]
               {
                    PromptForOptionsAsync,
                    ShowOptionDialog
               }));
            AddDialog(new InstallAppDialog(_botAccessors));
            AddDialog(new LocalAdminDialog());
            AddDialog(new ResetPasswordDialog());
            AddDialog(new ChoicePrompt("options", ValidateChoiceAsync));
        }

        private async Task<bool> ValidateChoiceAsync(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            if (!promptContext.Recognized.Succeeded)
            {
                await promptContext.Context.SendActivityAsync("I'm sorry, I do not understand. Please enter one of the options. (install, password, or admin)", cancellationToken: cancellationToken);
                return false;
            }
            return true;
        }

        private Task<DialogTurnResult> ShowOptionDialog(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            switch ((stepContext.Result as FoundChoice).Value)
            {
                case InstallAppOption:
                    return stepContext.BeginDialogAsync(nameof(InstallAppDialog));
                case ResetPasswordOption:
                    return stepContext.BeginDialogAsync(nameof(ResetPasswordDialog));
                case LocalAdminOption:
                    return stepContext.BeginDialogAsync(nameof(LocalAdminDialog));
            }

            throw new InvalidOperationException("invalid option");
        }

        private async Task<DialogTurnResult> PromptForOptionsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                           "options",
                           new PromptOptions()
                           {
                               Choices = HelpdeskOptions,
                               Prompt = MessageFactory.Text(GreetMessage),
                               RetryPrompt = MessageFactory.Text(ErrorMessage)

                           },
                           cancellationToken);
        }

        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string InstallAppOption = "Install Application (install)";
        private const string ResetPasswordOption = "Reset Password (password)";
        private const string LocalAdminOption = "Request Local Admin (admin)";
        private const string GreetMessage = "Welcome to **Contoso Helpdesk Chat Bot**.\n\nI am designed to use with mobile email app, make sure your replies do not contain signatures. \n\nFollowing is what I can help you with, just reply with word in parenthesis:";
        private const string ErrorMessage = "Not a valid option";
        private static List<Choice> HelpdeskOptions = new List<Choice>()
        {
            new Choice(InstallAppOption) { Synonyms = new List<string>(){ "1", "install" } },
            new Choice(ResetPasswordOption) { Synonyms = new List<string>(){ "2", "password" } },
            new Choice(LocalAdminOption)  { Synonyms = new List<string>(){ "3", "admin" } }
        };

        //public async Task StartAsync(DialogContext context)
        //{
        //    context.Wait(this.MessageReceivedAsync);
        //}

        //public virtual async Task MessageReceivedAsync(DialogContext context, IAwaitable<IMessageActivity> userReply)
        //{
        //    var message = await userReply;

        //    this.ShowOptions(context);
        //}

        //private void ShowOptions(DialogContext context)
        //{
        //    PromptDialog.Choice(context, this.OnOptionSelected, HelpdeskOptions, GreetMessage, ErrorMessage, 3, PromptStyle.PerLine);
        //}

        //private async Task OnOptionSelected(DialogContext context, IAwaitable<string> userReply)
        //{
        //    try
        //    {
        //        string optionSelected = await userReply;

        //        switch (optionSelected)
        //        {
        //            case InstallAppOption:
        //                context.Call(new InstallAppDialog(), this.ResumeAfterOptionDialog);
        //                break;
        //            case ResetPasswordOption:
        //                context.Call(new ResetPasswordDialog(), this.ResumeAfterOptionDialog);
        //                break;
        //            case LocalAdminOption:
        //                context.Call(new LocalAdminDialog(), this.ResumeAfterOptionDialog);
        //                break;
        //        }
        //    }
        //    catch (TooManyAttemptsException ex)
        //    {
        //        await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

        //        context.Wait(this.MessageReceivedAsync);
        //    }
        //}

    //    private async Task ResumeAfterOptionDialog(DialogContext context, IAwaitable<object> userReply)
    //    {
    //        try
    //        {
    //            var message = await userReply;

    //            var ticketNumber = new Random().Next(0, 20000);
    //            await context.PostAsync($"Thank you for using the Helpdesk Bot. Your ticket number is {ticketNumber}.");
    //            context.Done(ticketNumber);
    //        }
    //        catch (Exception ex)
    //        {
    //            await context.PostAsync($"Failed with message: {ex.Message}");

    //            // In general resume from task after calling a child dialog is a good place to handle exceptions
    //            // try catch will capture exceptions from the bot framework awaitable object which is essentially "userReply"
    //            logger.Error(ex);
    //        }
    //    }
    }
}