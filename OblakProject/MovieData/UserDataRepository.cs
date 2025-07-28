using Microsoft.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MovieData
{
    public class UserDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;


        public UserDataRepository()
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
                _table = tableClient.GetTableReference("UserTable");
                _table.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                // Log or rethrow with more info
                throw new Exception("Failed to initialize Azure Table Storage: " + ex.Message, ex);
            }
        }

        public IQueryable<User> RetrieveAllUsers()
        {
            var results = from g in _table.CreateQuery<User>()
                          where g.PartitionKey == "User"
                          select g;
            return results;
        }

        public async Task AddStudent(User newUser)
        {
            try
            {
                TableOperation insertOperation = TableOperation.Insert(newUser);
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
