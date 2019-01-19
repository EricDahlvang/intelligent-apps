using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using ContosoHelpdeskChatBot.Models;
using Bot.Builder.Community.Dialogs.FormFlow;
using System.Threading;

namespace ContosoHelpdeskChatBot.Dialogs
{
    public class LocalAdminDialog : ComponentDialog
    {
        public LocalAdminDialog()
        : base(nameof(LocalAdminDialog))
        {
            AddDialog(FormDialog.FromForm(this.BuildLocalAdminForm, FormOptions.PromptInStart));
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dialog = FindDialog(nameof(LocalAdminPrompt));
            return await dialog.BeginDialogAsync(outerDc);        
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dialog = FindDialog(nameof(LocalAdminPrompt));
            var dialogResult = await dialog.ContinueDialogAsync(outerDc, cancellationToken).ConfigureAwait(false);

            if(dialogResult.Status == DialogTurnStatus.Complete)
            {
                var result = dialogResult.Result as LocalAdminPrompt;
                LocalAdmin admin = new LocalAdmin()
                {
                    AdminDuration = result.AdminDuration,
                    MachineName = result.MachineName
                };

                using (var db = new ContosoHelpdeskContext())
                {
                    db.LocalAdmins.Add(admin);
                    db.SaveChanges();
                }
            }
            return dialogResult;
        }


        private IForm<LocalAdminPrompt> BuildLocalAdminForm()
        {
            //here's an example of how validation can be used in form builder
            return new FormBuilder<LocalAdminPrompt>()
                .Field(nameof(LocalAdminPrompt.MachineName),
                validate: async (state, value) =>
                {
                    var result = new ValidateResult { IsValid = true, Value = value };
                    //add validation here                    
                    return result;
                })
                .Field(nameof(LocalAdminPrompt.AdminDuration),
                validate: async (state, value) =>
                {
                    var result = new ValidateResult { IsValid = true, Value = value };
                    //add validation here                    
                    return result;
                })
                .Message($"Thank you for using the Helpdesk Bot. Your ticket number is {new Random().Next(0, 20000)}.")
                .Build();
        }
    }
}