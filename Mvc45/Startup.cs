using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Mvc45.Startup))]
namespace Mvc45
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
