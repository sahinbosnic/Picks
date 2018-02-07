using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Picks.Dal.DataAccess;
using Picks.Dal.Services;
using Newtonsoft.Json.Serialization;
using Picks.Dal.Configuration;
using Microsoft.AspNetCore.Routing;

namespace Picks.Web
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddMvc();
            services.AddSingleton<IConfiguration>(Configuration);

            services.Configure<ImageUploadConfiguration>(Configuration.GetSection("ImageUpload"));

            services.AddScoped<ImageService>();

            services
                .AddMvc()
                .AddJsonOptions(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
                .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddDistributedRedisCache(opt =>
            {
                opt.Configuration = Configuration.GetConnectionString("Redis");
                opt.InstanceName = "main_";
            });

            services.AddSession(opt =>
            {
                opt.Cookie.Name = "picks.io";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration _configuration)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSession();

            //RouteCollection.RouteExistingFiles = true;

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                //if(Convert.ToBoolean(_configuration["CDN:Active"]))
                //{
                //    routes.MapRoute(
                //    name: "files",
                //    template: "css/{file}");
                //}
            });
        }
    }
}
