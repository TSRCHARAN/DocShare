using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace DocumentsProject.Core.Domain.Models;

public partial class DocumentdbContext : DbContext
{
    public DocumentdbContext()
    {
    }

    public DocumentdbContext(DbContextOptions<DocumentdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Doc> Docs { get; set; }

    public virtual DbSet<Otptable> Otptables { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;user=root;password=root;database=documentdb", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.37-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Doc>(entity =>
        {
            entity.HasKey(e => new { e.DocId, e.DocumentCreatedBy })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("docs");

            entity.Property(e => e.DocId)
                .HasMaxLength(29)
                .HasColumnName("docId");
            entity.Property(e => e.DocumentCreatedBy)
                .HasMaxLength(29)
                .HasColumnName("documentCreatedBy");
            entity.Property(e => e.Document).HasColumnName("document");
            entity.Property(e => e.DocumentName)
                .HasMaxLength(100)
                .HasColumnName("documentName");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(100)
                .HasColumnName("documentType");
            entity.Property(e => e.DocumentUploadedTime)
                .HasColumnType("datetime")
                .HasColumnName("documentUploadedTime");
        });

        modelBuilder.Entity<Otptable>(entity =>
        {
            entity.HasKey(e => e.UserEmail).HasName("PRIMARY");

            entity.ToTable("otptable");

            entity.Property(e => e.UserEmail)
                .HasMaxLength(29)
                .HasColumnName("userEmail");
            entity.Property(e => e.Otp)
                .HasColumnType("tinytext")
                .HasColumnName("otp");
            entity.Property(e => e.OtpCreatedTime)
                .HasColumnType("timestamp")
                .HasColumnName("otpCreatedTime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("users");

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(29)
                .HasColumnName("userEmail");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .HasColumnName("userName");
            entity.Property(e => e.UserPassword)
                .HasMaxLength(29)
                .HasColumnName("userPassword");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
