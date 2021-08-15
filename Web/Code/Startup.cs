using Owin;
using Microsoft.Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Diagnostics;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(KjeringiData.Startup))]

namespace KjeringiData
{

    public class ErrorHandlingPipelineModule : HubPipelineModule
    {
        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            Debug.WriteLine("=> Exception " + exceptionContext.Error.Message);
            if (exceptionContext.Error.InnerException != null)
            {
                Debug.WriteLine("=> Inner Exception " + exceptionContext.Error.InnerException.Message);
            }
            base.OnIncomingError(exceptionContext, invokerContext);

        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();

            // Any connection or hub wire up and configuration should go here
            GlobalHost.HubPipeline.AddModule(new ErrorHandlingPipelineModule()); 

            //HubConfiguration hubConfig = new HubConfiguration();
            //hubConfig.EnableJSONP = true;
            //hubConfig.EnableDetailedErrors = true;

            //var task = Task.Run(() => app.MapSignalR(hubConfig));
            //task.Wait(300);
            ////try again if it fails just to be sure ;)
            //if (task.IsCanceled) Task.Run(() => app.MapSignalR(hubConfig)).Wait(300);            
        }
    }
}