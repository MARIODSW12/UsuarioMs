using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Usuarios.Domain.Entities;
using Usuarios.Domain.ValueObjects;

namespace Usuarios.Domain.Factories
{
    public static class RoleFactory
    {
        public static Role Create(string roleId, string roleName, List<string> permissionsIds)
        {
            return new Role(new VORoleId(roleId), new VORoleName(roleName), new VORolePermissions(permissionsIds));
        }

        //public static Role Default()
        //{
        //    return new Role(new VORoleId("default"), new VORoleName("Guest"), new VORolePermissions(new List<string>()));
        //}
    }
}
