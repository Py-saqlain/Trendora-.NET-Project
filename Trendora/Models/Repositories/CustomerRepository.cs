using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Trendora.Models;
using Trendora.Models.Interfaces;

namespace Trendora.Models.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        public CustomerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public void SaveCustomer(Customer customer)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string AddQuery = @"INSERT INTO AspNetUsers
                      (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
                       PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, 
                       TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, 
                       FullName, Address, ProfileImgPath, CreatedAt, UpdatedAt) 
                      VALUES 
                      (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed,
                       @PasswordHash, @SecurityStamp, @ConcurrencyStamp, @PhoneNumber, @PhoneNumberConfirmed,
                       @TwoFactorEnabled, @LockoutEnd, @LockoutEnabled, @AccessFailedCount,
                       @FullName, @Address, @ProfileImgPath, GETDATE(), GETDATE())";

                conn.Execute(AddQuery, customer);
            }
        }

        public IEnumerable<Customer> GetAllCustomers()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT u.* 
            FROM AspNetUsers u
            LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
            LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
            WHERE r.Name != 'Admin' OR r.Name IS NULL
            ORDER BY u.CreatedAt DESC";

                return con.Query<Customer>(query);
            }
        }

       


        public int GetCustomerCount()
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.ExecuteScalar<int>("SELECT COUNT(*) FROM AspNetUsers");
        }

        public Customer GetCustomer(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            using (var con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT * FROM AspNetUsers WHERE Email = @email";
                return con.QueryFirstOrDefault<Customer>(query, new { email = email });
            }
        }

        public Customer GetCustomerById(string id)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT * FROM AspNetUsers WHERE Id = @id";
                return con.QueryFirstOrDefault<Customer>(query, new { id = id });
            }
        }

        public void UpdateCustomer(Customer customer)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE AspNetUsers 
                        SET FullName = @FullName, 
                            UserName = @UserName,
                            NormalizedUserName = @NormalizedUserName,
                            Email = @Email,
                            NormalizedEmail = @NormalizedEmail,
                            PhoneNumber = @PhoneNumber, 
                            Address = @Address, 
                            ProfileImgPath = @ProfileImgPath, 
                            UpdatedAt = GETDATE()
                        WHERE Id = @Id";
                con.Execute(query, customer);
            }
        }

        public void UpdatePassword(string userId, string passwordHash)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE AspNetUsers 
                                SET PasswordHash = @PasswordHash,
                                    UpdatedAt = GETDATE()
                                WHERE Id = @Id";
                con.Execute(query, new { Id = userId, PasswordHash = passwordHash });
            }
        }

        public void DeleteCustomer(string id)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                string query = @"Delete from AspNetUsers 
                                WHERE Id = @Id";
                con.Execute(query, new { Id =id});
            }

        }


    }
}