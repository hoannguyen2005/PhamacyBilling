using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyBillingService.Domain
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // We manually assign standard IDs
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public ICollection<User> Users { get; set; } = new List<User>();

        // Predefined roles constants
        public const int AdminId = 1;
        public const int DoctorId = 2;
        public const int ReceptionistId = 3;
        public const int PatientId = 4;

        public const string Admin = "Admin";
        public const string Doctor = "Doctor";
        public const string Receptionist = "Receptionist";
        public const string Patient = "Patient";
    }
}
