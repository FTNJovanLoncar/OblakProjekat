using Microsoft.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;

namespace MovieData
{
    public class UserDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;


        public UserDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("UserTable");
            _table.CreateIfNotExists();
        }

        public IQueryable<User> RetrieveAllUsers()
        {
            var results = from g in _table.CreateQuery<User>()
                          where g.PartitionKey == "User"
                          select g;
            return results;
        }

        public void AddStudent(User newUser)
        {
            TableOperation insertOperation = TableOperation.Insert(newUser);
            _table.Execute(insertOperation);
        }
    }
}
