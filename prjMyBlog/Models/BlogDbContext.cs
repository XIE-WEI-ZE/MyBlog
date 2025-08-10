using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace prjMyBlog.Models;

public partial class BlogDbContext : DbContext
{
    public BlogDbContext()
    {
    }

    public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TBlogPost> TBlogPosts { get; set; }

    public virtual DbSet<TCategory> TCategories { get; set; }

    public virtual DbSet<TComment> TComments { get; set; }

    public virtual DbSet<TPostImage> TPostImages { get; set; }

    public virtual DbSet<TPostTag> TPostTags { get; set; }

    public virtual DbSet<TTag> TTags { get; set; }

    public virtual DbSet<TUser> TUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=dbBlog;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TBlogPost>(entity =>
        {
            entity.HasKey(e => e.FPostId);

            entity.ToTable("tBlogPosts");

            entity.Property(e => e.FPostId).HasColumnName("fPostId");
            entity.Property(e => e.FAuthorId).HasColumnName("fAuthorId");
            entity.Property(e => e.FCategoryId).HasColumnName("fCategoryId");
            entity.Property(e => e.FContent).HasColumnName("fContent");
            entity.Property(e => e.FCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("fCreatedAt");
            entity.Property(e => e.FTitle)
                .HasMaxLength(200)
                .HasColumnName("fTitle");
            entity.Property(e => e.FUpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("fUpdatedAt");
        });

        modelBuilder.Entity<TCategory>(entity =>
        {
            entity.HasKey(e => e.FCategoryId);

            entity.ToTable("tCategories");

            entity.Property(e => e.FCategoryId).HasColumnName("fCategoryId");
            entity.Property(e => e.FName)
                .HasMaxLength(50)
                .HasColumnName("fName");
        });

        modelBuilder.Entity<TComment>(entity =>
        {
            entity.HasKey(e => e.FCommentId);

            entity.ToTable("tComments");

            entity.Property(e => e.FCommentId).HasColumnName("fCommentId");
            entity.Property(e => e.FContent).HasColumnName("fContent");
            entity.Property(e => e.FCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("fCreatedAt");
            entity.Property(e => e.FPostId).HasColumnName("fPostId");
            entity.Property(e => e.FUserId).HasColumnName("fUserId");
        });

        modelBuilder.Entity<TPostImage>(entity =>
        {
            entity.HasKey(e => e.FImageId);

            entity.ToTable("tPostImages");

            entity.Property(e => e.FImageId).HasColumnName("fImageId");
            entity.Property(e => e.FImagePath)
                .HasMaxLength(200)
                .HasColumnName("fImagePath");
            entity.Property(e => e.FPostId).HasColumnName("fPostId");
            entity.Property(e => e.FSortOrder).HasColumnName("fSortOrder");
            entity.Property(e => e.FUploadedAt)
                .HasColumnType("datetime")
                .HasColumnName("fUploadedAt");
        });

        modelBuilder.Entity<TPostTag>(entity =>
        {
            entity.HasKey(e => e.FPostTagId);

            entity.ToTable("tPostTags");

            entity.Property(e => e.FPostTagId).HasColumnName("fPostTagId");
            entity.Property(e => e.FPostId).HasColumnName("fPostId");
            entity.Property(e => e.FTagId).HasColumnName("fTagId");
        });

        modelBuilder.Entity<TTag>(entity =>
        {
            entity.HasKey(e => e.FTagId);

            entity.ToTable("tTags");

            entity.Property(e => e.FTagId).HasColumnName("fTagId");
            entity.Property(e => e.FName)
                .HasMaxLength(50)
                .HasColumnName("fName");
        });

        modelBuilder.Entity<TUser>(entity =>
        {
            entity.HasKey(e => e.FUserId);

            entity.ToTable("tUsers");

            entity.Property(e => e.FUserId).HasColumnName("fUserId");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.FBio)
                .HasMaxLength(500)
                .HasColumnName("fBio");
            entity.Property(e => e.FEmail)
                .HasMaxLength(100)
                .HasColumnName("fEmail");
            entity.Property(e => e.FExternalId)
                .HasMaxLength(100)
                .HasColumnName("fExternalId");
            entity.Property(e => e.FIsEnabled).HasColumnName("fIsEnabled");
            entity.Property(e => e.FLoginProvider)
                .HasMaxLength(50)
                .HasColumnName("fLoginProvider");
            entity.Property(e => e.FPasswordHash).HasColumnName("fPasswordHash");
            entity.Property(e => e.FPasswordSalt).HasColumnName("fPasswordSalt");
            entity.Property(e => e.FPhotoPath)
                .HasMaxLength(200)
                .HasColumnName("fPhotoPath");
            entity.Property(e => e.FUsername)
                .HasMaxLength(50)
                .HasColumnName("fUsername");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
