using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Models
{
    [Table("Passwords", Schema = "TestTaskSchema")]
    public class PasswordsModel
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("userId")]
        public string UserId { get; set; }

        [Column("password")]
        public string Password { get; set; }
    }
}
