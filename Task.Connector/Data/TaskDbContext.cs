using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Task.Connector.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Data
{
    public class TaskDbContext : DbContext
    {
        public DbSet<ItRoleModel> ItRole { get; set; }
        public DbSet<PasswordsModel> Passwords { get; set; }
        public DbSet<RequestRightModel> RequestRight { get; set; }
        public DbSet<UserModel> User { get; set; }
        public DbSet<UserITRoleModel> UserITRole { get; set; }
        public DbSet<UserRequestRightModel> UserRequestRight { get; set; }

        private string _connection;

        public TaskDbContext(string connection)
        {
            Regex regex = new Regex(@"ConnectionString='([^']*)'");

            var connectionRegex = regex.Match(connection);

            if (connectionRegex.Success)
            {
                _connection = connectionRegex.Groups[1].Value;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItRoleModel>().HasKey(itRoleModel => itRoleModel.Id);

            modelBuilder.Entity<PasswordsModel>().HasKey(passwordModel => passwordModel.Id);

            modelBuilder.Entity<RequestRightModel>().HasKey(requestRightModel => requestRightModel.Id);

            modelBuilder.Entity<UserITRoleModel>().HasKey(userITRoleModel => new 
            {
                userITRoleModel.RoleId,
                userITRoleModel.UserId
            });

            modelBuilder.Entity<UserModel>().HasKey(UserModel => UserModel.Login);

            modelBuilder.Entity<UserRequestRightModel>().HasKey(userRequestRightModel => new
            {
                userRequestRightModel.UserId,
                userRequestRightModel.RightId
            });
        }
    }
}
