using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeGenerator;

public partial class Resume
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("User")]
    public int? UserId { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User? User { get; set; }
}
