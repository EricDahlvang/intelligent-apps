namespace ContosoHelpdeskChatBot.Dialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using ContosoHelpdeskChatBot.Models;
    using System.Threading;
    using Microsoft.Bot.Builder;

    public class InstallAppDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<BotDataBag> _conversationData;
        public InstallAppDialog(IStatePropertyAccessor<BotDataBag> conversationData)
        : base(nameof(InstallAppDialog))
        {
            _conversationData = conversationData;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await outerDc.PostAsync("Ok let's get started. What is the name of the application? ");
            //set default state values
            outerDc.ActiveDialog.State["AppName"] = string.Empty;
            var data = await _conversationData.GetAsync(outerDc.Context, () => new BotDataBag());
            data.Remove("AppList");
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }


        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            string appName = outerDc.ActiveDialog.State["AppName"] as string;

            if (string.IsNullOrEmpty(appName))
            {
                //If appName is missing, the user is either choosing from an AppList, 
                //or answering the question "What is the name of the application"
                List<string> applist = null;
                var data = await _conversationData.GetAsync(outerDc.Context, () => new BotDataBag());
                data.TryGetValue("AppList", out applist);

                if (applist == null)
                {
                    await appNameAsync(outerDc);
                }
                else
                {
                    await multipleAppsAsync(outerDc);
                }
                //since we are waiting for a user's response, we return DialogTurnStatus.Waiting
                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }
            else
            {
                await machineNameAsync(outerDc);
                return new DialogTurnResult(DialogTurnStatus.Complete);
            }
        }

        //public async Task StartAsync(DialogContext context)
        //{
        //    await context.PostAsync("Ok let's get started. What is the name of the application? ");

        //    context.Wait(appNameAsync);
        //}

        private async Task appNameAsync(DialogContext context)
        {
            //this will trigger a wait for user's reply
            //in this case we are waiting for an app name which will be used as keyword to search the AppMsi table
            var message = context.Context.Activity;

            var appname = message.Text;
            var names = await this.getAppsAsync(appname);

            if (names.Count == 1)
            {
                string name = names.First();
                context.ActiveDialog.State["AppName"] = name;
                await context.PostAsync($"Found {name}. What is the name of the machine to install application?");
               // context.Wait(machineNameAsync);
            }
            else if (names.Count > 1)
            {
                string appnames = "";
                for (int i = 0; i < names.Count; i++)
                {
                    appnames += $"<br/>&nbsp;&nbsp;&nbsp;{i + 1}.&nbsp;" + names[i];
                }
                await context.PostAsync($"I found {names.Count()} applications.<br/> {appnames}<br/> Please reply 1 - {names.Count()} to indicate your choice.");

                //at a conversation scope, store state data in ConversationData
                var data = await _conversationData.GetAsync(context.Context, () => new BotDataBag());
                data.SetValue("AppList", names);
            }
            else
            {
                await context.PostAsync($"Sorry, I did not find any application with the name \"{appname}\".");
               // context.Done<object>(null);
            }
        }

        private async Task multipleAppsAsync(DialogContext context)
        {
            //this will trigger a wait for user's reply
            //here we ask the user which specific app to install when we found more than one
            var message = context.Context.Activity;

            int choice;
            var isNum = int.TryParse(message.Text, out choice);
            List<string> applist;

            var data = await _conversationData.GetAsync(context.Context, () => new BotDataBag());
            data.TryGetValue("AppList", out applist);

            if (isNum && choice <= applist.Count && choice > 0)
            {
                //minus becoz index zero base
                context.ActiveDialog.State["AppName"] = applist[choice - 1];
                await context.PostAsync($"What is the name of the machine to install?");
                //context.Wait(machineNameAsync);
            }
            else
            {
                await context.PostAsync($"Invalid response. Please reply 1 - {applist.Count()} to indicate your choice.");
                //context.Wait(multipleAppsAsync);
            }
        }

        private async Task machineNameAsync(DialogContext context)
        {
            //this will trigger a wait for user's reply
            //finally we ask for the machine name on which to install the app
            var message = context.Context.Activity;

            var machinename = message.Text;

            var install = new InstallApp()
            {
                AppName = context.ActiveDialog.State["AppName"] as string,
                MachineName = message.Text
            };

            //TODO: Save to database
            using (var db = new ContosoHelpdeskContext())
            {
                db.InstallApps.Add(install);
                db.SaveChanges();
            }

            await context.PostAsync($"Great, your request to install {install.AppName} on {install.MachineName} has been scheduled.");
           // context.Done<object>(null);
        }

        private async Task<List<string>> getAppsAsync(string Name)
        {
            //TODO: Add EF to lookup database
            var names = new List<string>();

            using (var db = new ContosoHelpdeskContext())
            {
                names = (from app in db.AppMsis
                         where app.AppName.ToLower().Contains(Name.ToLower())
                         select app.AppName).ToList();
            }

            return names;
        }
    }
}