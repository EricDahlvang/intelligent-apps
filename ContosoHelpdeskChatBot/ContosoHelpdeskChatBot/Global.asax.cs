using System.Web.Http;
using System.Reflection;
using System.Configuration;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;

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
        
        public static class BotConfig
        {
            public static void Register(HttpConfiguration config)
            {
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
                builder.RegisterInstance(conversationState).As<ConversationState>();

                //var userState = new UserState(dataStore);
                //var privateConversationState = new PrivateConversationState(dataStore);

                // Create the custom state accessors.
                // State accessors enable other components to read and write individual properties of state.
                IStatePropertyAccessor<BotDataBag> dataBagAccessor = conversationState.CreateProperty<BotDataBag>(nameof(BotDataBag));
                IStatePropertyAccessor<DialogState> dialogStateAccessor = conversationState.CreateProperty<DialogState>(nameof(DialogState));

                builder.RegisterInstance(dialogStateAccessor).As<IStatePropertyAccessor<DialogState>>();
                builder.RegisterInstance(dataBagAccessor).As<IStatePropertyAccessor<BotDataBag>>();

                var container = builder.Build();
                var resolver = new AutofacWebApiDependencyResolver(container);
                config.DependencyResolver = resolver;
            }
        }
    }
}
