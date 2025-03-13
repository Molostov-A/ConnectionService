using System.ComponentModel.DataAnnotations;

namespace WebConsumer.Data.Models;

public class IpAddress
{
    [Key]
    public long Id { get; set; }

    [MaxLength(45)]
    [Required]
    public string Address { get; set; }

    [MaxLength(15)]
    [Required]
    public string Protocol { get; set; }
}
