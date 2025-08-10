using Microsoft.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PostData
{
    public class PostDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public PostDataRepository()
        {
            try
            {
                string connectionString = CloudConfigurationManager.GetSetting("DataConnectionString");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Azure storage connection string is missing.");
                }
                _storageAccount = CloudStorageAccount.Parse(connectionString);
                var tableClient = _storageAccount.CreateCloudTableClient();
                _table = tableClient.GetTableReference("PostTable");
                _table.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                // Log or rethrow with more info
                throw new Exception("Failed to initialize Azure Table Storage: " + ex.Message, ex);
            }
        }

        public IQueryable<Post> RetrieveAllUsers()
        {
            var results = from g in _table.CreateQuery<Post>()
                          where g.PartitionKey == "Post"
                          select g;
            return results;
        }

        public async Task AddPost(Post newPost)
        {
            try
            {
                TableOperation insertOperation = TableOperation.Insert(newPost);
                TableResult result = await _table.ExecuteAsync(insertOperation);

                if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
                {
                    Console.WriteLine("User inserted successfully.");
                }
                else
                {
                    Console.WriteLine("Insert returned HTTP status: " + result.HttpStatusCode);
                }
            }
            catch (StorageException ex)
            {
                throw new Exception("Azure Storage insert failed: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Insert failed: " + ex.Message, ex);
            }
        }

    }
}
