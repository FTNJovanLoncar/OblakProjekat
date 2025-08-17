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

        public async Task<User> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email must be provided", nameof(email));

            try
            {
                System.Diagnostics.Debug.WriteLine($"Retrieving user with PartitionKey='User', RowKey='{email}'");

                TableOperation retrieveOperation = TableOperation.Retrieve<User>("User", email);
                TableResult result = await _table.ExecuteAsync(retrieveOperation);

                if (result.Result == null)
                {
                    System.Diagnostics.Debug.WriteLine("User not found.");
                    return null;  // No user found with that email
                }

                return result.Result as User;
            }
            catch (StorageException ex)
            {
                throw new Exception("Azure Storage retrieval failed: " + ex.Message, ex);
            }
        }



        public async Task UpdateUserAsync(User user)
        {
            try
            {
                TableOperation replaceOperation = TableOperation.Replace(user);
                TableResult result = await _table.ExecuteAsync(replaceOperation);

                if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
                {
                    Console.WriteLine("User updated successfully.");
                }
                else
                {
                    throw new Exception("Update returned HTTP status: " + result.HttpStatusCode);
                }
            }
            catch (StorageException ex)
            {
                throw new Exception("Azure Storage update failed: " + ex.Message, ex);
            }
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

        public async Task<User[]> GetAllUsersAsync()
        {
            try
            {
                var query = new TableQuery<User>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "User"));

                var users = new System.Collections.Generic.List<User>();
                TableContinuationToken token = null;

                do
                {
                    var segment = await _table.ExecuteQuerySegmentedAsync(query, token);
                    users.AddRange(segment.Results);
                    token = segment.ContinuationToken;
                } while (token != null);

                return users.ToArray();
            }
            catch (StorageException ex)
            {
                throw new Exception("Azure Storage query failed: " + ex.Message, ex);
            }
        }

    }
}
