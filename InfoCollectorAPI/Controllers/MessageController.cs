using Microsoft.AspNetCore.Mvc;
using InfoCollectorAPI.Models;
using InfoCollectorAPI.Data;

namespace InfoCollectorAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;
    private readonly InfoCollectorDbContext _context;

    public MessageController(ILogger<MessageController> logger, InfoCollectorDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] MessageRequest request)
    {
        try
        {
            // 验证请求数据
            if (request == null)
            {
                return BadRequest("请求数据不能为空");
            }

            if (string.IsNullOrEmpty(request.GroupOrUserName) || string.IsNullOrEmpty(request.MessageContent))
            {
                return BadRequest("群名/用户名和消息内容不能为空");
            }

            // 记录接收到的消息
            _logger.LogInformation("接收消息: 来源={GroupOrUserName}, 内容={MessageContent}, 时间={ReceivedDateTime}", 
                request.GroupOrUserName, request.MessageContent, request.ReceivedDateTime);

            // 创建Message实体并保存到数据库
            var message = new Message
            {
                GroupOrUserName = request.GroupOrUserName,
                MessageContent = request.MessageContent,
                ReceivedDateTime = request.ReceivedDateTime
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation("消息已成功保存到数据库，ID: {MessageId}", message.Id);

            return Ok(new { 
                success = true, 
                messageId = message.Id,
                message = "消息保存成功" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存消息到数据库时发生错误");
            return StatusCode(500, new { 
                success = false, 
                message = "服务器内部错误，请稍后重试" 
            });
        }
    }
}