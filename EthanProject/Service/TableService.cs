using Azure.Storage.Blobs.Models;
using EthanProject.DataAccess;
using System.IO;

namespace EthanProject.Service
{
    public class TableService : ITableService
    {
        private readonly ITableDataAccess _tableService;
        public TableService(ITableDataAccess tableService) { 
            _tableService = tableService;
        }

        public async Task<String> getFirstTableData(String tableName)
        {
            return await _tableService.getFirstTableData(tableName);
        }

        public async Task<String> replaceFirstTableData(String name, String faction, String race)
        {
            return await _tableService.replaceFirstTableData(name, faction, race);
        }
        public async Task<String> addTableData(String name, String faction, String race)
        {
            return await _tableService.addTableData(name, faction, race);
        }

        public async Task<String> reset()
        {
            return await _tableService.reset();
        }

        public async Task<List<String>> display(String? faction, String? race)
        {
            return await _tableService.display(faction, race);
        }

        public async Task<byte[]> displayBlogImageAsync(String path, BlobDownloadOptions Options)
        {
            return await _tableService.displayBlogImageAsync(path, Options);
        }
    }
}
