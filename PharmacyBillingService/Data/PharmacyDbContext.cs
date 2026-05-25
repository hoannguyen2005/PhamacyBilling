using Microsoft.EntityFrameworkCore;
using PharmacyBillingService.Domain;

namespace PharmacyBillingService.Data
{
    public class PharmacyDbContext : DbContext
    {
        public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Medicine> Medicines { get; set; } = null!;
        public DbSet<MedicineStock> MedicineStocks { get; set; } = null!;
        public DbSet<Prescription> Prescriptions { get; set; } = null!;
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; } = null!;
        public DbSet<Bill> Bills { get; set; } = null!;
        public DbSet<BillItem> BillItems { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table relationships & constraints

            // Role unique constraints
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            // User unique username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // 1-to-1 relationship between Medicine and MedicineStock
            modelBuilder.Entity<Medicine>()
                .HasOne(m => m.Stock)
                .WithOne(s => s.Medicine)
                .HasForeignKey<MedicineStock>(s => s.MedicineId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prescription -> PrescriptionItems relationship
            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(pi => pi.Prescription)
                .WithMany(p => p.Items)
                .HasForeignKey(pi => pi.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // PrescriptionItem -> Medicine relationship (no delete cascade to avoid deleting prescription items when a medicine is deleted)
            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(pi => pi.Medicine)
                .WithMany()
                .HasForeignKey(pi => pi.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bill -> Prescription (nullable, 1-to-1 or 1-to-many, we make it 1-to-1 relation or simple foreign key)
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Prescription)
                .WithMany()
                .HasForeignKey(b => b.PrescriptionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Bill -> BillItems relationship
            modelBuilder.Entity<BillItem>()
                .HasOne(bi => bi.Bill)
                .WithMany(b => b.BillItems)
                .HasForeignKey(bi => bi.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bill -> Payments relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Bill)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment -> User (ReceivedBy) relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.ReceivedBy)
                .WithMany()
                .HasForeignKey(p => p.ReceivedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = Role.AdminId, Name = Role.Admin },
                new Role { Id = Role.DoctorId, Name = Role.Doctor },
                new Role { Id = Role.ReceptionistId, Name = Role.Receptionist },
                new Role { Id = Role.PatientId, Name = Role.Patient }
            );
        }
    }
}
