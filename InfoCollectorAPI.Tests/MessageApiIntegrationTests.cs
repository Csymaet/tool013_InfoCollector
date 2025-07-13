using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using InfoCollectorAPI.Models;

namespace InfoCollectorAPI.Tests;

/// <summary>
/// Message API 集成测试 - 向运行中的服务器发送请求测试完整功能
/// </summary>
public class MessageApiIntegrationTests
{
    private readonly HttpClient _client;
    private readonly string _baseUrl = "http://localhost:56560"; // 实际运行端口

    public MessageApiIntegrationTests()
    {
        _client = new HttpClient();
    }

    [Fact]
    public async Task SendWeChatMessage_ShouldSucceed()
    {
        // 发送一条普通的微信消息
        var message = new MessageRequest
        {
            GroupOrUserName = "工作群",
            MessageContent = "明天上午9点开会",
            ReceivedDateTime = DateTime.Now
        };

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/api/message", message);

        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("success", content);
        Assert.Contains("消息保存成功", content);
    }

    [Fact]
    public async Task SendMultipleMessages_AllShouldSucceed()
    {
        // 发送多条消息，模拟真实使用场景
        var messages = new[]
        {
            new MessageRequest { GroupOrUserName = "技术群", MessageContent = "代码审查完成", ReceivedDateTime = DateTime.Now },
            new MessageRequest { GroupOrUserName = "项目群", MessageContent = "进度更新", ReceivedDateTime = DateTime.Now },
            new MessageRequest { GroupOrUserName = "朋友", MessageContent = "周末聚餐", ReceivedDateTime = DateTime.Now }
        };

        foreach (var message in messages)
        {
            var response = await _client.PostAsJsonAsync($"{_baseUrl}/api/message", message);
            Assert.True(response.IsSuccessStatusCode, "每条消息都应该成功发送");
        }
    }

    [Fact]
    public async Task SendEmptyGroupName_ShouldFail()
    {
        // 测试错误情况 - 空群名
        var invalidMessage = new MessageRequest
        {
            GroupOrUserName = "",
            MessageContent = "测试消息",
            ReceivedDateTime = DateTime.Now
        };

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/api/message", invalidMessage);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendEmptyContent_ShouldFail()
    {
        // 测试错误情况 - 空消息内容
        var invalidMessage = new MessageRequest
        {
            GroupOrUserName = "测试群",
            MessageContent = "",
            ReceivedDateTime = DateTime.Now
        };

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/api/message", invalidMessage);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendLongMessage_ShouldWork()
    {
        // 测试长消息
        var longContent = string.Join("\n", Enumerable.Range(1, 50)
            .Select(i => $"这是第{i}行很长的消息内容，测试系统处理长文本的能力。"));

        var message = new MessageRequest
        {
            GroupOrUserName = "测试群",
            MessageContent = longContent,
            ReceivedDateTime = DateTime.Now
        };

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/api/message", message);

        Assert.True(response.IsSuccessStatusCode, "长消息应该能正常处理");
    }

    [Fact]
    public async Task SendChineseAndEmoji_ShouldWork()
    {
        // 测试中文和表情符号
        var message = new MessageRequest
        {
            GroupOrUserName = "🚀技术群",
            MessageContent = "测试中文消息😊，包含各种符号！@#￥%……&*（）——+",
            ReceivedDateTime = DateTime.Now
        };

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/api/message", message);

        Assert.True(response.IsSuccessStatusCode, "中文和表情符号应该能正常处理");
    }

    [Fact]
    public async Task SendNullRequest_ShouldFail()
    {
        // 测试发送null请求
        var content = new StringContent("null", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{_baseUrl}/api/message", content);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendInvalidJson_ShouldFail()
    {
        // 测试发送无效JSON
        var content = new StringContent("invalid json", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{_baseUrl}/api/message", content);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CheckResponseFormat_ShouldBeCorrect()
    {
        // 验证响应格式是否正确
        var message = new MessageRequest
        {
            GroupOrUserName = "格式测试群",
            MessageContent = "测试响应格式",
            ReceivedDateTime = DateTime.Now
        };

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/api/message", message);
        var content = await response.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        // 检查必要的字段
        Assert.True(root.TryGetProperty("success", out var successProp));
        Assert.True(successProp.GetBoolean());
        
        Assert.True(root.TryGetProperty("messageId", out var idProp));
        Assert.True(idProp.GetInt32() > 0);
        
        Assert.True(root.TryGetProperty("message", out var msgProp));
        Assert.Equal("消息保存成功", msgProp.GetString());
    }

    [Fact]
    public async Task ConcurrentRequests_ShouldAllSucceed()
    {
        // 测试并发请求
        var tasks = new List<Task<HttpResponseMessage>>();
        
        for (int i = 0; i < 5; i++)
        {
            var message = new MessageRequest
            {
                GroupOrUserName = $"并发测试群{i}",
                MessageContent = $"并发消息{i}",
                ReceivedDateTime = DateTime.Now
            };
            
            tasks.Add(_client.PostAsJsonAsync($"{_baseUrl}/api/message", message));
        }

        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            Assert.True(response.IsSuccessStatusCode, "并发请求都应该成功");
        }
    }
}