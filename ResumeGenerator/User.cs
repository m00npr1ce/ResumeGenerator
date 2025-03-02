using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ResumeGenerator;

public partial class User
{
    [Key]
    public int Id { get; set; }

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    [NotMapped]
    public string? Password { get; set; }

    public DateTime? CreatedAt { get; set; }  = DateTime.UtcNow;

    public virtual ICollection<Resume> Resumes { get; set; } = new List<Resume>();

    public void SetPassword(string password)
    {
        var hasher = new PasswordHasher<User>();
        PasswordHash = hasher.HashPassword(this, password);
    }
}
