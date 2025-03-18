using System.ComponentModel.DataAnnotations;

namespace ConnectionLogger.Data.Models;

public class User
{
    [Key]
    public long Id { get; set; }

    [MaxLength(45)]
    public required string LastName { get; set; }

    [MaxLength(45)]
    public required string FirstName { get; set; }
}
