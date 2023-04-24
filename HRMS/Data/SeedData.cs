using HRMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Net.NetworkInformation;

namespace HRMS.Data
{
    public class SeedData
    {
        //public static async Task Initialize(IServiceProvider serviceProvider, string password)
        //{
        //    using (var context = new ApplicationDbContext(
        //        serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
        //    {
        //        var adminID = await EnsureUser(serviceProvider, password, "Admin");
        //        await EnsureRole(serviceProvider, adminID, "Admin");

        //        var restaurantID = await EnsureUser(serviceProvider, password, "Manager");
        //        await EnsureRole(serviceProvider, restaurantID, "Manager");

        //        var receptionID = await EnsureUser(serviceProvider, password, "receptionist");
        //        await EnsureRole(serviceProvider, receptionID, "Receptionist");
        //    }

        //}

        public static async Task Initialize(IServiceProvider serviceProvider, List<InitUser> initUsers)
        {
            try
            {
                using (var context = new ApplicationDbContext(
                    serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
                {
                    await InitRole(serviceProvider, "Admin");
                    await InitRole(serviceProvider, "Manager");
                    await InitRole(serviceProvider, "Receptionist");

                    foreach (var initUser in initUsers)
                    {
                        await InitUser(serviceProvider, initUser);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private static async Task InitRole(IServiceProvider serviceProvider, string role)
        {
            try
            {

                var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private static async Task InitUser(IServiceProvider serviceProvider, InitUser initUser)
        {
            try
            {

                var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

                if (await userManager.FindByNameAsync(initUser.UserName) == null)
                {
                    var user = new IdentityUser(initUser.UserName);

                    await userManager.CreateAsync(user, initUser.Password);
                    await userManager.AddToRoleAsync(user, initUser.Role);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private static async Task<string> EnsureUser(IServiceProvider serviceProvider, string testUserPw, string UserName)
        {
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

            var user = await userManager.FindByNameAsync(UserName);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = UserName,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, testUserPw);
            }

            if (user == null)
            {
                throw new Exception("The password is probably not strong enough!");
            }
            return user.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider, string uid, string role)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (roleManager == null)
            {
                throw new Exception("roleManager null");
            }
            IdentityResult IR;
            if (!await roleManager.RoleExistsAsync(role))
            {
                IR = await roleManager.CreateAsync(new IdentityRole(role));
            }
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();
            var user = await userManager.FindByIdAsync(uid);
            if (user == null)
            {
                throw new Exception("The testUserPw password was probably not strong enough!");
            }

            IR = await userManager.AddToRoleAsync(user, role);
            return IR;
        }

    }
}
