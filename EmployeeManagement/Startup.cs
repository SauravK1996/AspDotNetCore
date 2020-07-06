using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement
{
    public class Startup
    {
        private IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(options =>
            options.UseSqlServer(_config.GetConnectionString("EmployeeDBConnection")));

            //services.AddIdentity<IdentityUser, IdentityRole>(options =>
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 3;

                options.SignIn.RequireConfirmedEmail = true;

                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

            }).AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<CustomEmailConfirmationTokenProvider
                <ApplicationUser>>("CustomEmailConfirmation");

            services.Configure<DataProtectionTokenProviderOptions>(o =>
                        o.TokenLifespan = TimeSpan.FromHours(5));

            services.Configure<CustomEmailConfirmationTokenProviderOptions>(o =>
                            o.TokenLifespan = TimeSpan.FromDays(3));

            //configure password by ovverriding the rules
            //services.Configure<IdentityOptions>(options => 
            //{
            //    options.Password.RequiredLength = 10;
            //    options.Password.RequiredUniqueChars = 3;
            //});
            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddMvc(options => {
                var policy = new AuthorizationPolicyBuilder()
                             .RequireAuthenticatedUser()
                             .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddXmlSerializerFormatters();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "1023636132785-t0tkirbs8t06bvjhkaluroutsnftedft.apps.googleusercontent.com";
                    options.ClientSecret = "xYxov98LWWwBRKm5ydY7pmVn";
                })
                .AddFacebook(options =>
                {
                    options.AppId = "3088518014604456";
                    options.AppSecret = "6bb3f731a698165e7099bb051304eca0";
                });
                 
          

            services.ConfigureApplicationCookie(options=> 
            {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });

            services.AddAuthorization(options=> 
            {
                options.AddPolicy("DeleteRolePolicy",
                    policy => policy.RequireClaim("Delete Role"));

                options.AddPolicy("EditRolePolicy",
                    policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirements()));

                //options.InvokeHandlersAfterFailure = false;

                options.AddPolicy("AdminRolePolicy",
                   policy => policy.RequireRole("Admin"));

                //RequireAssertion(context =>
                //context.User.IsInRole("Admin") &&
                //context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true")||
                //context.User.IsInRole("Super Admin")));
                //policy => policy.RequireClaim("Edit Role","true")
                //                .RequireRole("Admin")
                //                .RequireRole("Super Admin"));
            });

            //services.AddSingleton<IEmployeeRepository, MockEmplooyeeRepository>();
            //services.AddScoped<IEmployeeRepository, MockEmplooyeeRepository>();
            //services.AddTransient<IEmployeeRepository, MockEmplooyeeRepository>();

            services.AddSingleton<IMailHelper, MailHelper>();

            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();

            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();

            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();

            services.AddSingleton<DataProtectionPurposeStrings>();

            //services.AddMvcCore(options => options.EnableEndpointRouting = false);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)//,ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                //DeveloperExceptionPageOptions developerExceptionPageOptions = new DeveloperExceptionPageOptions
                //{
                //    SourceCodeLineCount = 1
                //};
                //app.UseDeveloperExceptionPage(developerExceptionPageOptions);
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                //app.UseStatusCodePagesWithRedirects("/Error/{0}");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }
            //else if(env.IsStaging() || env.IsProduction() || env.IsEnvironment("UAT"))
            //{
            //    app.UseExceptionHandler("/Error");
            //}

            //app.UseRouting();



            //app.Use(async (context,next) =>
            //{
            //    //await context.Response.WriteAsync("Hello world! from first middlware");
            //    logger.LogInformation("MW1: Incoming Request");
            //    await next();
            //    logger.LogInformation("MW1: Outgoing Response");
            //});

            //app.Use(async (context, next) =>
            //{
            //    //await context.Response.WriteAsync("Hello world! from first middlware");
            //    logger.LogInformation("MW2: Incoming Request");
            //    await next();
            //    logger.LogInformation("MW2: Outgoing Response");
            //});
            //DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            //defaultFilesOptions.DefaultFileNames.Clear();
            //defaultFilesOptions.DefaultFileNames.Add("foo.html");

            //app.UseDefaultFiles(defaultFilesOptions);
            //app.UseMvcWithDefaultRoute();

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            //app.UseMvc();
            //FileServerOptions fileServerOptions = new FileServerOptions();
            //fileServerOptions.DefaultFilesOptions.DefaultFileNames.Clear();
            //fileServerOptions.DefaultFilesOptions.DefaultFileNames.Add("foo.html");
            //app.UseFileServer(fileServerOptions);
            //app.UseFileServer();

            //app.Run(async (context) =>
            //{
            //    //throw new Exception("Some exception occurs");

            //    //await context.Response.WriteAsync("Hosting Environment : "+env.EnvironmentName);
            //    await context.Response.WriteAsync("Hello World!");


            //    //await context.Response.WriteAsync("MW3: Request handled and response produced");
            //    //logger.LogInformation("MW3: Request handled and response produced");
            //});

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        //await context.Response.WriteAsync(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            //        //await context.Response.WriteAsync(_config["MyKey"]);
            //        await context.Response.WriteAsync("Hello World!");
            //    });
            //});
        }
    }
}
