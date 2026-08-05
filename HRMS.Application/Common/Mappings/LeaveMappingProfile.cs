using AutoMapper;
using HRMS.Application.Features.Leaves.Validators;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Mappings
{
    /// <summary>
    /// AutoMapper Profile mapping Leave requests and commands.
    /// </summary>
    public class LeaveMappingProfile : Profile
    {
        public LeaveMappingProfile()
        {
            CreateMap<SubmitLeaveRequestCommand, LeaveRequest>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FromDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.ToDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.AppliedOn, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Pending"));
        }
    }
}
