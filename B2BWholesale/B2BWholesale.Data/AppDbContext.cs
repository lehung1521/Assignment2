using B2BWholesale.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Item ----------
        modelBuilder.Entity<Item>(e =>
        {
            e.ToTable("Item", t =>
            {
                t.HasCheckConstraint("CK_Item_Price", "[Price] > 0");
                t.HasCheckConstraint("CK_Item_Stock", "[StockQuantity] >= 0");
            });
            e.HasKey(x => x.ItemID);
            e.Property(x => x.ItemName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Size).IsRequired().HasMaxLength(50);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.HasIndex(x => new { x.ItemName, x.Size }).IsUnique();
        });

        // ---------- Agent ----------
        modelBuilder.Entity<Agent>(e =>
        {
            e.ToTable("Agent", t =>
                t.HasCheckConstraint("CK_Agent_Phone",
                    "[Phone] NOT LIKE '%[^0-9]%' AND LEN([Phone]) BETWEEN 9 AND 11"));
            e.HasKey(x => x.AgentID);
            e.Property(x => x.AgentName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Address).IsRequired().HasMaxLength(200);
            e.Property(x => x.Phone).IsRequired().HasMaxLength(15).IsUnicode(false);
            e.HasIndex(x => x.Phone).IsUnique();
        });

        // ---------- Order ----------
        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("Order", t =>
                t.HasCheckConstraint("CK_Order_Status",
                    "[OrderStatus] IN ('Pending','Shipped','Delivered','Cancelled')"));
            e.HasKey(x => x.OrderID);
            e.Property(x => x.OrderDate).HasDefaultValueSql("GETDATE()");
            e.Property(x => x.OrderStatus).IsRequired().HasMaxLength(20).IsUnicode(false)
                .HasDefaultValue("Pending");

            // An agent that has orders cannot be deleted (history is kept)
            e.HasOne(x => x.Agent).WithMany(a => a.Orders)
                .HasForeignKey(x => x.AgentID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- OrderDetail ----------
        modelBuilder.Entity<OrderDetail>(e =>
        {
            e.ToTable("OrderDetail", t =>
            {
                t.HasCheckConstraint("CK_OrderDetail_Qty", "[Quantity] > 0");
                t.HasCheckConstraint("CK_OrderDetail_Unit", "[UnitAmount] >= 0");
            });
            e.HasKey(x => x.ID);
            e.Property(x => x.UnitAmount).HasPrecision(18, 2);
            e.HasIndex(x => new { x.OrderID, x.ItemID }).IsUnique();

            // Deleting an order removes its detail lines
            e.HasOne(x => x.Order).WithMany(o => o.OrderDetails)
                .HasForeignKey(x => x.OrderID)
                .OnDelete(DeleteBehavior.Cascade);

            // An item that was sold cannot be deleted
            e.HasOne(x => x.Item).WithMany(i => i.OrderDetails)
                .HasForeignKey(x => x.ItemID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Users ----------
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users", t =>
            {
                t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Admin','Agent')");
                t.HasCheckConstraint("CK_Users_AgentLink",
                    "([Role] = 'Agent' AND [AgentID] IS NOT NULL) OR ([Role] = 'Admin' AND [AgentID] IS NULL)");
            });
            e.HasKey(x => x.UserID);
            e.Property(x => x.UserName).IsRequired().HasMaxLength(50);
            e.Property(x => x.Email).HasColumnName("email").IsRequired().HasMaxLength(100);
            e.Property(x => x.Password).HasColumnName("password").IsRequired().HasMaxLength(64).IsUnicode(false);
            e.Property(x => x.Lock).HasColumnName("lock");
            e.Property(x => x.Role).IsRequired().HasMaxLength(10).IsUnicode(false);
            e.HasIndex(x => x.UserName).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();

            // Deleting an agent removes the agent's login account
            e.HasOne(x => x.Agent).WithMany(a => a.Users)
                .HasForeignKey(x => x.AgentID)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Seed();
    }
}