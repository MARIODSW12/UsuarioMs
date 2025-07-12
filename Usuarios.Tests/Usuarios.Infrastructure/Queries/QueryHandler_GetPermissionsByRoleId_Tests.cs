using Moq;

using Usuarios.Infrastructure.Interfaces;
using Usuarios.Infrastructure.Queries;
using Usuarios.Infrastructure.Queries.QueryHandlers;

namespace Usuarios.Tests.Usuarios.Infrastructure.Queries
{
    public class GetPermissionsByRoleIdTests
    {
        private readonly Mock<IRoleReadRepository> _roleReadRepositoryMock;
        private readonly GetPermissionsByRoleIdQueryHandler _handler;

        public GetPermissionsByRoleIdTests()
        {
            _roleReadRepositoryMock = new Mock<IRoleReadRepository>();
            _handler = new GetPermissionsByRoleIdQueryHandler(_roleReadRepositoryMock.Object);
        }

        #region Handle_ReturnsPermissions_WhenRoleExists
        [Fact]
        public async Task Handle_ReturnsPermissions_WhenRoleExists()
        {
            var roleId = "role123";
            var permissions = new List<string> { "perm1", "perm2", "perm3" };

            _roleReadRepositoryMock.Setup(repo => repo.GetPermissionsByRoleIdAsync(roleId))
                .ReturnsAsync(permissions);

            var query = new GetPermissionsByRoleIdQuery(roleId);

            var result = await _handler.Handle(query, CancellationToken.None);

            //Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            //Assert.Contains("perm1", result);
            //Assert.Contains("perm2", result);
            //Assert.Contains("perm3", result);
        }
        #endregion

        #region Handle_ReturnsNull_WhenRoleDoesNotExist
        [Fact]
        public async Task Handle_ReturnsNull_WhenRoleDoesNotExist()
        {
            var roleId = "nonexistent_role";

            _roleReadRepositoryMock.Setup(repo => repo.GetPermissionsByRoleIdAsync(roleId))
                .ReturnsAsync((List<string>?)null);

            var query = new GetPermissionsByRoleIdQuery(roleId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Null(result);
        }
        #endregion

        #region Handle_ReturnsEmptyList_WhenRoleHasNoPermissions
        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenRoleHasNoPermissions()
        {
            var roleId = "role123";
            var permissions = new List<string>(); // ✅ Lista vacía

            _roleReadRepositoryMock.Setup(repo => repo.GetPermissionsByRoleIdAsync(roleId))
                .ReturnsAsync(permissions);

            var query = new GetPermissionsByRoleIdQuery(roleId);

            var result = await _handler.Handle(query, CancellationToken.None);

            //Assert.NotNull(result);
            Assert.Empty(result);
        }
        #endregion

        #region Handle_ThrowsException_WhenRepositoryFails
        [Fact]
        public async Task Handle_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            var roleId = "role123";

            _roleReadRepositoryMock.Setup(repo => repo.GetPermissionsByRoleIdAsync(roleId))
                .ThrowsAsync(new Exception("Database error"));

            var query = new GetPermissionsByRoleIdQuery(roleId);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        }
        #endregion

    }
}
