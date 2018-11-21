using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ApiDoc.Controllers
{
    /// <summary>
    /// api文档接口
    /// </summary>
    [Route("api/[controller]")]
    public class DocsController : ControllerBase
    {
        private readonly SwaggerUIOptions _swaggerUIOptions;

        public DocsController(IOptions<SwaggerUIOptions> options, IApplicationLifetime applicationLifetime)
        {
            _swaggerUIOptions = options.Value;
            _applicationLifetime = applicationLifetime;
        }

        private IApplicationLifetime _applicationLifetime { get; }

        /// <summary>
        /// 上传api的json文件接口
        /// </summary>
        /// <param name="files">文件在表单中的key名，注意不是文件名称</param>
        /// <returns>200或400状态码</returns>
        [HttpPost]
        public async Task<ActionResult> Post(ICollection<IFormFile> files)
        {
            if (files.Count <= 0)
            {
                return BadRequest();
            }
            var file = Request.Form.Files[0];
            using (var fileStream = new FileStream($"wwwroot/apidocs/{file.FileName}.json", FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            _applicationLifetime.StopApplication();

            return Ok();
        }
    }
}