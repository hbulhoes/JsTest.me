using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ScriptsDomain
{
    [Route("api/[controller]")]
    public class Weather : Controller
    {
        // GET: api/values
        [HttpGet]
        public async Task<JsonResult> Get()
        {
            JToken weatherInfo;

            var request = WebRequest.CreateHttp("http://api.openweathermap.org/data/2.5/weather?id=3448433&APPID=0a8c5046add15c781bba9eeb0015b11e");
            request.ContentType = "application/json; charset=utf-8";

            var response = await request.GetResponseAsync();
            using (var responseStream = response.GetResponseStream())
            {
                var reader = new StreamReader(responseStream, Encoding.UTF8);
                weatherInfo = JToken.Parse(reader.ReadToEnd());
            }

            Response.Headers["Access-Control-Allow-Origin"] = "*";

            return new JsonResult(new
            {
                Place = (string)weatherInfo["name"],
                Forecast = (string)weatherInfo["weather"][0]["description"],
                Icon = (string)weatherInfo["weather"][0]["icon"]
            });
        }
    }
}
