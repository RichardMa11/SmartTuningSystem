using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    public class PermissionManager
    {
        public readonly PermissionService PermissionService = new PermissionService();

        public int AddPermission(Permission permission)
        {
            return PermissionService.InsertPermission(permission);
        }

        public void ModifyPermission(Permission permission)
        {
            PermissionService.UpdatePermission(permission);
        }

        public void RemovePermission(Permission permission)
        {
            PermissionService.LogicDeletePermission(permission);
        }

        public void DeleteRole(int id)
        {
            PermissionService.DeletePermission(id);
        }

        public List<Permission> GetAllPermission()
        {
            return PermissionService.SelectAllPermission();//select -get  insert -add  update -modify  delete -remove
        }

        //批量分配角色权限
        public void AddRolePermission(List<RolePermission> rolePermissions)
        {
            PermissionService.InsertRolePermission(rolePermissions);
        }

        public List<RolePermission> GetAllRolePermission()
        {
            return PermissionService.SelectAllRolePermission();
        }

        public void DeleteRolePermissionByRoleId(int roleId)
        {
            PermissionService.DeleteRolePermissionByRoleId(roleId);
        }
    }
}
