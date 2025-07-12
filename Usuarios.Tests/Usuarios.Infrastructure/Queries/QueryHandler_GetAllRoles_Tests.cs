using Moq;

using Usuarios.Application.DTOs;

using Usuarios.Infrastructure.Interfaces;
using Usuarios.Infrastructure.Queries;
using Usuarios.Infrastructure.Queries.QueryHandlers;

namespace Usuarios.Tests.Usuarios.Infrastructure.Queries
{
    public class GetAllRolesQueryHandlerTests
    {
        private readonly Mock<IRoleReadRepository> _roleReadRepositoryMock;
        private readonly GetAllRolesQueryHandler _handler;

        public GetAllRolesQueryHandlerTests()
        {
            _roleReadRepositoryMock = new Mock<IRoleReadRepository>();
            _handler = new GetAllRolesQueryHandler(_roleReadRepositoryMock.Object);
        }

        #region Handle_ReturnsRoleList_WhenRolesExist
        [Fact]
        public async Task Handle_ReturnsRoleList_WhenRolesExist()
        {
            var query = new GetAllRolesQuery();
            var roles = new List<RoleDto>
        {
            new RoleDto { Id = "role1", Name = "Admin", Permissions = new List<string> { "Read", "Write" } },
            new RoleDto { Id = "role2", Name = "User", Permissions = new List<string> { "Read" } }
        };

            _roleReadRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(roles);

            var result = await _handler.Handle(query, CancellationToken.None);

            //Assert.Equal(2, result.Count);
            Assert.Equal("Admin", result.First().Name);
        }
        #endregion

        #region Handle_ReturnsEmptyList_WhenNoRolesExist
        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoRolesExist()
        {
            var query = new GetAllRolesQuery();

            _roleReadRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RoleDto>()); // Simulación de lista vacía

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Empty(result);
        }
        #endregion

        #region Handle_ThrowsException_WhenRepositoryFails
        [Fact]
        public async Task Handle_ThrowsException_WhenRepositoryFails()
        {
            var query = new GetAllRolesQuery();

            _roleReadRepositoryMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        }
        #endregion

    }
}
