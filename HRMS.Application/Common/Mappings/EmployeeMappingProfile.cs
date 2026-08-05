using AutoMapper;
using HRMS.Application.Features.Employees.Commands.CreateEmployee;
using HRMS.Application.Features.Employees.Commands.UpdateEmployee;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Mappings
{
    /// <summary>
    /// AutoMapper Profile mapping Employee commands, entities, and DTOs.
    /// </summary>
    public class EmployeeMappingProfile : Profile
    {
        public EmployeeMappingProfile()
        {
            CreateMap<CreateEmployeeCommand, Employee>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.JoinDate, opt => opt.MapFrom(src => src.JoinDate == default ? DateTime.UtcNow : src.JoinDate));

            CreateMap<UpdateEmployeeCommand, Employee>()
                .ForMember(dest => dest.JoinDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
        }
    }
}
