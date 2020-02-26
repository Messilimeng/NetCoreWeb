using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using Controllers.Attribute;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Hosting;

namespace NetCoreWeb
{
    /// <summary>
    /// ASP.NET Core 应用使用 Startup 类，按照约定命名为 Startup。 Startup 类：
    /// 可选择性地包括 ConfigureServices 方法以配置应用的服务。
    /// 必须包括 Configure 方法以创建应用的请求处理管道。
    /// 当应用启动时，运行时调用 ConfigureServices 和 Configure：
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public static IContainer AutofacContainer;

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //注册服务进 IServiceCollection

            #region Session store

            // services.AddDistributedMemoryCache();

            var redis = ConnectionMultiplexer.Connect(Configuration.GetConnectionString("Redis"));
            services.AddDataProtection().PersistKeysToRedis(redis, "DataProtection-Test-Keys");

            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration.GetConnectionString("Redis");
                //option.InstanceName = "master";
            });

            services.AddSession(options =>
            {
                //options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // https
                options.IdleTimeout = TimeSpan.FromMinutes(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = "sessionid";
            });
            // services.AddSession(option => { option.IdleTimeout = TimeSpan.FromMinutes(3000); });
            #endregion

            #region Cookie store
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(UserAuthorizeAttribute.UserAuthenticationScheme, o =>
                {
                    o.Cookie.Path = "/";
                    o.Cookie.Name = "my.web.cookie";
                    o.LoginPath = "/WebSite/Login";
                    o.LogoutPath = "/WebSite/Logout";
                    o.Cookie.SecurePolicy = CookieSecurePolicy.None;
                });

            #endregion
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            ContainerBuilder builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterModule<DefaultModuleRegister>();
            AutofacContainer = builder.Build();
            return new AutofacServiceProvider(AutofacContainer);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// Configure 方法用于指定应用响应 HTTP 请求的方式。 可通过将中间件组件添加到 IApplicationBuilder 实例来配置请求管道。
        /// Configure 方法可使用 IApplicationBuilder，但未在服务容器中注册。 托管创建 IApplicationBuilder 并将其直接传递到 Configure。
        /// ASP.NET Core 模板配置支持开发人员异常页、BrowserLink、错误页、静态文件和 ASP.NET Core MVC 的管道：
        /// 每个 Use 扩展方法将中间件组件添加到请求管道。 例如，UseMvc 扩展方法将路由中间件添加到请求管道，并将 MVC 配置为默认处理程序。
        /// 请求管道中的每个中间件组件负责调用管道中的下一个组件，或在适当情况下使链发生短路。 如果中间件链中未发生短路，则每个中间件都有第二次机会在将请求发送到客户端前处理该请求。
        /// 其他服务（如 IHostingEnvironment 和 ILoggerFactory），也可以在方法签名中指定。 如果指定，其他服务如果可用，将被注入。
        /// 有关如何使用 IApplicationBuilder 和中间件处理顺序的详细信息，请参阅中间件。
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            app.UseCookiePolicy();
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