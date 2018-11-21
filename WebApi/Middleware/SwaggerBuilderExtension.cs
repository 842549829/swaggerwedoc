using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace WebApi.Middleware
{
    public static class SwaggerBuilderExtension
    {
        /// <summary>
        /// 使用swagger生成文档并发送到文档中心
        /// </summary>
        /// <param name="builder">IApplicationBuilder</param>
        /// <param name="options">options 配置</param>
        /// <param name="httpClientFactory">httpClientFactory</param>
        /// <returns></returns>
        public static IApplicationBuilder UseSwaggerApiDoc(this IApplicationBuilder builder, IHttpClientFactory httpClientFactory, Action<ApiDocOptions> options = null)
        {
            if (builder.ApplicationServices.GetService(typeof(IOptions<ApiDocOptions>)) is IOptions<ApiDocOptions> serviceOption)
            {
                var option = serviceOption.Value ?? new ApiDocOptions();

                options?.Invoke(option);

                builder.Map(option.ApiDocUpdatePath, config =>
                {
                    config.Run(async context =>
                    {
                        //先执行cmd命令生成文档 需要项目引用 Swashbuckle.AspNetCore.Cli
                        var result = GenerateApiDocFile(option);
                        using (var client = httpClientFactory.CreateClient())
                        {
                            client.BaseAddress = option.ApiHost;
                            var requestContent = new MultipartFormDataContent();
                            var fileBuffer = string.IsNullOrWhiteSpace(option.ApiJsonFilePath)
                                ? Encoding.UTF8.GetBytes(result)
                                : File.ReadAllBytes(option.ApiJsonFilePath);

                            var filecontent = new ByteArrayContent(fileBuffer);
                            filecontent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                            requestContent.Add(filecontent, "files", option.ApiName);

                            var responseMessage = await client.PostAsync(option.ApiDocEndPoint, requestContent);
                            if (responseMessage.IsSuccessStatusCode)
                            {
                                var buffer = Encoding.Default.GetBytes("Api doc updated success!\r\n" + result);
                                await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                            }
                            else
                            {
                                var buffer =
                                    Encoding.Default.GetBytes("Api doc updated failed!\r\n" + responseMessage.ReasonPhrase);
                                await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                            }
                        }
                    });
                });
            }

            return builder;
        }

        /// <summary>
        /// 生成api文档
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private static string GenerateApiDocFile(ApiDocOptions option)
        {
            try
            {
                var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
                var process = new Process();
                var startInfo = new ProcessStartInfo();
                var cmdoptions = !string.IsNullOrEmpty(option.ApiJsonFilePath)
                    ? $" -- output {option.ApiJsonFilePath} "
                    : $" --host {option.ApiHost}";

                startInfo.WorkingDirectory = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = $"/C dotnet swagger tofile {cmdoptions} {AppContext.BaseDirectory}{assemblyName}.dll {option.ApiName ?? assemblyName}";
                process.StartInfo = startInfo;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                var result = process.StandardOutput.ReadToEnd();
                if (string.IsNullOrWhiteSpace(result))
                {
                    var error = process.StandardError.ReadToEnd();
                    throw new InvalidDataException("没有正确地生成的API文档 " + error);
                }
                return result;
            }
            catch (InvalidDataException e)
            {
                throw new Exception("生成API文档失败" + e.Message, e);
            }
            catch (Exception e)
            {
                throw new Exception("执行命令出错，需要引用Swashbuckle.AspNetCore.Cli程序包。为了减少程序集的依赖可以按照如下方式添加：" + "<DotNetCliToolReference Include=\"Swashbuckle.AspNetCore.Cli\" Version=\"4.0.1\" Condition=\"'$(Configuration)' == 'Debug'\" />", e);
            }
        }
    }

    public class ApiDocOptions
    {
        public Uri ApiDocEndPoint { get; set; }

        public Uri ApiHost { get; set; }

        public string ApiName { get; set; }

        public string ApiDocUpdatePath { get; set; } = "/updateapidoc";

        public string ApiJsonFilePath { get; set; }
    }
}