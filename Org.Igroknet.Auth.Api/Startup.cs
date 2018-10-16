using System;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.Igroknet.Auth.Data;
using Org.Igroknet.Auth.Domain;
using SQLite;
using StructureMap;

namespace Org.Igroknet.Auth.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
           
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            return ConfigureIoc(services);
        }

        public IServiceProvider ConfigureIoc(IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("SQLITE_DB");

            if (connectionString == null)
            {
                connectionString = "/usr/share/igronket_auth.db3";
            }

            var sqliteConnection = new SQLiteConnection(connectionString);

            var container = new Container();

            var host = Environment.GetEnvironmentVariable("SMTP_HOST");
            var port = Environment.GetEnvironmentVariable("SMTP_PORT");

            var login = Environment.GetEnvironmentVariable("SMTP_LOGIN");
            var pwd = Environment.GetEnvironmentVariable("SMTP_PASS");
            var smtpClient = new SmtpClient();
            if(!string.IsNullOrWhiteSpace(host) && ! string.IsNullOrWhiteSpace(port) && !string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(pwd) && int.TryParse(port,out int parsedPort))
            {
                smtpClient.Host = host;
                smtpClient.Port = parsedPort;
                smtpClient.Credentials = new NetworkCredential(login, pwd);
            }

            container.Configure(config =>
            {
                config.Scan(_ =>
                {
                    _.AssemblyContainingType<Startup>();
                    _.WithDefaultConventions();
                    _.AssemblyContainingType<IUserService>();
                });

                config.For(typeof(IUserService)).Use(typeof(UserService));

                config.For<SQLiteConnection>().Use(sqliteConnection);
                config.For<SmtpClient>().Use(smtpClient);

                config.Populate(services);
            });

            return container.GetInstance<IServiceProvider>();
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
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
