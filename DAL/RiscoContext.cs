namespace DAL
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class RiscoContext : DbContext
    {
        public RiscoContext()
            : base("name=RiscoContextQA")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<RiscoContext, DAL.Migrations.Configuration>());
            Configuration.ProxyCreationEnabled = false;
            this.Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<Application> Applications { get; set; }
        public virtual DbSet<PaymentCard> PaymentCards { get; set; }
        public virtual DbSet<ErrorLog> ErrorLogs { get; set; }
        public virtual DbSet<ForgotPasswordToken> ForgotPasswordTokens { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserRatings> UserRatings { get; set; }
        public virtual DbSet<AppRatings> AppRatings { get; set; }

        public virtual DbSet<ContactUs> ContactUs { get; set; }
        public virtual DbSet<UserDevice> UserDevices { get; set; }

        public virtual DbSet<RefreshTokens> RefreshTokens { get; set; }
        public virtual DbSet<Settings> Settings { get; set; }

        public virtual DbSet<UserSubscriptions> UserSubscriptions { get; set; }
        public virtual DbSet<AdminNotifications> AdminNotifications { get; set; }
        public virtual DbSet<AdminTokens> AdminTokens { get; set; }
        public virtual DbSet<VerifyNumberCodes> VerifyNumberCodes { get; set; }
        public virtual DbSet<TurnOffNotification> TurnOffNotifications { get; set; }
        public virtual DbSet<Franchise> Franchise { get; set; }
        public virtual DbSet<Docking> Docking { get; set; }
        public virtual DbSet<OutBoundCalls> OutBoundCalls { get; set; }
        public virtual DbSet<UploadedFiles> UploadedFiles { get; set; }
        public virtual DbSet<OutBoundCallDialPlan> OutBoundCallDialPlan { get; set; }
        public virtual DbSet<Contexts> Contexts { get; set; }
        public virtual DbSet<ExcelFile> ExcelFile { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>()
              .HasMany(x => x.ExcelFile)
              .WithRequired(x => x.User)
              .HasForeignKey(x => x.UserId)
              .WillCascadeOnDelete(false);


            modelBuilder.Entity<OutBoundCallDialPlan>()
              .HasMany(x => x.OutBoundCalls)
              .WithOptional(x => x.OutBoundCallDialPlan)
              .HasForeignKey(x => x.OutBoundCallDialPlan_Id)
              .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
              .HasMany(x => x.Contexts)
              .WithRequired(x => x.User)
              .HasForeignKey(x => x.User_Id)
              .WillCascadeOnDelete(false);

            modelBuilder.Entity<Admin>()
              .HasMany(x => x.Docking)
              .WithRequired(x => x.Admin)
              .HasForeignKey(x => x.Admin_Id)
              .WillCascadeOnDelete(false);

            modelBuilder.Entity<Franchise>()
              .HasMany(x => x.OutBoundCalls)
              .WithOptional(x => x.Franchise)
              .HasForeignKey(x => x.Franchise_Id)
              .WillCascadeOnDelete(false);


            modelBuilder.Entity<User>()
              .HasMany(x => x.VerifyNumberCodes)
              .WithRequired(x => x.User)
              .HasForeignKey(x => x.User_Id)
              .WillCascadeOnDelete(false);


            modelBuilder.Entity<Franchise>()
              .HasMany(x => x.Admins)
              .WithOptional(x => x.Franchise)
              .HasForeignKey(x => x.Franchise_Id)
              .WillCascadeOnDelete(false);


            modelBuilder.Entity<Admin>()
                .HasMany(x => x.AdminTokens)
                .WithRequired(x => x.Admin)
                .HasForeignKey(x => x.Admin_Id)
                .WillCascadeOnDelete(false);



            modelBuilder.Entity<AdminNotifications>()
               .HasMany(e => e.Notifications)
               .WithOptional(e => e.AdminNotification)
               .HasForeignKey(e => e.AdminNotification_Id)
               .WillCascadeOnDelete(false);



            modelBuilder.Entity<User>()
                .HasMany(x => x.UserSubscriptions)
                .WithRequired(x => x.User)
                .HasForeignKey(x => x.User_Id)
                .WillCascadeOnDelete(false);



            //modelBuilder.Entity<Order_Items>

            //modelBuilder.Entity<Store>()
            //    .HasMany(e => e.StoreDeliveryHours)
            //    .WithRequired(e => e.Store)
            //    .HasForeignKey(e => e.Store_Id)
            //    .WillCascadeOnDelete(false);


            //modelBuilder.Entity<StoreDeliveryHours>()
            //    .HasRequired(s => s.Store)
            //    .WithOptional(ad => ad.StoreDeliveryHours);



            modelBuilder.Entity<User>()
                .HasMany(e => e.UserDevices)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.User_Id)
                .WillCascadeOnDelete(false);



            modelBuilder.Entity<User>()
                .HasMany(e => e.UserRatings)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.User_ID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
              .HasMany(e => e.AppRatings)
              .WithRequired(e => e.User)
              .HasForeignKey(e => e.User_ID)
              .WillCascadeOnDelete(false);


            modelBuilder.Entity<User>()
                .HasMany(e => e.Notifications)
                .WithOptional(e => e.User)
                .HasForeignKey(e => e.User_ID)
                .WillCascadeOnDelete(false);



            modelBuilder.Entity<User>()
                .Property(e => e.FullName)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.PaymentCards)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.User_ID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.ForgotPasswordTokens)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.User_ID)
                .WillCascadeOnDelete(false);



            modelBuilder.Entity<User>()
                .HasMany(e => e.TurnOffNotifications)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.User_Id)
                .WillCascadeOnDelete(false);


        }
    }
}
