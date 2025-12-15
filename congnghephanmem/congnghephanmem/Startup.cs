using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(congnghephanmem.Startup))]
namespace congnghephanmem
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // BẬT TÍNH NĂNG CHAT REAL-TIME
            app.MapSignalR();
        }
    }
}