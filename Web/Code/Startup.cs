using Owin;
using Microsoft.Owin;
using Microsoft.AspNet.SignalR;
[assembly: OwinStartup(typeof(SignalRChat.Startup))]
namespace SignalRChat
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            HubConfiguration hubConfig = new HubConfiguration();
            hubConfig.EnableJSONP = true;
            hubConfig.EnableDetailedErrors = true;
            app.MapSignalR(hubConfig);
        }
    }
}