#!/bin/bash

# InfoCollector一键部署脚本
# 
# 使用示例：
# ./deploy.sh 192.168.1.100                    # 使用默认用户名ubuntu
# ./deploy.sh 192.168.1.100 root               # 使用root用户
# ./deploy.sh 10.0.0.5 ubuntu                  # 使用特定IP和用户名
# 
# 执行前确保：
# 1. 服务器已安装.NET 9运行时
# 2. 本地已配置SSH免密登录
# 3. 当前目录在项目根目录下

# 如果使用-e: 遇到错误立即退出，防止后续操作产生更多问题
# 重要：使用set +e而不是set -e
# 原因：SSH远程命令可能返回非0状态码（如pkill找不到进程返回1）
# 但这并不意味着有错误，不应该中断脚本执行
# 因此我们手动检查关键步骤，而不是让脚本自动退出
set +e

# 参数检查：确保至少提供了服务器IP
if [ $# -lt 1 ]; then
    echo "❌ 参数错误！"
    echo "使用方法: $0 服务器IP [用户名]"
    echo "示例: $0 192.168.1.100 ubuntu"
    exit 1
fi

# 读取命令行参数
SERVER_IP=$1          # 第1个参数：服务器IP地址
USER=${2:-ubuntu}     # 第2个参数：用户名，默认为ubuntu
REMOTE_DIR="/home/eli/myfile/project/tool/tool013_InfoCollector"   # 服务器上的部署目录
LOCAL_PUBLISH_DIR="./publish"     # 本地发布目录

echo "🚀 开始部署InfoCollector到服务器 $SERVER_IP"

# 1. 本地发布项目：编译并打包应用
# -c Release：使用发布配置（优化性能，不包含调试信息）
# -o：指定输出目录
echo "📦 正在发布项目..."
dotnet publish -c Release -o $LOCAL_PUBLISH_DIR
if [ $? -ne 0 ]; then          # $? 表示上一条命令的退出状态码
    echo "❌ 项目发布失败"
    exit 1                    # 非0表示失败
fi

# 2. 创建远程目录
echo "📁 创建远程目录..."
ssh $USER@$SERVER_IP "mkdir -p $REMOTE_DIR"

# 3. 上传文件
echo "⬆️  上传文件到服务器..."
scp -r $LOCAL_PUBLISH_DIR/* $USER@$SERVER_IP:$REMOTE_DIR/
if [ $? -ne 0 ]; then
    echo "❌ 文件上传失败"
    exit 1
fi

# 4. 上传生产配置
echo "⚙️  配置生产环境..."
scp ./appsettings.Production.json $USER@$SERVER_IP:$REMOTE_DIR/appsettings.json

# 5. 停止现有服务：确保不会让脚本中断
# 使用 || true 确保即使找不到进程也不会报错退出
echo "🛑 停止现有服务..."
ssh $USER@$SERVER_IP "pkill -f InfoCollectorAPI.dll 2>/dev/null || echo '没有找到运行中的服务'"
sleep 2

# 5. 启动后台服务
# 重要：SSH字符串中的变量需要正确转义
# 转义规则：
#   - \$! - 正确：让远程shell解析变量
#   - \$(...) - 正确：让远程shell解析命令替换
#   - $! - 错误：会被本地shell提前解析
echo "🔄 启动后台服务..."
ssh $USER@$SERVER_IP "
    cd $REMOTE_DIR
    # 使用生产环境配置
    export ASPNETCORE_ENVIRONMENT=Production
    nohup dotnet InfoCollectorAPI.dll --urls \"http://0.0.0.0:56560\" > logs/log.log 2>&1 &
    echo \$! > app.pid
    echo \"✅ 服务已启动，进程ID: \$(cat app.pid)\"
"

# 6. 等待服务启动
echo "⏳ 等待服务启动..."
sleep 5

# 7. 检查服务状态
echo "🔍 检查服务状态..."
RESPONSE=$(ssh $USER@$SERVER_IP "curl -s -o /dev/null -w \"%{http_code}\" http://localhost:56560/api/message || echo \"000\"")
if [ "$RESPONSE" = "200" ] || [ "$RESPONSE" = "405" ]; then
    echo "✅ 服务运行正常"
    echo "🌐 访问地址: http://$SERVER_IP:56560/api/message"
else
    echo "⚠️  服务可能未正常启动，请检查日志"
    echo "📋 查看日志命令: ssh $USER@$SERVER_IP 'tail -f $REMOTE_DIR/logs/log.log'"
fi

echo "🎉 部署完成！"
echo ""
echo "常用命令："
echo "查看日志: ssh $USER@$SERVER_IP 'tail -f $REMOTE_DIR/logs/log.log'"
echo "重启服务: ssh $USER@$SERVER_IP 'cd $REMOTE_DIR && pkill -f InfoCollectorAPI.dll && nohup dotnet InfoCollectorAPI.dll --urls \"http://0.0.0.0:56560\" > logs/log.log 2>&1 &'"
echo "停止服务: ssh $USER@$SERVER_IP 'pkill -f InfoCollectorAPI.dll'"
