﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ShareDataSiteNetCore
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.Configure<AzureADConfig>(config =>
            {
                config.Client_Id = "e0375f87-c47c-4180-9e20-ed3cebd53353";
                config.Client_Secret = "xvqmxVWR403=(crCZGQ93=!";
                config.Redirect_Uri = "https://localhost:44367/Authorize";
                config.Scope = "offline_access openid User.Read Files.Read.All Files.ReadWrite.All Sites.Read.All Sites.ReadWrite.All";
            });
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
            app.UseStaticFiles();
            app.UseMvc();
        }
    }

    public class AzureADConfig
    {
        public string Auth_Url { get; set; } = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
        public string Token_Url { get; set; } = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
        public string Logout_Url { get; set; }
        public string Client_Id { get; set; }
        public string Client_Secret { get; set; }
        public string Response_Type { get; set; } = "code";
        public string Redirect_Uri { get; set; }
        public string Scope { get; set; }
    }
}
