using ApiProject;
using ApiProject.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiTest.Helpers
{
    public static class DbUtilities
    {
        internal static List<User> DbUsers = new()
        {
            new User()
            {
                FirstName = "MyNameIs",
                LastName = "ABCDEF",
                Password = "xdxdxd12!",
                Username = "testuser"
            },
            new User()
            {
                FirstName = "User2",
                LastName = "AAAAA!@ł",
                Password = "ŻŹÓŁĆĘĄ",
                Username = "whyme"
            }
        };

        internal static void InitDbForTests(ApiProject.ApiContext db)
        {
            db.User.AddRange(DbUsers);
            db.SaveChanges();
        }

        internal static void ReinitDbForTests(ApiProject.ApiContext db)
        {
            db.User.Clear();
            db.SaveChanges();
            InitDbForTests(db);
        }

        static void Clear<T>(this DbSet<T> table) where T : class
        {
            table.RemoveRange(table);
        }
    }
}
