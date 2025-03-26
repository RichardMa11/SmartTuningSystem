using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace DAL
{
    public class UserService
    {

        public int InsertUser(User user)
        {
            User tmp;
            using (CoreDbContext context = new CoreDbContext())
            {
                //加入数据库
                var timeTmp = DateTime.Now;
                tmp = context.User.Add(new User
                {
                    UserName = user.UserName,
                    UserNo = user.UserNo,
                    UserPwd = user.UserPwd,
                    CanLogin = true,
                    CreateName = user.CreateName,
                    CreateNo = user.CreateNo,
                    CreateTime = timeTmp,
                    UpdateName = user.CreateName,
                    UpdateNo = user.CreateNo,
                    UpdateTime = timeTmp,
                    IsValid = true
                });

                context.SaveChanges();
            }

            return tmp.Id;
        }

        public void UpdateUser(User user, bool isCanLogin)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.User.Single(c => c.Id == user.Id);
                if (isCanLogin)
                    model.CanLogin = user.CanLogin;

                if (!string.IsNullOrEmpty(user.UserPwd))
                    model.UserPwd = user.UserPwd;

                model.UpdateName = user.UpdateName;
                model.UpdateNo = user.UpdateNo;
                model.UpdateTime = DateTime.Now;

                context.SaveChanges();
            }
        }

        public void DeleteUser(int id)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var user = context.User.FirstOrDefault(c => c.Id == id);
                if (user == null) return;
                context.User.Remove(user);
                context.SaveChanges();
            }
        }

        public void LogicDeleteUser(User user)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.Role.Single(c => c.Id == user.Id);
                model.DelName = user.DelName;
                model.DelNo = user.DelNo;
                model.DelTime = DateTime.Now;
                model.IsValid = false;

                context.SaveChanges();
            }
        }

        public List<User> SelectUser(DateTime startDate, DateTime endDate)
        {
            List<User> users;
            using (CoreDbContext context = new CoreDbContext())
            {
                users = context.User.Where(e => e.CreateTime >= startDate && e.CreateTime <= endDate && e.IsValid).ToList();
            }

            return users;
        }

        public List<User> SqlStrQueryUser(string strSql)
        {
            List<User> users;
            using (CoreDbContext context = new CoreDbContext())
            {
                users = context.User.SqlQuery(strSql).ToList();
            }

            return users;
        }

        public (List<UserRoleDto>, int) QueryPagedUser(string keyword, int pageIndex, int pageSize)
        {
            using (var db = new CoreDbContext())
            {
                var query = db.User
                    .Join(db.UserRole,                     // 关联第二张表
                        user => user.Id,
                        userRole => userRole.UserId,
                        (user, userRole) => new { User = user, UserRole = userRole })
                    .Join(db.Role,                        // 关联第三张表
                        combined => combined.UserRole.RoleId,
                        role => role.Id,
                        (combined, role) => new UserRoleDto
                        {
                            UserName = combined.User.UserName,
                            UserNo = combined.User.UserNo,
                            RoleName = role.RoleName,
                            RoleNo = role.RoleNo,
                            CreateTime = combined.User.CreateTime
                        })
                    .AsQueryable();

                // 动态条件过滤
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x =>
                        x.UserName.Contains(keyword) ||
                        x.UserNo.Contains(keyword) ||
                        x.RoleName.Contains(keyword));
                }

                // 分页操作
                int total = query.Count();
                var data = query.OrderByDescending(x => x.CreateTime)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return (data, total);
            }
        }
    }
}
