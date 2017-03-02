using dotnetcore2.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;

namespace dotnetcore2
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase());


            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new SignalRContractResolver();

            var serializer = JsonSerializer.Create(settings);
            services.Add(new ServiceDescriptor(typeof(JsonSerializer),
                         provider => serializer,
                         ServiceLifetime.Transient));

            services.AddSignalR(options =>
            {
                options.Hubs.EnableDetailedErrors = true;
            });

            // Add framework services.
            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info() { Title = "TODO API", Version = "1.0", Contact = new Contact() { Name = "Wepsys SRL" }, Description = "test para linux" });
            }); ;

            services.AddSingleton<ITodoRepository, TodoRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
    

            app.UseMvc();
            app.UseWebSockets();
            app.UseSignalR();
        }
    }
}
