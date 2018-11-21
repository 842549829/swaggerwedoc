using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    /// <summary>
    /// Values
    /// </summary>
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// Add
        /// </summary>
        /// <param name="actionResult">actionResult</param>
        /// <returns>结果</returns>
        [HttpPost]
        public ActionResult<bool> Add([FromBody] ActionResult actionResult)
        {
            return Ok(true);
        }

        /// <summary>
        /// Get by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>结果</returns>
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return Ok(id.ToString());
        }


        /// <summary>
        /// Get list
        /// </summary>
        /// <returns>结果</returns>
        [HttpGet]
        public ActionResult<string> Get()
        {
            return Ok(new List<string> { "a", "b" });
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="actionResult">actionResult</param>
        /// <returns>结果</returns>
        [HttpPut]
        public ActionResult<bool> Update([FromBody] ActionResult actionResult)
        {
            return Ok(true);
        }
    }
}