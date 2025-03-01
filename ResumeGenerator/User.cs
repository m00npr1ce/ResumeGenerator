using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeGenerator;

public partial class User
{
    [Key]
    public int Id { get; set; }

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }  = DateTime.UtcNow;

    public virtual ICollection<Resume> Resumes { get; set; } = new List<Resume>();
}
