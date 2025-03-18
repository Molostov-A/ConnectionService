using System.ComponentModel.DataAnnotations;

namespace ConnectionLogger.Data.Models;

public class IpAddress
{
    [Key]
    public long Id { get; set; }

    [MaxLength(45)]
    [Required]
    public required string Address { get; set; }

    [MaxLength(15)]
    [Required]
    public required string Protocol { get; set; }
}
