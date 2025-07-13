using Microsoft.EntityFrameworkCore;
using InfoCollectorAPI.Models;

namespace InfoCollectorAPI.Data;

public class InfoCollectorDbContext : DbContext
{
    public InfoCollectorDbContext(DbContextOptions<InfoCollectorDbContext> options) : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置Message实体
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.GroupOrUserName)
                .IsRequired()
                .HasMaxLength(500);
                
            entity.Property(e => e.MessageContent)
                .IsRequired();
                
            entity.Property(e => e.ReceivedDateTime)
                .IsRequired();

            // 创建索引以提高查询性能
            entity.HasIndex(e => e.ReceivedDateTime)
                .HasDatabaseName("IX_Messages_ReceivedDateTime");
                
            entity.HasIndex(e => e.GroupOrUserName)
                .HasDatabaseName("IX_Messages_GroupOrUserName");
        });
    }
}