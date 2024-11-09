using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Task.Connector.Data;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }

        private TaskDbContext _context;

        public void StartUp(string connectionString)
        {
            _context = new TaskDbContext(connectionString);
        }

        public void CreateUser(UserToCreate user)
        {
            if (user.Login.IsNullOrEmpty() || user.HashPassword.IsNullOrEmpty())
            {
                Logger.Error("It is impossible to create a new user, the login or password value is empty");
                return;
            }

            if (IsUserExists(user.Login))
            {
                Logger.Error($"The user {user.Login} already exists");
                return;
            }

            _context.User.Add(new UserModel
            {
                Login = user.Login,
                LastName = user.Properties.FirstOrDefault(p => p.Name == "lastName")?.Value ?? "lastName",
                FirstName = user.Properties.FirstOrDefault(p => p.Name == "firstName")?.Value ?? "firstName",
                MiddleName = user.Properties.FirstOrDefault(p => p.Name == "middleName")?.Value ?? "middleName",
                TelephoneNumber = user.Properties.FirstOrDefault(p => p.Name == "telephoneNumber")?.Value ?? "telephoneNumber",
                IsLead = bool.Parse(user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value ?? "false")
            });

            _context.Passwords.Add(new PasswordsModel 
            {
                UserId = user.Login,
                Password = user.HashPassword
            });

            _context.SaveChanges();

            Logger.Warn($"A user with Login {user.Login} has been successfully created");
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var userEntityType = _context.Model.FindEntityType(typeof(UserModel));

            if (userEntityType == null)
            {
                Logger.Warn("UserModel entity type not found in the model.");
                return Enumerable.Empty<Property>();
            }

            var userProperties = userEntityType.GetProperties().Select(p => new Property
            (
                p.GetColumnName(),
                p.PropertyInfo?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "None description"
            ));

            Logger.Warn($"All properties have been successfully obtained");

            return userProperties ?? Enumerable.Empty<Property>();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = _context.User.Where(p => p.Login == userLogin).FirstOrDefault();

            if (user == null)
            {
                Logger.Error($"The user with this login *{userLogin}* was not found");
                return Enumerable.Empty<UserProperty>();
            }

            var userEntityType = _context.Model.FindEntityType(typeof(UserModel));

            if (userEntityType == null)
            {
                Logger.Warn("UserModel entity type not found in the model.");
                return Enumerable.Empty<UserProperty>();
            }

            var userProperties = userEntityType.GetProperties().Where(p => !p.IsPrimaryKey()).Select(p => new UserProperty
            (
                p.GetColumnName(),
                p.PropertyInfo?.GetValue(user)?.ToString() ?? "Empty"
            ));

            Logger.Warn($"The properties of the user *{userLogin}* have been successfully obtained");

            return userProperties ?? Enumerable.Empty<UserProperty>();
        }

        public bool IsUserExists(string userLogin)
        {
            return _context.User.Where(p => p.Login == userLogin).FirstOrDefault() != null;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = _context.User.Where(p => p.Login == userLogin).FirstOrDefault();

            if (user == null)
            {
                Logger.Error($"The user with this login *{userLogin}* was not found");
                return;
            }

            var userEntityType = _context.Model.FindEntityType(typeof(UserModel));

            if(userEntityType == null)
            {
                Logger.Warn("UserModel entity type not found in the model.");
                return;
            }

            foreach (var property in properties)
            {
                userEntityType?.GetProperties().Where(p => p.GetColumnName() == property.Name).FirstOrDefault()?.PropertyInfo?.SetValue(user, property.Value);
            }

            _context.SaveChanges();

            Logger.Warn("The user's properties have been successfully set");
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var requestRightPermissions = _context.RequestRight.Select(requestRight => new Permission
            (
                Guid.NewGuid().ToString(),
                requestRight.Name,
                "Request right permissions"
            ));

            var itRolePermissions = _context.ItRole.Select(itRole => new Permission
            (
                Guid.NewGuid().ToString(),
                itRole.Name,
                "It role permissions"
            ));

            Logger.Warn("Successful extraction of all permissions");

            return Enumerable.Union(requestRightPermissions ?? Enumerable.Empty<Permission>(), itRolePermissions ?? Enumerable.Empty<Permission>());
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
            {
                Logger.Error($"There is no user with this login *{userLogin}*");
                return;
            }

            foreach (var rightId in rightIds)
            {
                if (int.TryParse(rightId.Split(':').Last(), out int id))
                {
                    if (_context.UserRequestRight.Where(p => p.UserId == userLogin && p.RightId == id).FirstOrDefault() == null)
                    {
                        _context.UserRequestRight.Add(new UserRequestRightModel
                        {
                            UserId = userLogin,
                            RightId = id
                        });
                    }
                    else Logger.Warn($"(UserRequestRight) The user *{userLogin}* already has this permission *{id}*");

                    if (_context.UserITRole.Where(p => p.UserId == userLogin && p.RoleId == id).FirstOrDefault() == null)
                    {
                        _context.UserITRole.Add(new UserITRoleModel
                        {
                            UserId = userLogin,
                            RoleId = id
                        });
                    }
                    else Logger.Warn($"(UserITRole) The user *{userLogin}* already has this permission *{id}*");
                }
            }
            _context.SaveChanges();
            Logger.Warn($"For the user *{userLogin}*, all the transferred permissions have been successfully added");
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
            {
                Logger.Error($"There is no user with this login *{userLogin}*");
                return;
            }

            foreach(var rightId in rightIds)
            {
                if (int.TryParse(rightId.Split(':').Last(), out int id))
                {
                    var userRequestRight = _context.UserRequestRight.Where(p => p.UserId == userLogin && p.RightId == id).FirstOrDefault();

                    if (userRequestRight != null)
                    {
                        _context.UserRequestRight.Remove(userRequestRight);
                    }
                }
            }

            _context.SaveChanges();
            Logger.Warn($"For the user {userLogin}, all the transferred permissions have been successfully deleted");
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            if (!IsUserExists(userLogin))
            {
                Logger.Error($"There is no user with this login *{userLogin}*");
                return Enumerable.Empty<string>();
            }


            var requestRightPermisions = _context.UserRequestRight.Where(p => p.UserId == userLogin).Join(_context.RequestRight, u => u.RightId, r => r.Id, (u,r) => r.Name);

            Logger.Warn($"The permissions of user *{userLogin}* have been successfully obtained");

            return requestRightPermisions;
        }
    }
}