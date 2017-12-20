﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CODE.Framework.Services.Server.AspNetCore.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace CODE.Framework.Services.Server.AspNetCore.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceHandler(config =>
            {
                config.Services.Clear();                

                config.Services.AddRange(new List<ServiceHandlerConfigurationInstance>
                {
                    new ServiceHandlerConfigurationInstance
                    {
                        //ServiceType = typeof(UserService), // Using an explicit Type (assembly reference comes in)
                        ServiceTypeName = "Sample.Services.Implementation.UserService",
                        AssemblyName = "Sample.Services.Implementation",
                        RouteBasePath = "/api/users",
                        JsonFormatMode = JsonFormatModes.CamelCase
                    },
                    new ServiceHandlerConfigurationInstance
                    {
                        ServiceTypeName = "Sample.Services.Implementation.CustomerService", // dynamically loaded type
                        AssemblyName = "Sample.Services.Implementation",  // framework needs to load assembly - might need .dll extension
                        RouteBasePath = "/api/customers",
                        JsonFormatMode = JsonFormatModes.ProperCase,
                        OnAuthorize = context =>
                        {
                            context.HttpContext.User = new ClaimsPrincipal(
                                new ClaimsIdentity(
                                    new Claim[] {
                                        new Claim("Permission", "CanViewPage"),
                                        new Claim(ClaimTypes.Role, "Administrator"),
                                        new Claim(ClaimTypes.NameIdentifier, "Rick")},
                                    "Basic"));

                            return Task.FromResult(true);
                        }
                    }
                });

                config.Cors.UseCorsPolicy = true;
                config.Cors.AllowedOrigins = "*";
            });                        
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ServiceHandlerConfiguration config)
        {            
            app.UseServiceHandler();            
        }
    }
}