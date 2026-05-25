using System;
using System.Linq;
using PharmacyBillingService.Authentication;
using PharmacyBillingService.Domain;

namespace PharmacyBillingService.Data
{
    public static class DbInitializer
    {
        public static void Initialize(PharmacyDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Seed Roles (handled in OnModelCreating, but let's double check if they exist in DB)
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Id = Role.AdminId, Name = Role.Admin },
                    new Role { Id = Role.DoctorId, Name = Role.Doctor },
                    new Role { Id = Role.ReceptionistId, Name = Role.Receptionist },
                    new Role { Id = Role.PatientId, Name = Role.Patient }
                );
                context.SaveChanges();
            }

            // Seed Users
            if (!context.Users.Any())
            {
                var adminUser = new User
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Username = "admin",
                    PasswordHash = SecurityHelper.HashPassword("admin123"),
                    FullName = "System Administrator",
                    Email = "admin@clinic.com",
                    PhoneNumber = "0123456789",
                    RoleId = Role.AdminId
                };

                var doctorUser = new User
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Username = "doctor1",
                    PasswordHash = SecurityHelper.HashPassword("doctor123"),
                    FullName = "Dr. Nguyen Van A",
                    Email = "nguyenvana@clinic.com",
                    PhoneNumber = "0987654321",
                    RoleId = Role.DoctorId
                };

                var receptionistUser = new User
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Username = "receptionist1",
                    PasswordHash = SecurityHelper.HashPassword("reception123"),
                    FullName = "Ms. Le Thi B",
                    Email = "lethib@clinic.com",
                    PhoneNumber = "0909090909",
                    RoleId = Role.ReceptionistId
                };

                var patientUser = new User
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Username = "patient1",
                    PasswordHash = SecurityHelper.HashPassword("patient123"),
                    FullName = "Tran Van C",
                    Email = "tranvanc@gmail.com",
                    PhoneNumber = "0888888888",
                    RoleId = Role.PatientId
                };

                context.Users.AddRange(adminUser, doctorUser, receptionistUser, patientUser);
                context.SaveChanges();
            }

            // Seed Medicines & Stock
            if (!context.Medicines.Any())
            {
                var medicines = new[]
                {
                    new Medicine
                    {
                        Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                        Name = "Paracetamol 500mg",
                        ActiveIngredient = "Paracetamol",
                        Unit = "Viên",
                        Price = 2000,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Medicine
                    {
                        Id = Guid.Parse("b2222222-2222-2222-2222-222222222222"),
                        Name = "Amoxicillin 500mg",
                        ActiveIngredient = "Amoxicillin",
                        Unit = "Viên",
                        Price = 5000,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Medicine
                    {
                        Id = Guid.Parse("c3333333-3333-3333-3333-333333333333"),
                        Name = "Ibuprofen 400mg",
                        ActiveIngredient = "Ibuprofen",
                        Unit = "Viên",
                        Price = 3500,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Medicine
                    {
                        Id = Guid.Parse("d4444444-4444-4444-4444-444444444444"),
                        Name = "Decolgen Forte",
                        ActiveIngredient = "Acetaminophen + Chlorpheniramine",
                        Unit = "Hộp",
                        Price = 28000,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Medicine
                    {
                        Id = Guid.Parse("e5555555-5555-5555-5555-555555555555"),
                        Name = "Gaviscon Suspension",
                        ActiveIngredient = "Sodium Alginate + Sodium Bicarbonate",
                        Unit = "Gói",
                        Price = 8500,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Medicines.AddRange(medicines);
                context.SaveChanges();

                var stocks = new[]
                {
                    new MedicineStock { MedicineId = medicines[0].Id, Quantity = 500, MinAlertQuantity = 50, LastUpdated = DateTime.UtcNow },
                    new MedicineStock { MedicineId = medicines[1].Id, Quantity = 300, MinAlertQuantity = 30, LastUpdated = DateTime.UtcNow },
                    new MedicineStock { MedicineId = medicines[2].Id, Quantity = 250, MinAlertQuantity = 25, LastUpdated = DateTime.UtcNow },
                    new MedicineStock { MedicineId = medicines[3].Id, Quantity = 15,  MinAlertQuantity = 10, LastUpdated = DateTime.UtcNow },
                    new MedicineStock { MedicineId = medicines[4].Id, Quantity = 120, MinAlertQuantity = 20, LastUpdated = DateTime.UtcNow }
                };

                context.MedicineStocks.AddRange(stocks);
                context.SaveChanges();
            }
        }
    }
}
