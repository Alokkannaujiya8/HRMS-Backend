using AutoMapper;
using HRMS.Application.Features.Attendance.Validators;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Mappings
{
    /// <summary>
    /// AutoMapper Profile mapping Attendance commands and entities.
    /// </summary>
    public class AttendanceMappingProfile : Profile
    {
        public AttendanceMappingProfile()
        {
            CreateMap<ClockInCommand, Attendance>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AttendanceDate, opt => opt.MapFrom(src => src.ClockInTime.Date))
                .ForMember(dest => dest.CheckInTime, opt => opt.MapFrom(src => src.ClockInTime));

            CreateMap<ClockOutCommand, Attendance>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CheckOutTime, opt => opt.MapFrom(src => src.ClockOutTime));
        }
    }
}
