using AutoMapper;
using DynamicForm_API.API.DTOs.Submissions;
using DynamicForm_API.API.DTOs.Versions;
using DynamicForm_API.API.Mapping;
using DynamicForm_API.Core.Models.Entities;
using DynamicForm_API.Infrastructure.Data;
using DynamicForm_API.Infrastructure.UnitOfWork;
using DynamicForm_API.Services.Implementation;
using DynamicForm_API.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace DynamicForm_API.Tests
{
    public class FormVersionServiceTests
    {
        [Fact]
        public async Task CreateNextDraftVersionAsync_WithSourceVersion_ClonesFieldsMetadata()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = TestFactory.CreateContext(dbName);
            var mapper = TestFactory.CreateMapper();
            var utcNow = DateTime.UtcNow;

            var form = new Form
            {
                Name = "Onboarding",
                Code = "onboarding",
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
                Versions =
                [
                    new FormVersion
                    {
                        VersionNumber = 1,
                        Status = FormVersionStatus.Published,
                        IsCurrent = true,
                        PublishedAt = utcNow,
                        CreatedAt = utcNow,
                        UpdatedAt = utcNow,
                        Fields =
                        [
                            new FormField
                            {
                                Type = FieldType.Text,
                                Label = "Full Name",
                                Name = "fullName",
                                IsRequired = true,
                                Order = 1,
                                GroupKey = "identity",
                                BindingKey = "profile.fullName",
                                RegexPattern = "^[A-Za-z ]+$",
                                ConfigJson = "{\"min\":1}",
                                CreatedAt = utcNow,
                                UpdatedAt = utcNow
                            },
                            new FormField
                            {
                                Type = FieldType.Select,
                                Label = "Department",
                                Name = "department",
                                IsRequired = true,
                                Order = 2,
                                GroupKey = "work",
                                BindingKey = "profile.department",
                                ConfigJson = "{\"optionsMode\":\"static\",\"options\":[{\"value\":\"eng\",\"label\":\"Engineering\"}]}",
                                CreatedAt = utcNow,
                                UpdatedAt = utcNow
                            }
                        ]
                    }
                ]
            };

            context.Forms.Add(form);
            await context.SaveChangesAsync();

            var sourceVersionId = form.Versions.Single().Id;
            var unitOfWork = new UnitOfWork(context);
            var service = new FormVersionService(unitOfWork, mapper);

            var result = await service.CreateNextDraftVersionAsync(
                form.Id,
                new CreateNextDraftVersionRequestDto { SourceVersionId = sourceVersionId });

            Assert.Equal(2, result.VersionNumber);
            Assert.Equal(FormVersionStatus.Draft, result.Status);
            Assert.Equal(2, result.Fields.Count);

            var clonedFirst = result.Fields.Single(x => x.Name == "fullName");
            Assert.Equal("Full Name", clonedFirst.Label);
            Assert.Equal(1, clonedFirst.Order);
            Assert.Equal("identity", clonedFirst.GroupKey);
            Assert.Equal("profile.fullName", clonedFirst.BindingKey);
            Assert.Equal("^[A-Za-z ]+$", clonedFirst.RegexPattern);
            Assert.Equal("{\"min\":1}", clonedFirst.ConfigJson);

            var clonedSecond = result.Fields.Single(x => x.Name == "department");
            Assert.Equal(FieldType.Select, clonedSecond.Type);
            Assert.Equal(2, clonedSecond.Order);
            Assert.Equal("work", clonedSecond.GroupKey);
            Assert.Equal("profile.department", clonedSecond.BindingKey);
            Assert.NotEmpty(clonedSecond.Options);
            Assert.Equal("eng", clonedSecond.Options[0].Value);
        }
    }

    public class SubmissionServiceValidationTests
    {
        [Fact]
        public async Task SubmitFormAsync_MissingRequiredField_ThrowsArgumentException()
        {
            var service = await CreateSubmissionServiceAsync(
                fields:
                [
                    new FormField
                    {
                        Type = FieldType.Text,
                        Label = "Name",
                        Name = "name",
                        IsRequired = true,
                        Order = 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                ]);

            var request = new SubmitFormVersionRequestDto
            {
                UserId = "u1",
                SubmissionData = new Dictionary<string, JsonElement>()
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitFormAsync("test-form", request));
        }

        [Fact]
        public async Task SubmitFormAsync_InvalidNumberType_ThrowsArgumentException()
        {
            var service = await CreateSubmissionServiceAsync(
                fields:
                [
                    new FormField
                    {
                        Type = FieldType.Number,
                        Label = "Age",
                        Name = "age",
                        IsRequired = true,
                        Order = 1,
                        ConfigJson = "{\"min\":18,\"max\":65}",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                ]);

            var request = new SubmitFormVersionRequestDto
            {
                UserId = "u1",
                SubmissionData = new Dictionary<string, JsonElement>
                {
                    ["age"] = JsonSerializer.SerializeToElement("not-number")
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitFormAsync("test-form", request));
        }

        [Fact]
        public async Task SubmitFormAsync_RegexFailure_ThrowsArgumentException()
        {
            var service = await CreateSubmissionServiceAsync(
                fields:
                [
                    new FormField
                    {
                        Type = FieldType.Text,
                        Label = "Code",
                        Name = "employeeCode",
                        IsRequired = true,
                        RegexPattern = "^[A-Z]{3}$",
                        Order = 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                ]);

            var request = new SubmitFormVersionRequestDto
            {
                UserId = "u1",
                SubmissionData = new Dictionary<string, JsonElement>
                {
                    ["employeeCode"] = JsonSerializer.SerializeToElement("ab1")
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitFormAsync("test-form", request));
        }

        [Fact]
        public async Task SubmitFormAsync_InvalidStaticOption_ThrowsArgumentException()
        {
            var service = await CreateSubmissionServiceAsync(
                fields:
                [
                    new FormField
                    {
                        Type = FieldType.Select,
                        Label = "Department",
                        Name = "department",
                        IsRequired = true,
                        Order = 1,
                        ConfigJson = "{\"optionsMode\":\"static\",\"options\":[{\"value\":\"eng\",\"label\":\"Engineering\"}]}",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                ]);

            var request = new SubmitFormVersionRequestDto
            {
                UserId = "u1",
                SubmissionData = new Dictionary<string, JsonElement>
                {
                    ["department"] = JsonSerializer.SerializeToElement("sales")
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitFormAsync("test-form", request));
        }

        private static async Task<SubmissionService> CreateSubmissionServiceAsync(ICollection<FormField> fields)
        {
            var context = TestFactory.CreateContext(Guid.NewGuid().ToString());
            var mapper = TestFactory.CreateMapper();
            var utcNow = DateTime.UtcNow;

            var form = new Form
            {
                Name = "Test Form",
                Code = "test-form",
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
                Versions =
                [
                    new FormVersion
                    {
                        VersionNumber = 1,
                        Status = FormVersionStatus.Published,
                        IsCurrent = true,
                        PublishedAt = utcNow,
                        CreatedAt = utcNow,
                        UpdatedAt = utcNow,
                        Fields = fields
                    }
                ]
            };

            context.Forms.Add(form);
            await context.SaveChangesAsync();

            var unitOfWork = new UnitOfWork(context);
            return new SubmissionService(unitOfWork, mapper);
        }
    }

    internal static class TestFactory
    {
        public static AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new AppDbContext(options);
        }

        public static IMapper CreateMapper()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(_ => { }, typeof(MappingProfile).Assembly);
            using var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IMapper>();
        }
    }
}
