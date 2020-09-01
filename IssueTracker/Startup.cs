using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IssueTracker.Models;
using IssueTracker.Security;
using IssueTracker.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IssueTracker
{
    public class Startup
    {
        
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(config =>
            {
                var globalPolicy = new AuthorizationPolicyBuilder()
                                       .RequireAuthenticatedUser()
                                       .Build();

                config.Filters.Add(new AuthorizeFilter(globalPolicy));
                config.EnableEndpointRouting = false;
            }).AddXmlSerializerFormatters();

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("ConnectionString")));

            services.AddScoped<IIssueRepository, SQLIssueRepository>();
            services.AddScoped<IProjectRepository, SQLProjectRepository>();
            services.AddScoped<IFirebaseStorage, FirebaseFileStorage>();

            services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 10;
                config.Password.RequiredUniqueChars = 3;
                config.Password.RequireNonAlphanumeric = true;
                config.SignIn.RequireConfirmedAccount = false;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("UserPolicy",
                    policy => policy.AddRequirements(new UserClaimsRequirement()));
                options.AddPolicy("DeveloperPolicy",
                   policy => policy.AddRequirements(new DeveloperClaimsRequirement()));
                options.AddPolicy("ManagerPolicy",
                   policy => policy.AddRequirements(new ManagerClaimsRequirement()));
                options.AddPolicy("AdminPolicy",
                   policy => policy.AddRequirements(new AdminClaimsRequirement()));
            });

            services.AddTransient<IAuthorizationHandler, UserLevel>();
            services.AddTransient<IAuthorizationHandler, DeveloperLevel>();
            services.AddTransient<IAuthorizationHandler, ManagerLevel>();
            services.AddTransient<IAuthorizationHandler, AdminLevel>();

            services.Configure<FirebaseSettings>(Configuration.GetSection("FirebaseStorage"));

            services.AddOptions();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
