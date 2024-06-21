using PRS.Data;
using PRS.Models;
using System.Security.Cryptography;
using System.Text;

namespace PRS.Services
{
    public class DbInitialiser
    {
        private readonly PRSContext _context;

        public DbInitialiser(PRSContext context)
        {
           _context = context;
        }    
        public void Run()
        {
            if(_context.Roles.ToList().Count == 0)
            {
                var RoleAdmin = new Role() { Rolename = "admin" };
                var RoleReviewer = new Role() { Rolename = "reviewer" };
                var RoleUser = new Role() { Rolename = "user" };
                _context.Roles.Add(RoleAdmin);
                _context.Roles.Add(RoleReviewer);
                _context.Roles.Add(RoleUser);
                _context.SaveChanges();
            }
            if(_context.Users.Count() == 0)
            {
                using var hmac = new HMACSHA512();
                User user = new User()
                {
                    Username = "admin",
                    Firstname = "admin",
                    Lastname = "admin",
                    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("adminpass")),
                    PasswordSalt = hmac.Key
                };
                _context.Add(user);
                _context.SaveChanges();

                var userToRolesAdmin = new UserRole()
                {
                    RoleID = 1,
                    UserID = user.Id
                };
                var userToRolesReviewer = new UserRole() { RoleID = 2, UserID = user.Id };    
                var userToRolesUser = new UserRole() { RoleID =3, UserID = user.Id};
                _context.Add(userToRolesAdmin);
                _context.Add(userToRolesReviewer);
                _context.Add(userToRolesUser);
                _context.SaveChanges();
            }


        }
    }
}
