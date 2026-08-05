using AutoMapper;
using HRMS.Application.Features.Departments.Commands.CreateDepartment;
using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Mappings
{
    /// <summary>
    /// AutoMapper Profile mapping Department commands and entities.
    /// </summary>
    public class DepartmentMappingProfile : Profile
    {
        public DepartmentMappingProfile()
        {
            CreateMap<CreateDepartmentCommand, Department>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}
