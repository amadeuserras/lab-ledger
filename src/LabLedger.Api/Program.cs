using System.Text;
using FluentValidation;
using LabLedger.Api.Authorization;
using LabLedger.Api.Exceptions;
using LabLedger.Api.GraphQL;
using LabLedger.Api.GraphQL.DataLoaders;
using LabLedger.Application.Features.Auth;
using LabLedger.Application.Features.Results;
using LabLedger.Application.Features.Samples;
using LabLedger.Application.Features.Tests;
using LabLedger.Core.Interfaces;
using LabLedger.DataModel;
using LabLedger.DataModel.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<LabLedgerDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IUnitOfWork, EFUnitOfWork>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IAuthComponent, AuthComponent>();
builder.Services.AddScoped<ISampleComponent, SampleComponent>();
builder.Services.AddScoped<ITestComponent, TestComponent>();
builder.Services.AddScoped<IResultComponent, ResultComponent>();
builder.Services.AddValidatorsFromAssemblyContaining<SampleValidator>();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration is missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permissions.All)
    {
        options.AddPolicy(
            RequirePermissionAttribute.PolicyPrefix + permission,
            policy => policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddType<SampleType>()
    .AddType<TestType>()
    .AddType<ResultType>()
    .AddType<UserType>()
    .AddDataLoader<TestsBySampleIdDataLoader>()
    .AddAuthorization()
    .ModifyRequestOptions(options =>
        options.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGraphQL();

app.Run();
