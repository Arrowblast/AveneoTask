using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alachisoft.NCache.Caching.Distributed;
using AveneoTask.Database;
using AveneoTask.Security;
using AveneoTask.ServiceLayer.ClientFactory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AveneoTask
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
            /*services.Configure<ServicePointsOptions>(Configuration.GetSection(
                                        ServicePointsOptions.ServicePoints));*/
            services.AddResponseCaching();
            services.AddMemoryCache();
            services.AddTransient<MySQLDB>(_ => new MySQLDB(Configuration["ConnectionStrings:DefaultConnection"]));
            services.AddNCacheDistributedCache
 (Configuration.GetSection("NCacheSettings"));
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseResponseCaching();
            
        }
    }
}
