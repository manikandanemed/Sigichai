using Microsoft.EntityFrameworkCore;
using HospitalProject.Models;

namespace HospitalProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =========================
        // DB TABLES
        // =========================
        public DbSet<User> Users => Set<User>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
        public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Hospital> Hospitals => Set<Hospital>(); // 🔥 ADD
        public DbSet<OtpStore> OtpStores => Set<OtpStore>();

        public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
        public DbSet<DoctorDocument> DoctorDocuments => Set<DoctorDocument>();

        public DbSet<DoctorStaff> DoctorStaffs => Set<DoctorStaff>();

        public DbSet<DoctorVerification> DoctorVerifications => Set<DoctorVerification>();


        public DbSet<MedicalRep> MedicalReps { get; set; }
        public DbSet<MedicalRepSlot> MedicalRepSlots { get; set; }
     
        public DbSet<MedicalRepAppointment> MedicalRepAppointments { get; set; }





        // =========================
        // FLUENT API (FK RULES)
        // =========================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User ↔ Patient (One-to-One)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Patient)
                .WithOne(p => p.User)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ↔ Doctor (One-to-One)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Doctor)
                .WithOne(d => d.User)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ↔ Admin (One-to-One)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Admin)
                .WithOne(a => a.User)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Doctor ↔ DoctorAvailability (One-to-Many)
            modelBuilder.Entity<DoctorAvailability>()
                .HasOne(da => da.Doctor)
                .WithMany(d => d.Availabilities)
                .HasForeignKey(da => da.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Patient ↔ FamilyMember (One-to-Many)
            modelBuilder.Entity<FamilyMember>()
                .HasOne(f => f.Patient)
                .WithMany(p => p.FamilyMembers)
                .HasForeignKey(f => f.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Patient ↔ Appointment (One-to-Many)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor ↔ Appointment (One-to-Many)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
    .HasOne(a => a.FamilyMember)
    .WithMany()
    .HasForeignKey(a => a.FamilyMemberId)
    .OnDelete(DeleteBehavior.SetNull);

            // Hospital ↔ Doctor
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Hospital)
                .WithMany()
                .HasForeignKey(d => d.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Hospital ↔ Admin
            modelBuilder.Entity<Admin>()
                .HasOne<Hospital>()
                .WithMany()
                .HasForeignKey(a => a.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Hospital ↔ Patient
            //modelBuilder.Entity<Patient>()
            //    .HasOne<Hospital>()
            //    .WithMany()
            //    .HasForeignKey(p => p.HospitalId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // Hospital ↔ Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne<Hospital>()
                .WithMany()
                .HasForeignKey(a => a.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<DoctorProfile>()
                 .HasOne(dp => dp.Doctor)
                 .WithOne()
                 .HasForeignKey<DoctorProfile>(dp => dp.DoctorId)
                 .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorDocument>()
                .HasOne(dd => dd.Doctor)
                .WithMany()
                .HasForeignKey(dd => dd.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<DoctorStaff>()
                .HasOne(ds => ds.Doctor)
                .WithMany()
                .HasForeignKey(ds => ds.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorStaff>()
                .HasOne(ds => ds.User)
                .WithMany()
                .HasForeignKey(ds => ds.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<MedicalRepAppointment>()
    .HasOne(x => x.MedicalRep)
    .WithMany()
    .HasForeignKey(x => x.MedicalRepId);

            modelBuilder.Entity<MedicalRepAppointment>()
                .HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId);







        }
    }
}
