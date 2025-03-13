using System.ComponentModel.DataAnnotations;

namespace WebConsumer.Data.Models;

public class User
{
    [Key]
    public long Id { get; set; }

    [MaxLength(45)]
    public string LastName { get; set; }

    [MaxLength(45)]
    public string FirstName { get; set; }
}
