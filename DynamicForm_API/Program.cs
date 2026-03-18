using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Fields;
using DynamicForm_API.API.DTOs.Forms;
using DynamicForm_API.API.DTOs.Lookups;
using DynamicForm_API.API.DTOs.Submissions;
using DynamicForm_API.API.DTOs.Versions;
using DynamicForm_API.API.Mapping;
using DynamicForm_API.API.Validators;
using DynamicForm_API.Core.Interfaces;
using DynamicForm_API.Infrastructure.Data;
using DynamicForm_API.Infrastructure.Repositories;
using DynamicForm_API.Infrastructure.UnitOfWork;
using DynamicForm_API.Services.Implementation;
using DynamicForm_API.Services.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(
    cfg => cfg.AddProfile<MappingProfile>(),
    typeof(MappingProfile).Assembly);



builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IFormRepository, FormRepository>();
builder.Services.AddScoped<IFormVersionRepository, FormVersionRepository>();
builder.Services.AddScoped<ILookupRepository, LookupRepository>();
builder.Services.AddScoped<IFormSubmissionRepository, FormSubmissionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<IFormVersionService, FormVersionService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();

builder.Services.AddScoped<IValidator<PagedRequestDto>, PagedRequestDtoValidator>();
builder.Services.AddScoped<IValidator<CreateFormDraftRequestDto>, CreateFormDraftRequestDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateFormMetadataRequestDto>, UpdateFormMetadataRequestDtoValidator>();
builder.Services.AddScoped<IValidator<ArchiveFormRequestDto>, ArchiveFormRequestDtoValidator>();

builder.Services.AddScoped<IValidator<CreateNextDraftVersionRequestDto>, CreateNextDraftVersionRequestDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateDraftVersionRequestDto>, UpdateDraftVersionRequestDtoValidator>();
builder.Services.AddScoped<IValidator<PublishDraftVersionRequestDto>, PublishDraftVersionRequestDtoValidator>();
builder.Services.AddScoped<IValidator<ArchiveVersionRequestDto>, ArchiveVersionRequestDtoValidator>();

builder.Services.AddScoped<IValidator<AddDraftFieldRequestDto>, AddDraftFieldRequestDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateDraftFieldRequestDto>, UpdateDraftFieldRequestDtoValidator>();
builder.Services.AddScoped<IValidator<ReorderDraftFieldItemDto>, ReorderDraftFieldItemDtoValidator>();
builder.Services.AddScoped<IValidator<ReorderDraftFieldsRequestDto>, ReorderDraftFieldsRequestDtoValidator>();

builder.Services.AddScoped<IValidator<CreateLookupTableRequestDto>, CreateLookupTableRequestDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateLookupTableRequestDto>, UpdateLookupTableRequestDtoValidator>();
builder.Services.AddScoped<IValidator<ArchiveLookupTableRequestDto>, ArchiveLookupTableRequestDtoValidator>();
builder.Services.AddScoped<IValidator<CreateLookupItemRequestDto>, CreateLookupItemRequestDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateLookupItemRequestDto>, UpdateLookupItemRequestDtoValidator>();
builder.Services.AddScoped<IValidator<SetLookupItemActiveStateRequestDto>, SetLookupItemActiveStateRequestDtoValidator>();
builder.Services.AddScoped<IValidator<ReorderLookupItemDto>, ReorderLookupItemDtoValidator>();
builder.Services.AddScoped<IValidator<ReorderLookupItemsRequestDto>, ReorderLookupItemsRequestDtoValidator>();

builder.Services.AddScoped<IValidator<SubmitFormVersionRequestDto>, SubmitFormVersionRequestDtoValidator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
