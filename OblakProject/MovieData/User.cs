using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace MovieData
{
    public enum UserRole
    {
        Author,
        Viewer
    }

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
        public string Role { get; set; }

        public User(string indexNo)
        {
            PartitionKey = "User";
            RowKey = indexNo;
            UserRole = UserRole.Viewer; // default role
        }

        public User()
        {
            UserRole = UserRole.Viewer; // default role
        }


        public UserRole UserRole
        {
            get
            {
                if (Enum.TryParse(Role, out UserRole roleEnum))
                    return roleEnum;
                return UserRole.Viewer; // default if not set
            }
            set
            {
                Role = value.ToString();
            }
        }
    }
}
