using System;
using MediaAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MediaAPI
{
    public partial class MediaDBContext : DbContext
    {
        public MediaDBContext()
        {
            this.ChangeTracker.LazyLoadingEnabled = false;
        }

        public MediaDBContext(DbContextOptions<MediaDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Favourite> Favourites { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<GroupGenre> GroupGenres { get; set; }
        public virtual DbSet<GroupInstrument> GroupInstruments { get; set; }
        public virtual DbSet<GroupMember> GroupMembers { get; set; }
        public virtual DbSet<MediaFile> MediaFiles { get; set; }
        public virtual DbSet<MediaGenre> MediaGenres { get; set; }
        public virtual DbSet<MediaInstrument> MediaInstruments { get; set; }
        public virtual DbSet<MusicFile> MusicFiles { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<VideoFile> VideoFiles { get; set; }
        public virtual DbSet<GroupFavourite> GroupFavourites { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CS_AS");

            modelBuilder.Entity<Favourite>(entity =>
            {
                entity.HasOne(d => d.Media)
                    .WithMany(p => p.Favourites)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("FK_Favourites_MediaFiles");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Favourites)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Favourites_Users");
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.GroupName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ImagePath).HasMaxLength(50);

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.Groups)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Groups_Users");
            });

            modelBuilder.Entity<GroupGenre>(entity =>
            {
                entity.HasOne(d => d.Group)
                    .WithMany(p => p.GroupGenres)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("FK_GroupGenres_Groups");
            });

            modelBuilder.Entity<GroupInstrument>(entity =>
            {
                entity.HasOne(d => d.Group)
                    .WithMany(p => p.GroupInstruments)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("FK_GroupInstruments_Groups");
            });

            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.HasOne(d => d.Group)
                    .WithMany(p => p.GroupMembers)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("FK_GroupMembers_Groups");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.GroupMembers)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_GroupMembers_Users");
            });

            modelBuilder.Entity<MediaFile>(entity =>
            {
                entity.HasKey(e => e.MediaId);

                entity.Property(e => e.MediaName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.MediaPath)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Author)
                    .WithMany(p => p.MediaFiles)
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_MediaFiles_Users");
            });

            modelBuilder.Entity<MediaGenre>(entity =>
            {
                entity.HasOne(d => d.Media)
                    .WithMany(p => p.MediaGenres)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("FK_MediaGenres_MediaFiles");
            });

            modelBuilder.Entity<MediaInstrument>(entity =>
            {
                entity.HasOne(d => d.Media)
                    .WithMany(p => p.MediaInstruments)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("FK_MediaInstruments_MediaFiles");
            });

            modelBuilder.Entity<MusicFile>(entity =>
            {
                entity.HasKey(e => e.MusicFileId);

                entity.HasOne(e => e.MediaFile)
                    .WithOne(e => e.MusicFile)
                    .HasForeignKey<MusicFile>(e => e.MediaFileId);

                entity.Property(e => e.Artist).HasMaxLength(50);

                entity.Property(e => e.Cover).HasMaxLength(50);
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.SubscriptionProviders)
                    .HasForeignKey(d => d.ProviderId)
                    .HasConstraintName("FK_Subscriptions_Users1");

                entity.HasOne(d => d.Subscriber)
                    .WithMany(p => p.SubscriptionSubscribers)
                    .HasForeignKey(d => d.SubscriberId)
                    .HasConstraintName("FK_Subscriptions_Users");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.AvatarPath).HasMaxLength(50);

                entity.Property(e => e.DateOfBirth).HasColumnType("datetime");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<VideoFile>(entity =>
            {
                entity.HasKey(e => e.VideoFileId);

                entity.HasOne(e => e.MediaFile)
                    .WithOne(e => e.VideoFile)
                    .HasForeignKey<VideoFile>(e => e.MediaFileId);

                entity.Property(e => e.Preview)
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GroupFavourite>(entity => 
            {
                entity.HasKey(e => e.GroupFavouriteId);

                entity.HasOne(e => e.Group)
                    .WithMany(e => e.GroupFavourites)
                    .HasForeignKey(e => e.GroupId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_GroupFavourites_Groups");

                entity.HasOne(e => e.MediaFile)
                    .WithMany(e => e.GroupFavourites)
                    .HasForeignKey(e => e.MediaFileId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_GroupFavourites_MediaFiles");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
