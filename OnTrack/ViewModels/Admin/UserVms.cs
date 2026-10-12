using System.ComponentModel.DataAnnotations;

namespace OnTrack.ViewModels.Admin;

public class RoleCheckbox
{
    public string Name { get; set; } = "";
    public bool Selected { get; set; }
}

public class UserListVm
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsLocked { get; set; }
}

public class CreateUserVm
{
    [Required, Display(Name = "Full name")]
    public string FullName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Phone, Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    public List<RoleCheckbox> Roles { get; set; } = new();
}

public class EditUserVm
{
    public string Id { get; set; } = "";

    [Required, Display(Name = "Full name")]
    public string FullName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Phone, Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    public List<RoleCheckbox> Roles { get; set; } = new();
}