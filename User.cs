using System;
using System.Collections.Generic;

namespace MeatShop.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsLocked { get; set; }

    public int FailedAttempts { get; set; }
}
