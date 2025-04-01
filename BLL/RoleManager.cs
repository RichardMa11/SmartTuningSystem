using System;
using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    public class RoleManager
    {
        public readonly RoleService RoleService = new RoleService();

        public int AddRole(Role role)
        {
            return RoleService.InsertRole(role);
        }

        public void ModifyRole(Role role)
        {
            RoleService.UpdateRole(role);
        }

        public void RemoveRole(Role role)
        {
            RoleService.LogicDeleteRole(role);
        }

        public void DeleteRole(int id)
        {
            RoleService.DeleteRole(id);
        }

        public List<Role> GetRoleByDate(DateTime startDate, DateTime endDate)
        {
            return RoleService.SelectRole(startDate, endDate);
        }

        public Role GetRoleById(int roleId)
        {
            return RoleService.SelectRoleById(roleId);
        }


        public List<Role> GetAllRole()
        {
            return RoleService.SelectAllRole();
        }

        public List<Role> GetRoleBySql(string strSql)
        {
            return RoleService.SqlStrQueryRole(strSql);
        }

        //添加人员角色
        public int AddUserRole(UserRole userRole)
        {
            return RoleService.InsertUserRole(userRole);
        }

        //添加人员角色2
        public void AddUserRole(UserRoleDto2 userRoleDto2, User user)
        {
            RoleService.InsertUserRole(userRoleDto2, user);
        }

        //分配角色菜单
        public int AddRoleMenu(RoleMenu roleMenu)
        {
            return RoleService.InsertRoleMenu(roleMenu);
        }

        //批量分配角色菜单
        public void AddRoleMenu(List<RoleMenu> roleMenus)
        {
            RoleService.InsertRoleMenu(roleMenus);
        }
    }
}
