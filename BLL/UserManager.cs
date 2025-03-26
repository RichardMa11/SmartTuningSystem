using System;
using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    public class UserManager
    {
        public readonly UserService UserService = new UserService();

        public int AddUser(User user)
        {
            return UserService.InsertUser(user);
        }

        public void ModifyUser(User user, bool isCanLogin)
        {
            UserService.UpdateUser(user, isCanLogin);
        }

        public void RemoveUser(User user)
        {
            UserService.LogicDeleteUser(user);
        }

        public void DeleteUser(int id)
        {
            UserService.DeleteUser(id);
        }

        public List<User> GetUserByDate(DateTime startDate, DateTime endDate)
        {
            return UserService.SelectUser(startDate, endDate);
        }

        public List<User> GetUserBySql(string strSql)
        {
            return UserService.SqlStrQueryUser(strSql);
        }

        public (List<UserRoleDto>, int) GetPagedUser(string keyword, int pageIndex, int pageSize)
        {
            return UserService.QueryPagedUser(keyword, pageIndex, pageSize);
        }
    }
}
