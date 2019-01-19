namespace ContosoHelpdeskChatBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using ContosoHelpdeskBot;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;

    [Serializable]
    public class RootDialog : ComponentDialog
    {
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
        BotAccessors _accessors;
        public RootDialog(BotAccessors accessors)
            : base(nameof(RootDialog))
        {
            _accessors = accessors;
            AddDialog(new WaterfallDialog("choiceswaterfall",  new WaterfallStep[]
                {
                    PromptForOptions,
                    ShowOptionDialog
                }));
            AddDialog(new InstallAppDialog(_accessors));
            AddDialog(new LocalAdminDialog());
            AddDialog(new ResetPasswordDialog());
            AddDialog(new ChoicePrompt("options", ValidateChoiceAsync));
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

        private async Task<DialogTurnResult> PromptForOptions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

        private async Task<bool> ValidateChoiceAsync(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            if (!promptContext.Recognized.Succeeded)
            {
                await promptContext.Context.SendActivityAsync("I'm sorry, I do not understand. Please enter one of the options. (install, password, or admin)", cancellationToken: cancellationToken);
                return false;
            }
            return true;
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ticketNumber = new Random().Next(0, 20000);
            await outerDc.PostAsync($"Thank you for using the Helpdesk Bot. Your ticket number is {ticketNumber}.");
            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        public override Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dialogState = (DialogState)instance.State["dialogs"];
            dialogState.DialogStack.Clear();
            return Task.CompletedTask;
        }
    }
}