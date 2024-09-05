using Azure.Storage.Blobs.Models;
using EthanProject.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EthanProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SWAPIController : ControllerBase
    {
        

        private readonly ILogger<SWAPIController> _logger;
        private readonly ITableService _tableService;

        public SWAPIController(ILogger<SWAPIController> logger, ITableService tableService)
        {
            _logger = logger;
            _tableService = tableService;
        }

        [AllowAnonymous]
        [Route("storeData")]
        [HttpGet]
        public async Task<String> storeData(String name, String faction, String race, String tableName)
        {
            var response = await _tableService.getFirstTableData(name);

            if (response == "true")
            {
                return await _tableService.replaceFirstTableData(name, faction, race);
            }
            else
            {
                return await _tableService.addTableData(name, faction, race);
            }
        }

        [AllowAnonymous]
        [Route("reset")]
        [HttpGet]
        public async Task<String> reset()
        {
            return await _tableService.reset();
        }

        [AllowAnonymous]
        [Route("display")]
        [HttpGet]
        public async Task<List<String>> display(String? faction, String? race)
        {
            if (faction == "null") { faction = null; }
            if(race == "null") { race = null; }
            return await _tableService.display(faction, race);
        }

        [AllowAnonymous]
        [Route("displayImages")]
        [HttpGet]
        public async Task<IActionResult> display(String path)
        {
            var img = await _tableService.displayBlogImageAsync(path, new BlobDownloadOptions());
            return File(img, "image/jpg");
        }
    }
}
