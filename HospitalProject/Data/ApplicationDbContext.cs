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
        public DbSet<Speciality> Specialities => Set<Speciality>();

        public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
        public DbSet<DoctorDocument> DoctorDocuments => Set<DoctorDocument>();

        public DbSet<DoctorStaff> DoctorStaffs => Set<DoctorStaff>();

        public DbSet<DoctorVerification> DoctorVerifications => Set<DoctorVerification>();


        public DbSet<MedicalRep> MedicalReps { get; set; }
        public DbSet<MedicalRepSlot> MedicalRepSlots { get; set; }
     
        public DbSet<MedicalRepAppointment> MedicalRepAppointments { get; set; }

        public DbSet<PaymentLog> PaymentLogs { get; set; }

        public DbSet<InternalPharmacy> InternalPharmacies => Set<InternalPharmacy>();

        public DbSet<InternalPharmacyStaffRequest> InternalPharmacyStaffRequests => Set<InternalPharmacyStaffRequest>();

        // Module 3
        public DbSet<Medicine> Medicines => Set<Medicine>();
        public DbSet<InternalPharmacyInventory> InternalPharmacyInventories => Set<InternalPharmacyInventory>();
        public DbSet<Prescription> Prescriptions => Set<Prescription>();
        public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
        public DbSet<DispenseRecord> DispenseRecords => Set<DispenseRecord>();
        public DbSet<DispenseItem> DispenseItems => Set<DispenseItem>();
        public DbSet<PharmacyNotification> PharmacyNotifications => Set<PharmacyNotification>();
        public DbSet<DrugInteraction> DrugInteractions => Set<DrugInteraction>();
        public DbSet<PrescriptionQrCode> PrescriptionQrCodes => Set<PrescriptionQrCode>();
        public DbSet<QrScanLog> QrScanLogs => Set<QrScanLog>();

        //ExternalPharmacy
        public DbSet<ExternalPharmacy> ExternalPharmacies => Set<ExternalPharmacy>();
        public DbSet<ExternalPharmacyDocument> ExternalPharmacyDocuments => Set<ExternalPharmacyDocument>();

        public DbSet<PatientPreferredPharmacy> PatientPreferredPharmacies => Set<PatientPreferredPharmacy>();
        public DbSet<PrescriptionRoute> PrescriptionRoutes => Set<PrescriptionRoute>();

        public DbSet<PharmacyQuotation> PharmacyQuotations => Set<PharmacyQuotation>();
        public DbSet<PharmacyQuotationItem> PharmacyQuotationItems => Set<PharmacyQuotationItem>();

        public DbSet<PharmacyOrder> PharmacyOrders => Set<PharmacyOrder>();
        public DbSet<PharmacyOrderStatusLog> PharmacyOrderStatusLogs => Set<PharmacyOrderStatusLog>();

        public DbSet<PharmacyRating> PharmacyRatings => Set<PharmacyRating>();

        public DbSet<NmcDoctorRecord> NmcDoctorRecords => Set<NmcDoctorRecord>();

        public DbSet<DoctorServiceLocation> DoctorServiceLocations => Set<DoctorServiceLocation>();
        public DbSet<DoctorServiceSlot> DoctorServiceSlots => Set<DoctorServiceSlot>();





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

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Speciality)
                .WithMany()
                .HasForeignKey(d => d.SpecialityId)
                .OnDelete(DeleteBehavior.SetNull);

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


            modelBuilder.Entity<PaymentLog>()
            .Property(p => p.Amount)
             .HasPrecision(18, 2);

            modelBuilder.Entity<PaymentLog>()
        .HasOne(p => p.Appointment)
        .WithMany(a => a.PaymentLogs)
        .HasForeignKey(p => p.AppointmentId)
        .OnDelete(DeleteBehavior.SetNull); // or Cascade


            // Prescription Relationships
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Appointment)
                .WithMany()
                .HasForeignKey(p => p.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany()
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany()
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(pi => pi.Prescription)
                .WithMany(p => p.Items)
                .HasForeignKey(pi => pi.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InternalPharmacyInventory>()
                .HasOne(i => i.InternalPharmacy)
                .WithMany()
                .HasForeignKey(i => i.InternalPharmacyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DispenseRecord>()
                .HasOne(dr => dr.Prescription)
                .WithMany()
                .HasForeignKey(dr => dr.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DispenseItem>()
                .HasOne(di => di.DispenseRecord)
                .WithMany(dr => dr.Items)
                .HasForeignKey(di => di.DispenseRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DispenseItem>()
                .HasOne(di => di.Medicine)
                .WithMany()
                .HasForeignKey(di => di.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InternalPharmacyInventory>()
                .Property(i => i.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DispenseRecord>()
                .Property(dr => dr.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DispenseItem>()
                .Property(di => di.PricePerUnit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DrugInteraction>()
                .HasOne(x => x.MedicineA)
                .WithMany()
                .HasForeignKey(x => x.MedicineIdA)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DrugInteraction>()
                .HasOne(x => x.MedicineB)
                .WithMany()
                .HasForeignKey(x => x.MedicineIdB)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<PrescriptionQrCode>()
            .HasOne(q => q.Prescription)
            .WithMany()
            .HasForeignKey(q => q.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QrScanLog>()
                .HasOne(l => l.PrescriptionQrCode)
                .WithMany()
                .HasForeignKey(l => l.PrescriptionQrCodeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QrScanLog>()
                .HasOne(l => l.ScannedByUser)
                .WithMany()
                .HasForeignKey(l => l.ScannedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            //ExternalPharmacy

            modelBuilder.Entity<ExternalPharmacyDocument>()
               .HasOne(d => d.ExternalPharmacy)
               .WithMany()
               .HasForeignKey(d => d.ExternalPharmacyId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExternalPharmacy>()
                .Property(e => e.AverageRating)
                .HasPrecision(3, 2);

            modelBuilder.Entity<ExternalPharmacy>()
                .Property(e => e.DeliveryRadius)
                .HasPrecision(10, 2);


            modelBuilder.Entity<PatientPreferredPharmacy>()
                .HasOne(p => p.Patient)
                .WithMany()
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PatientPreferredPharmacy>()
                .HasOne(p => p.ExternalPharmacy)
                .WithMany()
                .HasForeignKey(p => p.ExternalPharmacyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PrescriptionRoute>()
                .HasOne(r => r.Prescription)
                .WithMany()
                .HasForeignKey(r => r.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PrescriptionRoute>()
                .HasOne(r => r.InternalPharmacy)
                .WithMany()
                .HasForeignKey(r => r.InternalPharmacyId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PrescriptionRoute>()
                .HasOne(r => r.ExternalPharmacy)
                .WithMany()
                .HasForeignKey(r => r.ExternalPharmacyId)
                .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<PharmacyQuotation>()
                .HasOne(q => q.PrescriptionRoute)
                .WithMany()
                .HasForeignKey(q => q.PrescriptionRouteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PharmacyQuotation>()
                .HasOne(q => q.ExternalPharmacy)
                .WithMany()
                .HasForeignKey(q => q.ExternalPharmacyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PharmacyQuotationItem>()
                .HasOne(i => i.Quotation)
                .WithMany(q => q.Items)
                .HasForeignKey(i => i.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PharmacyQuotationItem>()
                .HasOne(i => i.Medicine)
                .WithMany()
                .HasForeignKey(i => i.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PharmacyQuotation>()
                .Property(q => q.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PharmacyQuotation>()
                .Property(q => q.DeliveryCharge)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PharmacyQuotationItem>()
                .Property(i => i.PricePerUnit)
                .HasPrecision(18, 2);


            modelBuilder.Entity<PharmacyOrder>()
                .HasOne(o => o.Quotation)
                .WithMany()
                .HasForeignKey(o => o.QuotationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PharmacyOrder>()
                .HasOne(o => o.Patient)
                .WithMany()
                .HasForeignKey(o => o.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PharmacyOrderStatusLog>()
                .HasOne(l => l.PharmacyOrder)
                .WithMany(o => o.StatusLogs)
                .HasForeignKey(l => l.PharmacyOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PharmacyOrder>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PharmacyOrder>()
                .Property(o => o.DeliveryCharge)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PharmacyRating>()
                .HasOne(r => r.ExternalPharmacy)
                .WithMany()
                .HasForeignKey(r => r.ExternalPharmacyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PharmacyRating>()
                .HasOne(r => r.Patient)
                .WithMany()
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PharmacyRating>()
                .HasOne(r => r.PharmacyOrder)
                .WithMany()
                .HasForeignKey(r => r.PharmacyOrderId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<DoctorServiceLocation>()
                .HasOne(d => d.Doctor)
                .WithMany()
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorServiceLocation>()
                .HasOne(d => d.Hospital)
                .WithMany()
                .HasForeignKey(d => d.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorServiceLocation>()
                .HasOne(d => d.Speciality)
                .WithMany()
                .HasForeignKey(d => d.SpecialityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorServiceSlot>()
                .HasOne(s => s.DoctorServiceLocation)
                .WithMany(d => d.Slots)
                .HasForeignKey(s => s.DoctorServiceLocationId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<InternalPharmacy>()
                .HasOne(p => p.Hospital)
                .WithMany()
                .HasForeignKey(p => p.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.DoctorServiceLocation)
                .WithMany()
                .HasForeignKey(a => a.DoctorServiceLocationId)
                .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<DoctorAvailability>()
                .HasOne(a => a.Hospital)
                .WithMany()
                .HasForeignKey(a => a.HospitalId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
