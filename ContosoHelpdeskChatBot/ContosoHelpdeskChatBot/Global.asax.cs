using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Azure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using System.Reflection;
using System.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Builder;

namespace ContosoHelpdeskChatBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            GlobalConfiguration.Configure(BotConfig.Register);

            log4net.Config.XmlConfigurator.Configure();
        }

        //setting Bot data store policy to use last write win
        //example if bot service got restarted, existing conversation would just overwrite data to store
        public static class BotConfig
        {

            public static void Register(HttpConfiguration config)
            {
                BotAccessors accessors = null;

                var builder = new ContainerBuilder();
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

                var credentialProvider = new SimpleCredentialProvider(ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppIdKey],
                                                                    ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppPasswordKey]);
                builder.RegisterInstance(credentialProvider).As<ICredentialProvider>();

                // The Memory Storage used here is for local bot debugging only. When the bot
                // is restarted, everything stored in memory will be gone.
                IStorage dataStore = new MemoryStorage();

                // Create Conversation State object.
                // The Conversation State object is where we persist anything at the conversation-scope.
                var conversationState = new ConversationState(dataStore);
             //   var userState = new UserState(dataStore);
             //   var privateConversationState = new PrivateConversationState(dataStore);

                // Create the custom state accessor.
                // State accessors enable other components to read and write individual properties of state.
                accessors = new BotAccessors(conversationState)
                {
                //    UserData = userState.CreateProperty<BotDataBag>(BotAccessors.UserDataPropertyName),
                    ConversationData = conversationState.CreateProperty<BotDataBag>(BotAccessors.ConversationDataPropertyName),
               //     PrivateConversationData = privateConversationState.CreateProperty<BotDataBag>(BotAccessors.PrivateConversationDataPropertyName),
                    DialogData = conversationState.CreateProperty<DialogState>(nameof(DialogState))
                };

                builder.RegisterInstance(accessors).As<BotAccessors>();
                var container = builder.Build();

                var resolver = new AutofacWebApiDependencyResolver(container);
                config.DependencyResolver = resolver;
            }
        }
    }
}
