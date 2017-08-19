using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Api1.Startup))]

namespace Api1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
