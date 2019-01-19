using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using ContosoHelpdeskChatBot.Models;
using Bot.Builder.Community.Dialogs.FormFlow;
using System.Threading;

namespace ContosoHelpdeskChatBot.Dialogs
{
    [Serializable]
    public class LocalAdminDialog : ComponentDialog
    {
        private LocalAdmin admin = new LocalAdmin();

        public LocalAdminDialog()
        : base(nameof(LocalAdminDialog))
        {
            AddDialog(FormDialog.FromForm(this.BuildLocalAdminForm, FormOptions.PromptInStart));
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dialog = FindDialog(nameof(LocalAdminPrompt));
            return await outerDc.BeginDialogAsync(nameof(LocalAdminPrompt));            
        }

        public override Task<DialogTurnResult> ResumeDialogAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var db = new ContosoHelpdeskContext())
            {
                db.LocalAdmins.Add(admin);
                db.SaveChanges();
            }

            return Task.FromResult(new DialogTurnResult(DialogTurnStatus.Complete));
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

                    this.admin.MachineName = (string)value;
                    return result;
                })
                .Field(nameof(LocalAdminPrompt.AdminDuration),
                validate: async (state, value) =>
                {
                    var result = new ValidateResult { IsValid = true, Value = value };
                    //add validation here

                    this.admin.AdminDuration = Convert.ToInt32((long)value) as int?;
                    return result;
                })
                .Build();
        }
    }
}