using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace VendorX.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Shop> Shops { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ShopCustomer> ShopCustomers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<POSTransaction> POSTransactions { get; set; }
        public DbSet<POSTransactionItem> POSTransactionItems { get; set; }
        public DbSet<Baki> BakiRecords { get; set; }
        public DbSet<BakiInvoice> BakiInvoices { get; set; }
        public DbSet<BakiInvoiceItem> BakiInvoiceItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AdminNotice> AdminNotices { get; set; }
        
        // Expense entities
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<FixedExpense> FixedExpenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ShopCustomer many-to-many relationship
            modelBuilder.Entity<ShopCustomer>()
                .HasKey(sc => new { sc.ShopId, sc.CustomerId });

            modelBuilder.Entity<ShopCustomer>()
                .HasOne(sc => sc.Shop)
                .WithMany(s => s.ShopCustomers)
                .HasForeignKey(sc => sc.ShopId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ShopCustomer>()
                .HasOne(sc => sc.Customer)
                .WithMany(c => c.ShopCustomers)
                .HasForeignKey(sc => sc.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ApplicationUser relationships
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Shop)
                .WithOne(s => s.User)
                .HasForeignKey<Shop>(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Customer)
                .WithOne(c => c.User)
                .HasForeignKey<Customer>(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Order relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Shop)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.ShopId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure POSTransaction relationships
            modelBuilder.Entity<POSTransaction>()
                .HasOne(p => p.Customer)
                .WithMany(c => c.POSTransactions)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<POSTransaction>()
                .HasOne(p => p.Shop)
                .WithMany(s => s.POSTransactions)
                .HasForeignKey(p => p.ShopId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Baki relationships
            modelBuilder.Entity<Baki>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.BakiRecords)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Baki>()
                .HasOne(b => b.Shop)
                .WithMany(s => s.BakiRecords)
                .HasForeignKey(b => b.ShopId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Category relationships
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Shop)
                .WithMany(s => s.Categories)
                .HasForeignKey(c => c.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Product relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Shop)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.ShopId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Expense relationships
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Shop)
                .WithMany(s => s.Expenses)
                .HasForeignKey(e => e.ShopId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();  // ShopId is required

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.ExpenseCategory)
                .WithMany(ec => ec.Expenses)
                .HasForeignKey(e => e.ExpenseCategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();  // ExpenseCategoryId is required

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.FixedExpense)
                .WithMany(fe => fe.GeneratedExpenses)
                .HasForeignKey(e => e.FixedExpenseId)
                .OnDelete(DeleteBehavior.ClientSetNull)  // Changed to ClientSetNull for nullable FK
                .IsRequired(false);  // FixedExpenseId is optional

            // Configure ExpenseCategory relationships
            modelBuilder.Entity<ExpenseCategory>()
                .HasOne(ec => ec.Shop)
                .WithMany(s => s.ExpenseCategories)
                .HasForeignKey(ec => ec.ShopId)
                .OnDelete(DeleteBehavior.ClientSetNull)  // Changed to ClientSetNull for nullable FK
                .IsRequired(false);  // ShopId is optional (null for default categories)

            // Configure FixedExpense relationships
            modelBuilder.Entity<FixedExpense>()
                .HasOne(fe => fe.Shop)
                .WithMany(s => s.FixedExpenses)
                .HasForeignKey(fe => fe.ShopId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();  // ShopId is required

            modelBuilder.Entity<FixedExpense>()
                .HasOne(fe => fe.ExpenseCategory)
                .WithMany(ec => ec.FixedExpenses)
                .HasForeignKey(fe => fe.ExpenseCategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();  // ExpenseCategoryId is required

            // Add indexes for better query performance
            modelBuilder.Entity<Shop>()
                .HasIndex(s => s.ShopCode)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<POSTransaction>()
                .HasIndex(p => p.TransactionNumber)
                .IsUnique();

            modelBuilder.Entity<BakiInvoice>()
                .HasIndex(b => b.InvoiceNumber)
                .IsUnique();

            modelBuilder.Entity<ExpenseCategory>()
                .HasIndex(ec => new { ec.ShopId, ec.CategoryName });

            modelBuilder.Entity<Expense>()
                .HasIndex(e => new { e.ShopId, e.ExpenseDate });

            modelBuilder.Entity<FixedExpense>()
                .HasIndex(fe => new { fe.ShopId, fe.IsActive, fe.NextDueDate });
        }
    }
}
