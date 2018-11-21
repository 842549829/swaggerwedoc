using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApi.Middleware;

namespace WebApi
{
    public class Startup
    {
        /// <summary>
        /// 启动项构造
        /// </summary>
        /// <param name="configuration">配置文件</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 配置文件
        /// </summary>
        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
#if DEBUG

            // API文档支持
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(Configuration["Api:ApiName"], new Swashbuckle.AspNetCore.Swagger.Info { Title = Configuration["Api:ApiName"], Version = Configuration["Api:Version"] });
                var files = Directory.GetFiles(AppContext.BaseDirectory).Where(n => n.EndsWith(".xml"));
                foreach (var file in files)
                {
                    options.IncludeXmlComments(file);
                }
            });
#endif

            // 注册服务并且实例化AutoFac替换默认容器
            var containerBuilder = new ContainerBuilder();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddHttpClient(Options.DefaultName);

            containerBuilder.Populate(services);
            return new AutofacServiceProvider(containerBuilder.Build());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IHttpClientFactory httpClientFactory)
        {

#if DEBUG
            app.UseSwaggerApiDoc(httpClientFactory, option =>
            {
                option.ApiDocEndPoint = new Uri(Configuration["Api:ApiDocEndPoint"]);
                option.ApiHost = new Uri(Configuration["Api:ApiHost"]);
                option.ApiName = Configuration["Api:ApiName"];
                option.ApiDocUpdatePath = Configuration["Api:ApiDocUpdatePath"];
            });
#endif

            app.UseMvc();
        }
    }
}
