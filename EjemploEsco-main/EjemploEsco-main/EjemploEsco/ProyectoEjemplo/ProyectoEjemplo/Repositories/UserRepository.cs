using ProyectoEjemplo.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoEjemplo.Repositories
{
    public class UserRepository : RepositoryBase, IUserRepository
    {
        public void Add(UserModel userModel)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText =
                    "INSERT INTO [User] (Id, Username, [Password], Name, LastName, Email) " +
                    "VALUES(@Id, @Username, @Password, @Name, @LastName, @Email)";
                command.Parameters.AddWithValue("@Id", userModel.Id);
                command.Parameters.AddWithValue("@Username", userModel.Username);
                command.Parameters.AddWithValue("@Password", userModel.Password);
                command.Parameters.AddWithValue("@Name", userModel.Name);
                command.Parameters.AddWithValue("@LastName", userModel.LastName);
                command.Parameters.AddWithValue("@Email", userModel.Email);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }


        public bool AuthenticateUser(NetworkCredential credential)
        {
            bool validUser;
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "select * from [User] where username = @username and [password] = @password";
                command.Parameters.Add("@username", System.Data.SqlDbType.NVarChar).Value = credential.UserName;
                command.Parameters.Add("@password", System.Data.SqlDbType.NVarChar).Value = credential.Password;
                validUser = command.ExecuteScalar() == null ? false : true;
            }
            return validUser;
        }

        public void Remove(string username)
        {
            throw new NotImplementedException();
        }

        public UserModel GetByUsername(string username)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText =
                    "SELECT Id, Username, [Password], Name, LastName, Email FROM [User] WHERE Username = @Username";
                command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar).Value = username;

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new UserModel
                    {
                        Id = reader["Id"].ToString(),
                        Username = reader["Username"].ToString(),
                        Password = reader["Password"].ToString(),
                        Name = reader["Name"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Email = reader["Email"].ToString()
                    };
                }
            }
        }

        public void Edit(UserModel useModel)
        {
            throw new NotImplementedException();
        }
    }
}
