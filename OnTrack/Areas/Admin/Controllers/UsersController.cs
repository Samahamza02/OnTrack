using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnTrack.Models;
using OnTrack.ViewModels.Admin;

namespace OnTrack.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _um;
    private readonly RoleManager<IdentityRole> _rm;

    public UsersController(UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
    {
        _um = um;
        _rm = rm;
    }

    private List<RoleCheckbox> AllRoles(IEnumerable<string>? selected = null) =>
        _rm.Roles.OrderBy(r => r.Name)
            .Select(r => new RoleCheckbox
            {
                Name = r.Name!,
                Selected = selected != null && selected.Contains(r.Name!)
            }).ToList();

    // ---------- LIST ----------
    public async Task<IActionResult> Index(string? search)
    {
        var query = _um.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Email!.Contains(search) || u.FullName.Contains(search));

        var list = new List<UserListVm>();
        foreach (var u in await query.OrderBy(u => u.Email).ToListAsync())
        {
            list.Add(new UserListVm
            {
                Id = u.Id,
                FullName = u.FullName ?? "",
                Email = u.Email ?? "",
                PhoneNumber = u.PhoneNumber,
                Roles = (await _um.GetRolesAsync(u)).ToList(),
                IsLocked = await _um.IsLockedOutAsync(u)
            });
        }

        ViewBag.Search = search;
        return View(list);
    }

    // ---------- CREATE ----------
    [HttpGet]
    public IActionResult Create() =>
        View(new CreateUserVm { Roles = AllRoles(new[] { "Customer" }) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            FullName = vm.FullName,
            PhoneNumber = vm.PhoneNumber,
            EmailConfirmed = true   // admin-created users don't need email confirmation
        };

        var result = await _um.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return View(vm);
        }

        var selected = vm.Roles.Where(r => r.Selected).Select(r => r.Name).ToList();
        if (selected.Any()) await _um.AddToRolesAsync(user, selected);

        TempData["Success"] = "User created.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- EDIT ----------
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _um.FindByIdAsync(id);
        if (user == null) return NotFound();

        var userRoles = await _um.GetRolesAsync(user);
        return View(new EditUserVm
        {
            Id = user.Id,
            FullName = user.FullName ?? "",
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber,
            Roles = AllRoles(userRoles)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _um.FindByIdAsync(vm.Id);
        if (user == null) return NotFound();

        var wanted = vm.Roles.Where(r => r.Selected).Select(r => r.Name).ToList();

        // an admin can't remove their own Admin role
        if (user.Id == _um.GetUserId(User) && !wanted.Contains("Admin"))
        {
            ModelState.AddModelError("", "You cannot remove the Admin role from your own account.");
            return View(vm);
        }

        if (user.Email != vm.Email)
        {
            var r1 = await _um.SetEmailAsync(user, vm.Email);
            var r2 = await _um.SetUserNameAsync(user, vm.Email);
            if (!r1.Succeeded || !r2.Succeeded)
            {
                foreach (var e in r1.Errors.Concat(r2.Errors))
                    ModelState.AddModelError("", e.Description);
                return View(vm);
            }
        }

        user.FullName = vm.FullName;
        user.PhoneNumber = vm.PhoneNumber;

        var update = await _um.UpdateAsync(user);
        if (!update.Succeeded)
        {
            foreach (var e in update.Errors) ModelState.AddModelError("", e.Description);
            return View(vm);
        }

        var current = await _um.GetRolesAsync(user);
        await _um.RemoveFromRolesAsync(user, current.Except(wanted));
        await _um.AddToRolesAsync(user, wanted.Except(current));

        TempData["Success"] = "User updated.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- LOCK / UNLOCK ----------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await _um.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (user.Id == _um.GetUserId(User))
        {
            TempData["Error"] = "You cannot lock your own account.";
            return RedirectToAction(nameof(Index));
        }

        await _um.SetLockoutEnabledAsync(user, true);

        if (await _um.IsLockedOutAsync(user))
        {
            await _um.SetLockoutEndDateAsync(user, null);
            await _um.ResetAccessFailedCountAsync(user);
            TempData["Success"] = $"{user.Email} unlocked.";
        }
        else
        {
            await _um.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            await _um.UpdateSecurityStampAsync(user);   // logs out active sessions
            TempData["Success"] = $"{user.Email} locked.";
        }

        return RedirectToAction(nameof(Index));
    }
}