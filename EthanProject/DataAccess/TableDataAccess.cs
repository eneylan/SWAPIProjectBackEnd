using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
//using Microsoft.Azure.Cosmos.Table;


namespace EthanProject.DataAccess
{
    public class TableDataAccess : ITableDataAccess
    {

        private readonly TableServiceClient _tableServiceClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<TableDataAccess> _logger;

        public TableDataAccess(ILogger<TableDataAccess> logger, TableServiceClient tableDataAccess, BlobServiceClient blobServiceClient) { 
            _tableServiceClient = tableDataAccess;
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        public async Task<String> getFirstTableData(String tableName)
        {
            StringBuilder sb = new StringBuilder();

            var tableClient = _tableServiceClient.GetTableClient("Characters");

            AsyncPageable<TableEntity> results = tableClient.QueryAsync<TableEntity>(filter: $"RowKey eq '1'");


            // Iterate the <see cref="Pageable"> to access all queried entities.
            await foreach (TableEntity qEntity in results)
            {
                sb.Append(qEntity.GetString("Name"));
            }

            if (sb.ToString().Trim() == "p")
            {
                return "true";
            } else
            {
                return "false";
            }

        }



        public async Task<String> replaceFirstTableData(String name, String faction, String race)
        {

            var tableClient = _tableServiceClient.GetTableClient("Characters");

            var tableEntity = new TableEntity("0", "1")
            {
                { "Faction", faction },
                { "Name", name },
                { "Race", race }
            };

            await tableClient.UpdateEntityAsync(tableEntity, ETag.All, TableUpdateMode.Merge);

            return "";
        }
        public async Task<String> addTableData(String name, String faction, String race)
        {
            String nextRowKey = await getNextRowkeyAsync();

            int next = Int32.Parse(nextRowKey);
            next++;
            nextRowKey = next + "";

            var tableClient = _tableServiceClient.GetTableClient("Characters");

            Boolean repeat = false;
            AsyncPageable<TableEntity> results = tableClient.QueryAsync<TableEntity>(filter: $"Name eq '{name}'");
            
            await foreach (TableEntity eq in results)
            {
                if (eq.GetString("Name") == name)
                {
                    repeat = true;
                    break;
                }
            }

           

            if (repeat)
            {
                _logger.LogInformation($"Not inserting, because the specified name '{name}' already exists");
                return "exceptionrc";
            }

            _logger.LogInformation($"The next row key is {nextRowKey}");

            var tableEntity = new TableEntity("0", nextRowKey)
            {
                { "Faction", faction },
                { "Name", name },
                { "Race", race }
            };
            await tableClient.AddEntityAsync(tableEntity);
            

            return "";
        }

       
        private async Task<String> getNextRowkeyAsync()
        {
            var tableClient = _tableServiceClient.GetTableClient("Characters");

            AsyncPageable<TableEntity> results = tableClient.QueryAsync<TableEntity>();

            String? rowkey = null;
            await foreach (TableEntity tableEntity in results)
            {
                if (tableEntity.GetString("RowKey") != null)
                {
                    if (rowkey == null || rowkey.CompareTo(tableEntity.GetString("RowKey")) < 0)
                    {
                        rowkey = tableEntity.GetString("RowKey");
                    }
                }
            }
            if (rowkey != null) { return rowkey; }
            return "";
        }

        public async Task<String> reset()
        {
            var tableClient = _tableServiceClient.GetTableClient("Characters");

            String nextRowKey = await getNextRowkeyAsync();
            int next = Int32.Parse(nextRowKey);
            next++;

            for (int i = 2; i <= next; i++)
            {
                AsyncPageable<TableEntity> results = tableClient.QueryAsync<TableEntity>(filter: $"RowKey eq '{i}'");
                await foreach (TableEntity eq in results)
                {
                    await tableClient.DeleteEntityAsync(eq);
                }
            }

            var tableEntity = new TableEntity("0", "1")
            {
                { "Faction", "p" },
                { "Name", "p" },
                { "Race", "p" }
            };

            await tableClient.UpdateEntityAsync(tableEntity, ETag.All, TableUpdateMode.Merge);

            return "";

        }

        public async Task<List<String>> display(String? faction, String? race)
        {
            List<String> list = new List<String>();
            var tableClient = _tableServiceClient.GetTableClient("Characters");
            
            if (faction == null)
            {
                AsyncPageable<TableEntity> results = tableClient.QueryAsync<TableEntity>(filter: $"Race eq '{race}'");
                await foreach (TableEntity eq in results)
                {
                    list.Add(eq.GetString("Name"));
                }
            } else if (race == null)
            {
                AsyncPageable<TableEntity> results = tableClient.QueryAsync<TableEntity>(filter: $"Faction eq '{faction}'");
                await foreach (TableEntity eq in results)
                {
                    list.Add(eq.GetString("Name"));
                }
            } else
            {
                AsyncPageable<TableEntity> results = tableClient.QueryAsync<TableEntity>(filter: $"Race eq '{race}' and Faction eq '{faction}'");
                await foreach (TableEntity eq in results)
                {
                    list.Add(eq.GetString("Name"));
                }
            }
            return list;
        }

        public async Task<byte[]> displayBlogImageAsync(String path, BlobDownloadOptions Options = null)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("swcontain");
            var blobClient = containerClient.GetBlobClient(path);
        
            Response<BlobDownloadResult> response = await blobClient.DownloadContentAsync(Options, CancellationToken.None);
            BlobDownloadResult downloadResult = response.Value;
            return downloadResult.Content.ToArray();
        }



    }
}
