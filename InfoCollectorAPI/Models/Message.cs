using System.ComponentModel.DataAnnotations;

namespace InfoCollectorAPI.Models;

public class Message
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string GroupOrUserName { get; set; } = string.Empty;
    
    [Required]
    public string MessageContent { get; set; } = string.Empty;
    
    [Required]
    public DateTime ReceivedDateTime { get; set; }
}