using Microsoft.AspNetCore.Authentication.Negotiate;
using ServerFramework;
using WebServer.Web;

namespace WebServer
{
    class WebSetting
    {
        public bool IsDeviloop = false;
        public string hosting = "http://0.0.0.0:8080";
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            WebSetting setting = Server.Config.GetOrLoadConfig<WebSetting>();

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
               .AddNegotiate();
            builder.WebHost.UseUrls(setting.hosting);
            builder.Services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy.
                options.FallbackPolicy = options.DefaultPolicy;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (setting.IsDeviloop)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(args =>
            {
                args.AllowAnyHeader();
                args.AllowAnyMethod();
                args.AllowAnyOrigin();
            });

            app.UseHttpsRedirection();
            app.MapControllers();
            app.Run();
        }
    }
}
