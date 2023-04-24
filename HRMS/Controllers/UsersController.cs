using HRMS.Areas.IdentityManager.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using HRMS.Helpers;

namespace HRMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger _logger;
        private readonly Dictionary<string, string> _roles;
        private readonly Dictionary<string, string> _claimTypes;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;

            _roles = roleManager.Roles.ToDictionary(r => r.Id, r => r.Name);
            var fldInfo = typeof(ClaimTypes).GetFields(BindingFlags.Static | BindingFlags.Public);
            _claimTypes = fldInfo.ToDictionary(i => i.Name, i => (string)i.GetValue(null));
        }

        public IActionResult Users()
        {
            ViewBag.Roles = _roles;
            ViewBag.ClaimTypes = _claimTypes.Keys.OrderBy(s => s);
            return View();
        }

        public IActionResult Roles()
        {
            ViewBag.ClaimTypes = _claimTypes.Keys.OrderBy(s => s);
            return View();
        }

        [HttpGet("api/[action]")]
        public  IActionResult UserList(int draw, List<Dictionary<string, string>> columns, List<Dictionary<string, string>> order, int start, int length, Dictionary<string, string> search)
        {
            var users = _userManager.Users; //.Include(u => u.Roles).Include(u => u.Claims);
            string filter = search["value"];
            var qry = users.Where(u =>
                string.IsNullOrWhiteSpace(filter) || u.Email.Contains(filter) ||
                string.IsNullOrWhiteSpace(filter) || u.UserName.Contains(filter)
            );

            var idx = int.Parse(order[0]["column"]);
            var dir = order[0]["dir"];
            var col = columns[idx]["data"];

            if (dir == "asc")
                qry = qry.OrderBy(x => EF.Property<string>(x, char.ToUpper(col[0]) + col.Substring(1)));
            else
                qry = qry.OrderByDescending(x => EF.Property<string>(x, char.ToUpper(col[0]) + col.Substring(1)));

            var result = new
            {
                draw,
                recordsTotal = users.Count(),
                recordsFiltered = qry.Count(),
                data = qry.Skip(start)
                //.Take(length)
                .ToArray().Select(u => new
                {
                    u.Id,
                    u.Email,
                    LockedOut = u.LockoutEnd == null ? string.Empty : "Yes",
                    //Roles = u.Roles.Select(r => _roles[r.RoleId]),
                    Roles = _userManager.GetRolesAsync(u).Result.ToList(), //.Select( r =>  _roles[r.Id]),
                    //Key/Value props not camel cased (https://github.com/dotnet/corefx/issues/41309)
                    //Claims = u.Claims.Select(c => new KeyValuePair<string, string>(_claimTypes.Single(x => x.Value == c.ClaimType).Key, c.ClaimValue)),
                    Claims = new KeyValuePair<string, string>[] {},
                    DisplayName = "", //u.Claims.FirstOrDefault(c => c.ClaimType == ClaimTypes.Name)?.ClaimValue,
                    u.UserName
                })
            };

            return Json(result);
        }

        [HttpPost("api/[action]")]
        public async Task<IActionResult> CreateUser(string userName, string name, string email, string password)
        {
            try
            {
                var user = new ApplicationUser() { Email = email, UserName = userName };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created user {userName}.", userName);

                    if (name != null)
                        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, name));

                    return NoContent();
                }
                else
                    return BadRequest(result.Errors.First().Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure creating user {userName}.", userName);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("api/[action]")]
        public async Task<ActionResult> UpdateUser(string id, string email, bool locked, string[] roles, List<KeyValuePair<string, string>> claims)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound("User not found.");

                user.Email = email;
                user.LockoutEnd = locked ? DateTimeOffset.MaxValue : default(DateTimeOffset?);

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Updated user {name}.", user.UserName);

                    var userRoles = await _userManager.GetRolesAsync(user);

                    foreach (string role in roles.Except(userRoles))
                        await _userManager.AddToRoleAsync(user, role);

                    foreach (string role in userRoles.Except(roles))
                        await _userManager.RemoveFromRoleAsync(user, role);

                    var userClaims = await _userManager.GetClaimsAsync(user);

                    foreach (var kvp in claims.Where(a => !userClaims.Any(b => _claimTypes[a.Key] == b.Type && a.Value == b.Value)))
                        await _userManager.AddClaimAsync(user, new Claim(_claimTypes[kvp.Key], kvp.Value));

                    foreach (var claim in userClaims.Where(a => !claims.Any(b => a.Type == _claimTypes[b.Key] && a.Value == b.Value)))
                        await _userManager.RemoveClaimAsync(user, claim);

                    return NoContent();
                }
                else
                    return BadRequest(result.Errors.First().Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure updating user {userId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("api/[action]")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound("User not found.");

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Deleted user {name}.", user.UserName);
                    return NoContent();
                }
                else
                    return BadRequest(result.Errors.First().Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure deleting user {userId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("api/[action]")]
        public async Task<IActionResult> ResetPassword(string id, string password, string verify)
        {
            try
            {
                if (password != verify)
                    return BadRequest("Passwords entered do not match.");

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound("User not found.");

                if (await _userManager.HasPasswordAsync(user))
                    await _userManager.RemovePasswordAsync(user);

                var result = await _userManager.AddPasswordAsync(user, password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Password reset for {name}.", user.UserName);
                    return NoContent();
                }
                else
                    return BadRequest(result.Errors.First().Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed password reset for user {userId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("api/[action]")]
        public IActionResult RoleList(int draw, List<Dictionary<string, string>> columns, List<Dictionary<string, string>> order, int start, int length, Dictionary<string, string> search)
        {
            var roles = _roleManager.Roles; //.Include(r => r.Claims);

            string filter = search["value"];
            var qry = roles.Where(r =>
                string.IsNullOrWhiteSpace(filter) || r.Name.Contains(filter)
            );

            var idx = int.Parse(order[0]["column"]);
            var dir = order[0]["dir"];
            var col = columns[idx]["data"];

            if (dir == "asc")
                qry = qry.OrderBy(x => EF.Property<string>(x, char.ToUpper(col[0]) + col.Substring(1)));
            else
                qry = qry.OrderByDescending(x => EF.Property<string>(x, char.ToUpper(col[0]) + col.Substring(1)));

            var result = new
            {
                draw,
                recordsTotal = roles.Count(),
                recordsFiltered = qry.Count(),
                data = qry.Skip(start).Take(length).ToArray().Select(r => new
                {
                    r.Id,
                    r.Name,
                    //Key/Value props not camel cased (https://github.com/dotnet/corefx/issues/41309)
                    Claims = new KeyValuePair<string, string>() { } // r.Claims.Select(c => new KeyValuePair<string, string>(_claimTypes.Single(x => x.Value == c.ClaimType).Key, c.ClaimValue))
                })
            };

            return Json(result);
        }

        [HttpPost("api/[action]")]
        public async Task<IActionResult> CreateRole(string name)
        {
            try
            {
                var role = new ApplicationRole(name);

                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role {name}.", name);
                    return NoContent();
                }
                else
                    return BadRequest(result.Errors.First().Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure creating role {name}.", name);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("api/[action]")]
        public async Task<ActionResult> UpdateRole(string id, string name, List<KeyValuePair<string, string>> claims)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                    return NotFound("Role not found.");

                role.Name = name;

                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Updated role {name}.", role.Name);

                    var roleClaims = await _roleManager.GetClaimsAsync(role);

                    foreach (var kvp in claims.Where(a => !roleClaims.Any(b => _claimTypes[a.Key] == b.Type && a.Value == b.Value)))
                        await _roleManager.AddClaimAsync(role, new Claim(_claimTypes[kvp.Key], kvp.Value));

                    foreach (var claim in roleClaims.Where(a => !claims.Any(b => a.Type == _claimTypes[b.Key] && a.Value == b.Value)))
                        await _roleManager.RemoveClaimAsync(role, claim);

                    return NoContent();
                }
                else
                    return BadRequest(result.Errors.First().Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure updating role {roleId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("api/[action]")]
        public async Task<ActionResult> DeleteRole(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                    return NotFound("Role not found.");

                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Deleted role {name}.", role.Name);
                    return NoContent();
                }
                else
                    return BadRequest(result.Errors.First().Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure deleting role {roleId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
