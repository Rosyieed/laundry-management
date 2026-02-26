using System.ComponentModel.DataAnnotations;

public class User
{
    public int id { get; set; }

    [Required]
    public required string name { get; set; }

    [Required]
    public required string username { get; set; }

    [Required]
    public required string phone_number { get; set; }

    [Required]
    public required string password_hash { get; set; }

    [Required]
    public required string role { get; set; }

    public string? image_path { get; set; }
}