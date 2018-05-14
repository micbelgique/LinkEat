using LinkEat.Models;
using LinkEat.Repositories;
using LinkEat.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace LinkEat
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            //AppSettings used for migrations
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddSingleton(_ => Configuration);

            var credentialProvider = new SimpleCredentialProvider(Configuration["BotConnectionInfo:AppId"], Configuration["BotConnectionInfo:AppPassword"]);

            services.AddAuthentication()
                .AddBotAuthentication(credentialProvider);

            services.AddSingleton(typeof(ICredentialProvider), credentialProvider);
         
            services.AddScoped<MealRepository>();
            services.AddScoped<OrderRepository>();
            services.AddScoped<PlaceRepository>();
            services.AddScoped<AppointmentRepository>();
            services.AddScoped<UserRepository>();

            TwilioClient.Init(Configuration["Twilio:AccountSID"], Configuration["Twilio:AuthToken"]);
            services.AddSingleton<EventsOnGivenTime, EventsOnGivenTime>();

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(TrustServiceUrlAttribute));
            }).AddXmlSerializerFormatters();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            } else {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
