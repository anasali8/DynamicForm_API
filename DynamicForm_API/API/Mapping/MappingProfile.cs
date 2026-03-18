using AutoMapper;
using DynamicForm_API.API.DTOs.Fields;
using DynamicForm_API.API.DTOs.Forms;
using DynamicForm_API.API.DTOs.Lookups;
using DynamicForm_API.API.DTOs.Submissions;
using DynamicForm_API.API.DTOs.Versions;
using DynamicForm_API.Core.Models.Entities;

namespace DynamicForm_API.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Form, FormSummaryResponseDto>();
            CreateMap<Form, FormDetailResponseDto>();

            CreateMap<FormVersion, FormVersionSummaryResponseDto>();
            CreateMap<FormVersion, FormVersionHistoryItemResponseDto>();
            CreateMap<FormVersion, FormVersionDetailResponseDto>();

            CreateMap<FormField, FormFieldResponseDto>();

            CreateMap<LookupTable, LookupTableSummaryResponseDto>();
            CreateMap<LookupTable, LookupTableDetailResponseDto>();
            CreateMap<LookupItem, LookupItemResponseDto>();
            CreateMap<LookupItem, LookupOptionItemResponseDto>();

            CreateMap<FormSubmission, FormSubmissionListItemResponseDto>();
            CreateMap<FormSubmission, FormSubmissionDetailResponseDto>();
            CreateMap<FormSubmission, FormSubmissionCreatedResponseDto>()
                .ForMember(d => d.SubmissionId, opt => opt.MapFrom(s => s.Id));
        }
    }
}
