using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        // 不需要服务端主动向客户端发起请求（如采样），使用无状态模式
        options.Stateless = true;
    })
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();

/// <summary>
/// 对话查询工具：查询某人过去在某个时间、某个地点与某人的对话内容。
/// 数据来源为模拟数据，实际项目中可替换为数据库/会议记录服务。
/// </summary>
[McpServerToolType]
public static class ConversationQueryTool
{
    [McpServerTool(Name = "query_conversation")]
    [Description("查询某人过去在某个时间、某个地点与另一个人的面对面对话内容。")]
    public static string QueryConversation(
        [Description("要查询的人的姓名")] string person,
        [Description("对话对象的姓名")] string otherPerson,
        [Description("对话发生的日期，例如 2026-09-15")] DateTime date,
        [Description("对话发生的地点，例如 3楼会议室")] string location)
    {
        var record = ConversationStore.Records.FirstOrDefault(r =>
            r.Date.Date == date.Date &&
            r.Participants.Any(p => p.Contains(person, StringComparison.OrdinalIgnoreCase)) &&
            r.Participants.Any(p => p.Contains(otherPerson, StringComparison.OrdinalIgnoreCase)) &&
            r.Location.Contains(location, StringComparison.OrdinalIgnoreCase));

        if (record is null)
        {
            return $"未找到 {date:yyyy-MM-dd} {person} 与 {otherPerson} 在 {location} 的对话记录。";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"时间：{record.Date:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"地点：{record.Location}");
        sb.AppendLine($"参与人：{string.Join("、", record.Participants)}");
        sb.AppendLine("对话内容：");
        foreach (var message in record.Messages)
        {
            sb.AppendLine($"[{message.Time:HH:mm}] {message.Speaker}：{message.Content}");
        }

        return sb.ToString();
    }
    
    [McpServerTool(Name = "get_user_info")]
    [Description("查询某人个人信息。")]
    public static string GetUserInfo(
        [Description("要查询的人的姓名")] string Name)
    {
       return $"{Name} was a software engineer with experience in C# and .NET Core.";   
    }
}

public sealed record ConversationMessage(DateTime Time, string Speaker, string Content);

public sealed record ConversationRecord(
    DateTime Date,
    string Location,
    IReadOnlyList<string> Participants,
    IReadOnlyList<ConversationMessage> Messages);

public static class ConversationStore
{
    public static readonly IReadOnlyList<ConversationRecord> Records =
    [
        new(
            new DateTime(2026, 9, 15, 10, 0, 0),
            "公司3楼会议室",
            ["张伟", "李娜"],
            [
                new(new DateTime(2026, 9, 15, 10, 0, 0), "张伟", "李娜，新版本的接口文档你这边整理得怎么样了？"),
                new(new DateTime(2026, 9, 15, 10, 1, 0), "李娜", "已经完成了大部分，还差支付模块，预计明天能给你。"),
                new(new DateTime(2026, 9, 15, 10, 3, 0), "张伟", "好的，那我们周五一起对一下联调进度。"),
            ]),
        new(
            new DateTime(2026, 9, 16, 14, 30, 0),
            "星巴克国贸店",
            ["王强", "陈静"],
            [
                new(new DateTime(2026, 9, 16, 14, 30, 0), "王强", "客户对这次的报价还有什么反馈吗？"),
                new(new DateTime(2026, 9, 16, 14, 32, 0), "陈静", "整体满意，就是希望交付时间能提前两周。"),
            ]),
    ];
}
