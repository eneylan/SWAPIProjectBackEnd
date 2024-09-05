using Azure.Storage.Blobs.Models;

namespace EthanProject.DataAccess
{
    public interface ITableDataAccess
    {
        Task<String> getFirstTableData(String tableName);
        Task<String> replaceFirstTableData(String name, String faction, String race);
        Task<String> addTableData(String name, String faction, String race);
        Task<String> reset();
        Task<List<String>> display(String? faction, String? race);
        Task<byte[]> displayBlogImageAsync(String path, BlobDownloadOptions Options);
    }
}
