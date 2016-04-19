using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Restaurante.Startup))]
namespace Restaurante
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
