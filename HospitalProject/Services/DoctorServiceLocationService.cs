using HospitalProject.Models;
using HospitalProject.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalProject.Services
{
    public class DoctorServiceLocationService
    {
        private readonly IRepository<DoctorServiceLocation> _location;
        private readonly IRepository<DoctorServiceSlot> _slot;
        private readonly IRepository<Doctor> _doctor;
        private readonly IRepository<Hospital> _hospital;
        private readonly IRepository<Speciality> _speciality;

        public DoctorServiceLocationService(
            IRepository<DoctorServiceLocation> location,
            IRepository<DoctorServiceSlot> slot,
            IRepository<Doctor> doctor,
            IRepository<Hospital> hospital,
            IRepository<Speciality> speciality)
        {
            _location = location;
            _slot = slot;
            _doctor = doctor;
            _hospital = hospital;
            _speciality = speciality;
        }

        // =====================================================================
        // 1️⃣ DOCTOR — Save Service Locations (Multiple)
        // =====================================================================
        public async Task SaveServiceLocations(int userId, SaveServiceLocationsDto dto)
        {
            var doctor = await _doctor.GetAsync(d => d.UserId == userId)
                ?? throw new Exception("Doctor not found");

            // Existing locations delete பண்றோம் — fresh save
            var existing = await _location.Query()
                .Include(l => l.Slots)
                .Where(l => l.DoctorId == doctor.Id)
                .ToListAsync();

            if (existing.Any())
            {
                foreach (var loc in existing)
                    loc.IsActive = false;

                await _location.SaveAsync();
            }

            // புது locations save பண்றோம்
            foreach (var locDto in dto.Locations)
            {
                // Hospital valid-ஆ check
                var hospital = await _hospital.GetAsync(h => h.Id == locDto.HospitalId)
                    ?? throw new Exception($"Hospital {locDto.HospitalId} not found");

                // Speciality valid-ஆ check
                var speciality = await _speciality.GetAsync(
                    s => s.Id == locDto.SpecialityId && s.IsActive)
                    ?? throw new Exception($"Speciality {locDto.SpecialityId} not found");

                // Timeslots வேணும்
                if (locDto.TimeSlots == null || locDto.TimeSlots.Count == 0)
                    throw new Exception("At least one time slot is required");

                var location = new DoctorServiceLocation
                {
                    DoctorId = doctor.Id,
                    HospitalId = locDto.HospitalId,
                    SpecialityId = locDto.SpecialityId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var timeSlot in locDto.TimeSlots)
                {
                    location.Slots.Add(new DoctorServiceSlot
                    {
                        TimeSlot = timeSlot
                    });
                }

                await _location.AddAsync(location);
            }

            await _location.SaveAsync();
        }

        // =====================================================================
        // 2️⃣ DOCTOR — View My Service Locations
        // =====================================================================
        public async Task<List<DoctorServiceLocationViewDto>> GetMyLocations(int userId)
        {
            var doctor = await _doctor.GetAsync(d => d.UserId == userId)
                ?? throw new Exception("Doctor not found");

            return await _location.Query()
                .Include(l => l.Hospital)
                .Include(l => l.Speciality)
                .Include(l => l.Slots)
                .Where(l => l.DoctorId == doctor.Id && l.IsActive)
                .Select(l => new DoctorServiceLocationViewDto(
                    l.Id,
                    l.HospitalId,
                    l.Hospital.Name,
                    l.Hospital.State,
                    l.Hospital.Area,
                    l.SpecialityId,
                    l.Speciality.Name,
                    l.IsActive,
                    l.Slots.Select(s => new DoctorServiceSlotViewDto(
                        s.Id,
                        s.TimeSlot
                    )).ToList()
                ))
                .ToListAsync();
        }

        // =====================================================================
        // 3️⃣ PUBLIC — Get States List
        // =====================================================================
        public async Task<List<string>> GetStates()
        {
            return await _hospital.Query()
                .Where(h => !string.IsNullOrEmpty(h.State))
                .Select(h => h.State)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        // =====================================================================
        // 4️⃣ PUBLIC — Get Areas by State
        // =====================================================================
        public async Task<List<string>> GetAreasByState(string state)
        {
            return await _hospital.Query()
                .Where(h => h.State == state && !string.IsNullOrEmpty(h.Area))
                .Select(h => h.Area)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }

        // =====================================================================
        // 5️⃣ PUBLIC — Get Hospitals by State + Area
        // =====================================================================
        public async Task<List<HospitalByStateAreaDto>> GetHospitals(
            string state, string area)
        {
            return await _hospital.Query()
                .Where(h => h.State == state && h.Area == area)
                .Select(h => new HospitalByStateAreaDto(
                    h.Id,
                    h.Name,
                    h.State,
                    h.Area
                ))
                .ToListAsync();
        }
    }
}