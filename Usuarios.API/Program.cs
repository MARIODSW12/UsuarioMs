using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using log4net;
using log4net.Config;
using MassTransit;
using System.Reflection;

using Usuarios.API.Middlewares;

using Usuarios.Application.EventHandlers;
using Usuarios.Application.Events;
using Usuarios.Application.Handlers;
using Usuarios.Application.Validations;

using Usuarios.Domain.Events;
using Usuarios.Domain.Events.EventHandler;
using Usuarios.Domain.Repositories;

using Usuarios.Infrastructure.Configurations;
using Usuarios.Infrastructure.Consumer;
using Usuarios.Infrastructure.Interfaces;
using Usuarios.Infrastructure.Persistence.Repository;
using Usuarios.Infrastructure.Persistence.Repository.MongoRead;
using Usuarios.Infrastructure.Persistence.Repository.MongoWrite;
using Usuarios.Infrastructure.Queries.QueryHandlers;

var loggerRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(loggerRepository, new FileInfo("log4net.config"));

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar configuración de MongoDB
builder.Services.AddSingleton<MongoWriteDbConfig>();
builder.Services.AddSingleton<MongoReadDbConfig>();
builder.Services.AddSingleton<MongoReadUserActivityDbConfig>();

// Registrar configuración de Log4Net
builder.Services.AddSingleton(LogManager.GetLogger(typeof(Program)));

// REGISTRA EL REPOSITORIO ANTES DE MediatR
builder.Services.AddScoped<IUserRepository, MongoWriteUserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleWriteRepository>();
builder.Services.AddScoped<IUserReadRepository, MongoReadUserRepository>();
builder.Services.AddScoped<IRoleReadRepository, RoleReadRepository>();
builder.Services.AddScoped<IUserActivityReadRepository, MongoReadUserActivityRepository>();
builder.Services.AddScoped<IPermissionReadRepository, PermissionReadRepository>();

// REGISTRA MediatR PARA TODOS LOS HANDLERS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUserCommandHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateUserCommandHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateUserRoleCommandHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AddPermissionToRoleCommandHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RemovePermissionFromRoleCommandHandler).Assembly));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UserCreatedEventHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UserUpdatedEventHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UserRoleUpdatedEventHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PermissionAddedToRoleEventHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PermissionRemovedFromRoleEventHandler).Assembly));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UserActivityQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetUserByEmailQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllRolesQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllUsersQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllPermissionsQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetPermissionsByRoleIdQueryHandler).Assembly));

builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidation>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserDtoValidation>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserActivityDtoValidation>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserRoleDtoValidation>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.AddConsumer<CreateUserConsumer>();
    busConfigurator.AddConsumer<UpdateUserConsumer>();
    busConfigurator.AddConsumer<UserActivityConsumer>();
    busConfigurator.AddConsumer<UserRoleUpdateConsumer>();
    busConfigurator.AddConsumer<PermissionAddedToRoleConsumer>();
    busConfigurator.AddConsumer<PermissionRemovedFromRoleConsumer>();

    busConfigurator.SetKebabCaseEndpointNameFormatter();
    busConfigurator.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(new Uri(Environment.GetEnvironmentVariable("RABBIT_URL")), h =>
        {
            h.Username(Environment.GetEnvironmentVariable("RABBIT_USERNAME"));
            h.Password(Environment.GetEnvironmentVariable("RABBIT_PASSWORD"));
        });

        configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_QUEUE"), e => {
            e.ConfigureConsumer<CreateUserConsumer>(context);
        });
        configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_QUEUE_UPDATE"), e => {
            e.ConfigureConsumer<UpdateUserConsumer>(context);
        });
        configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_QUEUE_ACTIVITY"), e => {
            e.ConfigureConsumer<UserActivityConsumer>(context);
        });
        configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_QUEUE_UPDATE_ROLE"), e => {
            e.ConfigureConsumer<UserRoleUpdateConsumer>(context);
        });
        configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_QUEUE_ADD_ROLE_PERMISSION"), e => {
            e.ConfigureConsumer<PermissionAddedToRoleConsumer>(context);
        });
        configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_QUEUE_REMOVE_ROLE_PERMISSION"), e => {
            e.ConfigureConsumer<PermissionRemovedFromRoleConsumer>(context);
        });

        configurator.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        configurator.ConfigureEndpoints(context);
    });
});
EndpointConvention.Map<UserCreatedEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_QUEUE")));
EndpointConvention.Map<UserUpdatedEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_QUEUE_UPDATE")));
EndpointConvention.Map<UserActivityMadeEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_QUEUE_ACTIVITY")));
EndpointConvention.Map<UserRoleUpdatedEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_QUEUE_UPDATE_ROLE")));
EndpointConvention.Map<PermissionAddedToRoleEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_QUEUE_ADD_ROLE_PERMISSION")));
EndpointConvention.Map<PermissionRemovedFromRoleEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_QUEUE_REMOVE_ROLE_PERMISSION")));

// Configuración CORS permisiva (¡Solo para desarrollo!)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // Permite cualquier dominio
            .AllowAnyMethod()  // GET, POST, PUT, DELETE, etc.
            .AllowAnyHeader(); // Cualquier cabecera
    });
});

var app = builder.Build();

// Habilitar CORS
app.UseCors("AllowAll");

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.

app.UseSwagger();
 app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
