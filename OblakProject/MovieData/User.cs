using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieData
{
    public class User : TableEntity
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string ImageUrl { get; set; }

        public User(string IndexNo)
        {
            PartitionKey = "User";
            RowKey = IndexNo;
        }

        public User() { }
        

        
    }
}
