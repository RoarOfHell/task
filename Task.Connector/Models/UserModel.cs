using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Models
{
    [Table("User", Schema = "TestTaskSchema")]
    public class UserModel
    {
        [Column("login")]
        public string Login { get; set; }

        [Column("lastName")]
        public string LastName { get; set; }

        [Column("firstName")]
        public string FirstName { get; set; }

        [Column("middleName")]
        public string MiddleName { get; set; }

        [Column("telephoneNumber")]
        public string TelephoneNumber { get; set; }

        [Column("isLead")]
        public bool IsLead { get; set; }
    }
}
