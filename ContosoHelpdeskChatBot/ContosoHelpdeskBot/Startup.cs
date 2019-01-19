using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContosoHelpdeskBot
{
    public class Startup
    {
        private bool _isProduction = false;
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;

            // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
            BotConfiguration botConfig = null;
            try
            {
                botConfig = BotConfiguration.Load(botFilePath ?? @".\contoso-help-desk.bot", secretKey);
            }
            catch
            {
                var msg = @"Error reading bot file. Please ensure you have valid botFilePath and botFileSecret set for your environment.
                    - You can find the botFilePath and botFileSecret in the Azure App Service application settings.
                    - If you are running this bot locally, consider adding a appsettings.json file with botFilePath and botFileSecret.
                    - See https://aka.ms/about-bot-file to learn more about .bot file its use and bot configuration.
                    ";
                throw new InvalidOperationException(msg);
            }

            // Retrieve current endpoint.
            var endpointName = _isProduction ? "production" : "development";
            var service = botConfig.Services.Where(s => s.Type == "endpoint" && s.Name == endpointName).FirstOrDefault();
            if (!(service is EndpointService endpointService))
            {
                throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{endpointName}'.");
            }

            // Create credential provider and add as singleton so it is available in the controller
            var credentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);
            services.AddSingleton<ICredentialProvider>(credentialProvider);
            
            // The Memory Storage used here is for local bot debugging only. When the bot
            // is restarted, everything stored in memory will be gone.
            IStorage dataStore = new MemoryStorage();

            // Create Conversation State object.
            // The Conversation State object is where we persist anything at the conversation-scope.
            var conversationState = new ConversationState(dataStore);
            var userState = new UserState(dataStore);
            var privateConversationState = new PrivateConversationState(dataStore);

            // Create the custom state accessor.
            // State accessors enable other components to read and write individual properties of state.
            // This bot only uses dialog state.
            BotAccessors accessors = new BotAccessors(conversationState)
            {
                DialogData = conversationState.CreateProperty<DialogState>(nameof(DialogState)),
                ConversationData = conversationState.CreateProperty<BotDataBag>(nameof(BotDataBag))
            };

            services.AddSingleton(accessors);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                _isProduction = true;
            }

            app.UseMvc();
        }
    }
}
