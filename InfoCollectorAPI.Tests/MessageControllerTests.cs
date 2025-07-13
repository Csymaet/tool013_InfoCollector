using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InfoCollectorAPI.Controllers;
using InfoCollectorAPI.Data;
using InfoCollectorAPI.Models;

namespace InfoCollectorAPI.Tests;

public class MessageControllerTests
{
    private InfoCollectorDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<InfoCollectorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new InfoCollectorDbContext(options);
    }

    private ILogger<MessageController> GetLogger()
    {
        return new LoggerFactory().CreateLogger<MessageController>();
    }

    [Fact]
    public async Task Post_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var controller = new MessageController(logger, context);
        var request = new MessageRequest
        {
            GroupOrUserName = "测试群",
            MessageContent = "测试消息",
            ReceivedDateTime = DateTime.Now
        };

        // Act
        var result = await controller.Post(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
        
        // 验证数据库中确实保存了消息
        var savedMessage = await context.Messages.FirstOrDefaultAsync();
        Assert.NotNull(savedMessage);
        Assert.Equal("测试群", savedMessage.GroupOrUserName);
        Assert.Equal("测试消息", savedMessage.MessageContent);
    }

    [Fact]
    public async Task Post_NullRequest_ReturnsBadRequest()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var controller = new MessageController(logger, context);

        // Act
        var result = await controller.Post(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("请求数据不能为空", badRequestResult.Value);
    }

    [Fact]
    public async Task Post_EmptyGroupOrUserName_ReturnsBadRequest()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var controller = new MessageController(logger, context);
        var request = new MessageRequest
        {
            GroupOrUserName = "",
            MessageContent = "测试消息",
            ReceivedDateTime = DateTime.Now
        };

        // Act
        var result = await controller.Post(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("群名/用户名和消息内容不能为空", badRequestResult.Value);
    }

    [Fact]
    public async Task Post_EmptyMessageContent_ReturnsBadRequest()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var controller = new MessageController(logger, context);
        var request = new MessageRequest
        {
            GroupOrUserName = "测试群",
            MessageContent = "",
            ReceivedDateTime = DateTime.Now
        };

        // Act
        var result = await controller.Post(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("群名/用户名和消息内容不能为空", badRequestResult.Value);
    }

    [Fact]
    public async Task Post_ValidRequest_SavesMessageToDatabase()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var controller = new MessageController(logger, context);
        var testDateTime = new DateTime(2025, 7, 13, 10, 30, 0);
        var request = new MessageRequest
        {
            GroupOrUserName = "工作群",
            MessageContent = "重要通知：会议推迟到下午3点",
            ReceivedDateTime = testDateTime
        };

        // Act
        await controller.Post(request);

        // Assert
        var savedMessage = await context.Messages.FirstOrDefaultAsync();
        Assert.NotNull(savedMessage);
        Assert.Equal("工作群", savedMessage.GroupOrUserName);
        Assert.Equal("重要通知：会议推迟到下午3点", savedMessage.MessageContent);
        Assert.Equal(testDateTime, savedMessage.ReceivedDateTime);
        Assert.True(savedMessage.Id > 0);
    }

    [Fact]
    public async Task Post_MultipleRequests_SavesAllMessages()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var controller = new MessageController(logger, context);
        
        var request1 = new MessageRequest
        {
            GroupOrUserName = "群1",
            MessageContent = "消息1",
            ReceivedDateTime = DateTime.Now
        };
        
        var request2 = new MessageRequest
        {
            GroupOrUserName = "群2", 
            MessageContent = "消息2",
            ReceivedDateTime = DateTime.Now
        };

        // Act
        await controller.Post(request1);
        await controller.Post(request2);

        // Assert
        var messageCount = await context.Messages.CountAsync();
        Assert.Equal(2, messageCount);
        
        var messages = await context.Messages.ToListAsync();
        Assert.Contains(messages, m => m.GroupOrUserName == "群1" && m.MessageContent == "消息1");
        Assert.Contains(messages, m => m.GroupOrUserName == "群2" && m.MessageContent == "消息2");
    }
}