# 数据库设计

## 消息表 (Messages)

用于存储从Tasker转发的微信消息数据。

### 表结构

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|----------|------|------|
| Id | INTEGER | PRIMARY KEY AUTOINCREMENT | 主键，消息ID |
| GroupOrUserName | TEXT | NOT NULL | 群名或用户名 |
| MessageContent | TEXT | NOT NULL | 消息内容 |
| ReceivedDateTime | DATETIME | NOT NULL | 接收时间 |

### SQL DDL (SQLite版本)

```sql
CREATE TABLE Messages (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    GroupOrUserName TEXT NOT NULL,
    MessageContent TEXT NOT NULL, 
    ReceivedDateTime DATETIME NOT NULL
);

-- 创建索引以提高查询性能
CREATE INDEX IX_Messages_ReceivedDateTime ON Messages(ReceivedDateTime);
CREATE INDEX IX_Messages_GroupOrUserName ON Messages(GroupOrUserName);
```

### 字段说明

- **Id**: 自增主键，确保每条消息记录的唯一性
- **GroupOrUserName**: 存储群名或发送者用户名，用于区分消息来源
- **MessageContent**: 存储完整的消息内容
- **ReceivedDateTime**: Tasker接收到消息的时间戳

### 数据示例

```json
{
  "Id": 1,
  "GroupOrUserName": "项目讨论群",
  "MessageContent": "今天的会议推迟到下午3点",
  "ReceivedDateTime": "2024-01-15T10:30:00"
}
```

### 设计决策

1. **单表设计**: 所有群/用户的消息存储在同一张表中，通过GroupOrUserName字段区分
2. **索引策略**: 针对最常用的查询条件（时间范围、群名/用户名）创建索引
3. **数据量预估**: 每天1000条消息，单表可支持数年的数据存储