using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using log4net;

using Usuarios.Application.Commands;
using Usuarios.Application.DTOs;
using Usuarios.Application.Events;

using Usuarios.Infrastructure.Queries;

namespace Usuarios.Presentation.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IPublishEndpoint _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UsersController));

        public UsersController(IMediator mediator, IPublishEndpoint publishEndpoint)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        #region CreateUser
        [HttpPost("createUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
        {
            _logger.Info("Iniciando solicitud POST /api/users/create");

            try
            {
                var userId = await _mediator.Send(new CreateUserCommand(userDto));

                if (userId == null)
                {
                    _logger.Warn("Error al crear usuario: El ID retornado es nulo.");
                    return BadRequest("No se pudo crear el usuario.");
                }

                _logger.Info($"Usuario creado exitosamente con ID: {userId}");

                return CreatedAtAction(nameof(CreateUser), new { id = userId }, new
                {
                    id = userId
                    // message = "Usuario creado exitosamente."
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error interno al crear usuario", ex);
                return StatusCode(500, "Error interno en el servidor.");
            }
        }
        #endregion

        #region UpdateUserById
        [HttpPatch("update/{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] UpdateUserDto userDto)
        {
            _logger.Info($"Iniciando solicitud PATCH /api/users/update/{id}");

            try
            {
                var result = await _mediator.Send(new UpdateUserCommand(userDto, id));

                if (!result)
                {
                    _logger.Warn($"No se pudo actualizar el usuario con ID: {id}");
                    return NotFound("El usuario no pudo ser actualizado.");
                }

                _logger.Info($"Usuario con ID {id} actualizado exitosamente.");
                return Ok("Usuario actualizado exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error interno al actualizar usuario con ID: {id}", ex);
                return StatusCode(500, "Error interno en el servidor.");
            }
        }
        #endregion

        #region GetUserActivityById
        [HttpGet("{UserId}/activity")]
        public async Task<IActionResult> GetByUserIdAsync([FromRoute] string UserId)
        {
            _logger.Info($"Iniciando solicitud GET /api/users/{UserId}/activity");

            try
            {
                var activities = await _mediator.Send(new UserActivityQuery(UserId, DateTime.MaxValue));

                if (activities == null || !activities.Any())
                {
                    _logger.Warn($"No se encontraron actividades para el usuario con ID: {UserId}");
                    return NotFound($"No se encontraron actividades para el usuario con ID {UserId}");
                }

                _logger.Info($"Actividades obtenidas exitosamente para el usuario con ID: {UserId}");
                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error interno al obtener actividades del usuario con ID: {UserId}", ex);
                return StatusCode(500, "Error interno en el servidor.");
            }
        }
        #endregion

        #region CreateUserActivity
        [HttpPost("publishActivity")]
        public async Task<IActionResult> PublishUserActivity([FromBody] CreateUserActivityDto createUserActivityDto)
        {
            _logger.Info($"Iniciando solicitud POST /api/users/publishActivity para el usuario {createUserActivityDto.UserId}");

            try
            {
                await _publishEndpoint.Publish(new UserActivityMadeEvent(
                    createUserActivityDto.UserId,
                    createUserActivityDto.Action,
                    DateTime.UtcNow
                ));

                _logger.Info($"Evento de actividad publicado correctamente para el usuario {createUserActivityDto.UserId}");
                return Ok("Evento de actividad publicado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error interno al publicar actividad para el usuario {createUserActivityDto.UserId}", ex);
                return StatusCode(500, "Error interno en el servidor.");
            }
        }
        #endregion

        #region GetUserByEmail
        [HttpGet("getuserbyemail")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            _logger.Info($"Iniciando solicitud GET /api/users/getuserbyemail para el email: {email}");

            try
            {
                var query = new GetUserByEmailQuery(email);
                var userDto = await _mediator.Send(query);

                if (userDto == null)
                {
                    _logger.Warn($"No se encontró un usuario con el email: {email}");
                    return NotFound($"No se encontró un usuario con el email {email}");
                }

                _logger.Info($"Usuario encontrado exitosamente con email: {email}");
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error interno al obtener usuario con email: {email}", ex);
                return StatusCode(500, "Error interno en el servidor.");
            }
        }
        #endregion

        #region GetUserById
        [HttpGet("getuserbyid/{id}")]
        public async Task<IActionResult> GetUserById([FromRoute] string id)
        {
            try
            {
                var query = new GetUserByIdQuery(id);
                var userDto = await _mediator.Send(query);

                if (userDto == null)
                {
                    return NotFound($"No se encontró un usuario con el id {id}");
                }

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno en el servidor.");
            }
        }
        #endregion

        #region GetUserByName
        [HttpGet("getuserbyname/{name}")]
        public async Task<IActionResult> GetUserByName([FromRoute] string name)
        {
            try
            {
                var query = new GetUserByNameQuery(name);
                var userDto = await _mediator.Send(query);
                if (userDto == null)
                {
                    return NotFound($"No se encontró un usuario con el nombre {name}");
                }

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno en el servidor.");
            }
        }
        #endregion

        #region UpdateUserRoleId
        [HttpPut("updaterole/{userId}")]
        public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] UpdateUserRoleDto updateUserRoleDto)
        {
            _logger.Info($"Iniciando solicitud PUT /api/users/updaterole/{userId}");

            try
            {
                var command = new UpdateUserRoleCommand(userId, updateUserRoleDto);
                var result = await _mediator.Send(command);

                if (result)
                {
                    _logger.Info($"El rol del usuario con ID {userId} se actualizó correctamente.");
                    return Ok("El rol del usuario se actualizó correctamente.");
                }

                _logger.Warn($"No se pudo actualizar el rol del usuario con ID {userId}.");
                return BadRequest("No se pudo actualizar el rol del usuario.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error interno al actualizar rol del usuario con ID {userId}", ex);
                return StatusCode(500, "Error interno en el servidor.");
            }
        }
        #endregion

        #region GetAllRoles
        [HttpGet("GetallRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            _logger.Info("Iniciando solicitud GET /api/roles/allRoles");

            try
            {
                var query = new GetAllRolesQuery();
                var roles = await _mediator.Send(query);

                if (roles == null || !roles.Any())
                {
                    _logger.Warn("No se encontraron roles en la base de datos.");
                    return NotFound("No se encontraron roles.");
                }

                _logger.Info($"Roles obtenidos exitosamente. Cantidad: {roles.Count}");
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.Error("Error interno al obtener roles.", ex);
                return StatusCode(500, "Error interno en el servidor.");
            }
        }
        #endregion

        #region GetAllUsers
        [HttpGet("allUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.Info("Iniciando solicitud GET /api/users/all");

            try
            {
                var query = new GetAllUsersQuery();
                var users = await _mediator.Send(query);
                if (users == null || !users.Any())
                {
                    _logger.Warn("No se encontraron usuarios.");
                    return NotFound("No se encontraron usuarios.");
                }
                _logger.Info("Solicitud completada exitosamente.");
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.Error("Error en GET /api/users/all", ex);
                return StatusCode(500, "Error interno en el servidor.");
            }
        }
        #endregion

        #region AddPermissionsToRole
        [HttpPost("{roleId}/add-permission/{permissionId}")]
        public async Task<IActionResult> AddPermissionToRole(string roleId, string permissionId)
        {
            _logger.Info($"Iniciando solicitud POST /api/roles/{roleId}/add-permission/{permissionId}");

            try
            {
                var command = new AddPermissionToRoleCommand(roleId, permissionId);
                var result = await _mediator.Send(command);

                if (!result)
                {
                    _logger.Warn($"Error al agregar el permiso {permissionId} al rol {roleId}.");
                    return BadRequest("Error al agregar el permiso.");
                }

                _logger.Info($"Permiso {permissionId} agregado correctamente al rol {roleId}.");
                return Ok("Permiso agregado correctamente al rol.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error interno al agregar permiso {permissionId} al rol {roleId}", ex);
                return StatusCode(500, "Error interno en el servidor.");
            }
        }
        #endregion

        #region RemovePermissionFromRole
        [HttpDelete("{roleId}/remove-permission/{permissionId}")]
        public async Task<IActionResult> RemovePermissionFromRole(string roleId, string permissionId)
        {
            _logger.Info($"Iniciando solicitud DELETE /api/roles/{roleId}/remove-permission/{permissionId}");

            try
            {
                var command = new RemovePermissionFromRoleCommand(roleId, permissionId);
                var result = await _mediator.Send(command);

                if (!result)
                {
                    _logger.Warn($"Error al eliminar el permiso {permissionId} del rol {roleId}.");
                    return BadRequest("Error al eliminar el permiso.");
                }

                _logger.Info($"Permiso {permissionId} eliminado correctamente del rol {roleId}.");
                return Ok("Permiso eliminado correctamente del rol.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error interno al eliminar permiso {permissionId} del rol {roleId}", ex);
                return StatusCode(500, "Error interno en el servidor.");
            }
        }
        #endregion

        #region GetAllPermissions
        [HttpGet("allPermissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            _logger.Info("Iniciando solicitud GET /api/permissions/allPermissions");

            try
            {
                var query = new GetAllPermissionsQuery();
                var permissions = await _mediator.Send(query);

                if (permissions == null || !permissions.Any())
                {
                    _logger.Warn("No se encontraron permisos en la base de datos.");
                    return NotFound("No se encontraron permisos.");
                }

                _logger.Info($"Permisos obtenidos exitosamente. Cantidad: {permissions.Count}");
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.Error("Error interno al obtener permisos.", ex);
                return StatusCode(500, "Error interno en el servidor.");
            }
        }
        #endregion

        #region GetPermissionsFromRole
        [HttpGet("{roleId}/permissions")]
        public async Task<IActionResult> GetPermissionsFromRole(string roleId)
        {
            _logger.Info($"Recibiendo solicitud para obtener permisos del rol {roleId}");

            var permissions = await _mediator.Send(new GetPermissionsByRoleIdQuery(roleId));

            if (permissions == null || !permissions.Any())
            {
                _logger.Warn($"No se encontraron permisos para el rol {roleId}");
                return NotFound(new { message = $"No se encontraron permisos para el rol con ID {roleId}" });
            }

            _logger.Info($"Permisos obtenidos para el rol {roleId}: {string.Join(", ", permissions)}");
            return Ok(permissions);
        }
        #endregion

    }

}
