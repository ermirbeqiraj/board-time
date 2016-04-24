using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using DbModel;
namespace MvcToDo.Persistence
{
    public class ModelContext : DbContext
    {
        public ModelContext()
            : base("name=ModelContext")
        {
            Configuration.LazyLoadingEnabled = false;
            //Database.SetInitializer<ModelContext>(new DropCreateDatabaseIfModelChanges<ModelContext>());
            Database.SetInitializer<ModelContext>(new CreateDatabaseIfNotExists<ModelContext>());
            //Database.SetInitializer<ModelContext>(null);
            //Database.SetInitializer<ModelContext>(new ModelContextInitializer());
            //Database.Initialize(true);
        }

        public virtual DbSet<Comments> Comments { get; set; }
        public virtual DbSet<Files> Files { get; set; }
        public virtual DbSet<Project> Project { get; set; }
        public virtual DbSet<TaskAssigned> TaskAssigned { get; set; }
        public virtual DbSet<TaskFiles> TaskFiles { get; set; }
        public virtual DbSet<TaskItem> TaskItem { get; set; }
        public virtual DbSet<TaskMark> TaskMark { get; set; }
        public virtual DbSet<TaskCategory> TaskCategory { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<CustomerUser> CustomerUser { get; set; }
        public virtual DbSet<Chat> Chat { get; set; }
        public virtual DbSet<Conversation> Conversation { get; set; }
        public virtual DbSet<TaskLifecycle> TaskLifecycle { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comments>()
                .Property(e => e.Author)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Files>()
                .Property(e => e.Author)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Files>()
                .HasMany(e => e.TaskFiles)
                .WithRequired(e => e.Files)
                .HasForeignKey(e => e.FileId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Customer>()
                .HasMany(e => e.CustomerUser)
                .WithRequired(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Project>()
                .Property(e => e.Author)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Project>()
                .HasMany(e => e.TaskItem)
                .WithRequired(e => e.Project)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<TaskAssigned>()
                .Property(e => e.Author)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<TaskAssigned>()
                .Property(e => e.AssignedTo)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<TaskItem>()
                .Property(e => e.Author)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<TaskItem>()
                .HasMany(e => e.Comments)
                .WithOptional(e => e.TaskItem)
                .HasForeignKey(e => e.TaskId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<TaskItem>()
                .HasMany(e => e.TaskAssigned)
                .WithRequired(e => e.TaskItem)
                .HasForeignKey(e => e.TaskId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<TaskItem>()
                .HasMany(e => e.TaskFiles)
                .WithRequired(e => e.TaskItem)
                .HasForeignKey(e => e.TaskId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<TaskMark>()
                .HasMany(e => e.TaskItem)
                .WithRequired(e => e.TaskMark)
                .HasForeignKey(e => e.Mark)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TaskCategory>()
                .HasMany(e => e.TaskItem)
                .WithRequired(e => e.TaskCategory)
                .HasForeignKey(e => e.Category)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Conversation>()
                            .Property(e => e.Usr_1)
                            .IsFixedLength()
                            .IsUnicode(false);

            modelBuilder.Entity<Conversation>()
                .Property(e => e.Usr_2)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Conversation>()
                .HasMany(e => e.Chat)
                .WithRequired(e => e.Conversation)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TaskItem>()
                .HasMany(e => e.TaskLifecycle)
                .WithRequired(e => e.TaskItem)
                .HasForeignKey(e => e.TaskId);

            modelBuilder.Entity<TaskLifecycle>()
                .Property(e => e.Actor)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<TaskMark>()
                .HasMany(e => e.TaskItem)
                .WithRequired(e => e.TaskMark)
                .HasForeignKey(e => e.Mark)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TaskMark>()
                .HasMany(e => e.TaskLifecycle)
                .WithRequired(e => e.TaskMark)
                .HasForeignKey(e => e.MarkFromId);

            modelBuilder.Entity<TaskMark>()
                .HasMany(e => e.TaskLifecycle1)
                .WithRequired(e => e.TaskMark1)
                .HasForeignKey(e => e.MarkToId)
                .WillCascadeOnDelete(false);

        }
    }
}