namespace InfoCollectorAPI.Models;

public class MessageRequest
{
    public string GroupOrUserName { get; set; } = string.Empty;
    public string MessageContent { get; set; } = string.Empty;
    public DateTime ReceivedDateTime { get; set; }  // 直接用DateTime，框架自动解析ISO格式
}
