using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IDao.Lib;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;

namespace Controllers.ApiController
{
    [Route("api/[controller]/[action]")]
    public class UserController: Controller
    {
        public IExampleDao _iExampleDao { get; set; }
        public UserController(IExampleDao iExampleDao)
        {
            _iExampleDao = iExampleDao;
        }

        [HttpGet]
        public async Task<dynamic> Get()
        {
            var s = await _iExampleDao.GetUser();
            return s;
        }
        [HttpGet]
        public async Task<dynamic> PageList()
        {
            var client = new HttpClient();

            // Execution of GetFirstCharactersCountAsync() is yielded to the caller here
            // GetStringAsync returns a Task<string>, which is *awaited*
            var page = await client.GetStringAsync("https://www.dotnetfoundation.org");
            var s = await _iExampleDao.PageList();
            return s;
        }
    }
}
