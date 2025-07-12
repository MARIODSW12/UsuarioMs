using MediatR;
using log4net;

using Usuarios.Application.Commands;

using Usuarios.Domain.Repositories;
using Usuarios.Domain.ValueObjects;
using Usuarios.Domain.Aggregates;
using Usuarios.Domain.Events;

namespace Usuarios.Application.Handlers
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMediator _mediator;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CreateUserCommandHandler));

        public CreateUserCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository, IMediator mediator)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando comando CreateUser para {request.UserDto.Email}");

            try
            {
                // Verificar si el ID del rol existe en la base de datos
                var roleExists = await _roleRepository.GetByIdAsync(request.UserDto.RoleId);
                if (roleExists == null)
                {
                    _logger.Warn($"El rol {request.UserDto.RoleId} no existe.");
                    throw new ArgumentException("El rol especificado no existe.");
                }

                // Verificar si el correo electrónico ya está registrado
                var existingUser = await _userRepository.GetByEmailAsync(request.UserDto.Email);
                if (existingUser != null)
                {
                    _logger.Warn($"El correo {request.UserDto.Email} ya está registrado.");
                    throw new ArgumentException("El correo electrónico ya está registrado.");
                }

                // Crear el usuario solo con el ID del rol
                var user = new User(
                    new VOId(Guid.NewGuid().ToString()),
                    new VOName(request.UserDto.Name),
                    new VOLastName(request.UserDto.LastName),
                    new VOEmail(request.UserDto.Email),
                    new VORoleId(request.UserDto.RoleId),
                    new VOAddress(request.UserDto.Address),
                    new VOPhone(request.UserDto.Phone)
                );

                await _userRepository.AddAsync(user);
                _logger.Info($"Usuario {user.Email.Value} creado con éxito con ID {user.Id.Value}");

                // Publicar el evento de usuario creado
                var userCreatedEvent = new UserCreatedEvent(
                    user.Id.Value, user.Name.Value, user.LastName.Value, user.Email.Value, user.RoleId.Value,
                    user.Address?.Value, user.Phone?.Value
                );
                await _mediator.Publish(userCreatedEvent);
                _logger.Info($"Evento publicado: Usuario {user.Id.Value} creado.");

                return user.Id.Value;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al crear usuario {request.UserDto.Email}", ex);
                throw;
            }
        }
    }
}