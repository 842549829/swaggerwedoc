using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace ApiDoc
{
    /*
     * 文档浏览地址http://localhost:61480/docs/index.html
     */
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("ApiDocServer", new Info
                {
                    Title = "Api文档服务器",
                    Version = "v1",
                    Description = "提供中心化的API文档查看工具，以下列出本服务的接口。"
                });
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c => { c.RouteTemplate = "docs/{documentName}/swagger.json"; });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.

            app.UseStaticFiles();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "docs";
                var paths = GetApiDocs();
                foreach (var path in paths)
                {
                    var filename = Path.GetFileName(path);
                    c.SwaggerEndpoint($"/apidocs/{filename}", filename);
                }

                //c.RoutePrefix = "swagger/ui";
                c.SwaggerEndpoint("ApiDocServer/swagger.json", "ApiDocServer");
            });

            app.UseMvc();
        }

        private IEnumerable<string> GetApiDocs()
        {
            var apiPaths = Directory.GetFileSystemEntries(@"wwwroot/apidocs");
            return apiPaths;
        }
    }
}