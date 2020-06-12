using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EH.TimeTrackNet.Web.Startup))]
namespace EH.TimeTrackNet.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
