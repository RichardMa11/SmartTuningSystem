using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Model;

namespace DAL
{
    public class RoleService
    {
        public int InsertRole(Role role)
        {
            Role tmp;
            using (CoreDbContext context = new CoreDbContext())
            {
                //加入数据库
                var timeTmp = DateTime.Now;
                tmp = context.Role.Add(new Role
                {
                    RoleName = role.RoleName,
                    RoleNo = role.RoleNo,
                    CreateName = role.CreateName,
                    CreateNo = role.CreateNo,
                    CreateTime = timeTmp,
                    UpdateName = role.CreateName,
                    UpdateNo = role.CreateNo,
                    UpdateTime = timeTmp,
                    IsValid = true
                });

                context.SaveChanges();
            }

            return tmp.Id;
        }

        public void UpdateRole(Role role)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.Role.Single(c => c.Id == role.Id);
                model.RoleName = role.RoleName;
                //model.RoleNo = role.RoleNo;
                model.UpdateName = role.UpdateName;
                model.UpdateNo = role.UpdateNo;
                model.UpdateTime = DateTime.Now;

                context.SaveChanges();
            }
        }

        public void DeleteRole(int id)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var role = context.Role.FirstOrDefault(c => c.Id == id);
                if (role == null) return;
                context.Role.Remove(role);
                context.SaveChanges();
            }
        }

        public void LogicDeleteRole(Role role)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.Role.Single(c => c.Id == role.Id);
                model.DelName = role.DelName;
                model.DelNo = role.DelNo;
                model.DelTime = DateTime.Now;
                model.IsValid = false;

                context.SaveChanges();
            }
        }

        public List<Role> SelectRole(DateTime startDate, DateTime endDate)
        {
            List<Role> roles;
            using (CoreDbContext context = new CoreDbContext())
            {
                roles = context.Role.Where(e => e.CreateTime >= startDate && e.CreateTime <= endDate && e.IsValid).ToList();
            }

            return roles;
        }

        public Role SelectRoleById(int roleId)
        {
            Role role;
            using (CoreDbContext context = new CoreDbContext())
            {
                role = context.Role.First(c => c.Id == roleId && c.IsValid);
            }

            return role;
        }

        public List<Role> SelectAllRole()
        {
            List<Role> roles;
            using (CoreDbContext context = new CoreDbContext())
            {
                roles = context.Role.Where(e => e.IsValid).ToList();
            }

            return roles;
        }

        public List<Role> SqlStrQueryRole(string strSql)
        {
            List<Role> roles;
            using (CoreDbContext context = new CoreDbContext())
            {
                roles = context.Role.SqlQuery(strSql).ToList();
            }

            return roles;
        }

        //分配人员角色
        public int InsertUserRole(UserRole userRole)
        {
            UserRole tmp;
            using (var context = new CoreDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var timeTmp = DateTime.Now;
                        var tmpUserRole = context.UserRole.FirstOrDefault(c => c.UserId == userRole.UserId);
                        if (tmpUserRole != null)
                        {
                            context.UserRole.Remove(tmpUserRole);
                            context.SaveChanges();
                        }

                        tmp = context.UserRole.Add(new UserRole
                        {
                            RoleId = userRole.RoleId,
                            UserId = userRole.UserId,
                            CreateName = userRole.CreateName,
                            CreateNo = userRole.CreateNo,
                            CreateTime = timeTmp,
                            UpdateName = userRole.CreateName,
                            UpdateNo = userRole.CreateNo,
                            UpdateTime = timeTmp,
                            IsValid = true
                        });

                        context.SaveChanges();
                        // 提交事务
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务（所有 SaveChanges 操作均撤销）
                        transaction.Rollback();
                        throw new ApplicationException("事务执行失败，已回滚", ex);
                    }
                }
            }

            return tmp.Id;
        }

        //分配人员角色2
        public void InsertUserRole(UserRoleDto2 userRoleDto2, User user)
        {
            using (var context = new CoreDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var timeTmp = DateTime.Now;
                        var tmp = context.User.Add(new User
                        {
                            UserName = userRoleDto2.UserName,
                            UserNo = userRoleDto2.UserNo,
                            UserPwd = userRoleDto2.UserPwd,
                            CanLogin = true,
                            CreateName = user.UserName,
                            CreateNo = user.UserNo,
                            CreateTime = timeTmp,
                            UpdateName = user.UserName,
                            UpdateNo = user.UserNo,
                            UpdateTime = timeTmp,
                            IsValid = true
                        });
                        context.SaveChanges();

                        var tmpUserRole = context.UserRole.FirstOrDefault(c => c.UserId == tmp.Id);
                        if (tmpUserRole != null)
                        {
                            context.UserRole.Remove(tmpUserRole);
                            context.SaveChanges();
                        }

                        context.UserRole.Add(new UserRole
                        {
                            RoleId = userRoleDto2.RoleId,
                            UserId = tmp.Id,
                            CreateName = user.UserName,
                            CreateNo = user.UserNo,
                            CreateTime = timeTmp,
                            UpdateName = user.UserName,
                            UpdateNo = user.UserNo,
                            UpdateTime = timeTmp,
                            IsValid = true
                        });
                        context.SaveChanges();

                        // 提交事务
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务（所有 SaveChanges 操作均撤销）
                        transaction.Rollback();
                        throw new ApplicationException("事务执行失败，已回滚", ex);
                    }
                }
            }
        }

        public void DeleteUserRole(int id)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                context.UserRole.Remove(context.UserRole.First(c => c.UserId == id));

                context.SaveChanges();
            }
        }

        //分配角色菜单
        public int InsertRoleMenu(RoleMenu roleMenu)
        {
            RoleMenu tmp;
            using (CoreDbContext context = new CoreDbContext())
            {
                var timeTmp = DateTime.Now;
                tmp = context.RoleMenu.Add(new RoleMenu
                {
                    RoleId = roleMenu.RoleId,
                    MenuId = roleMenu.MenuId,
                    CreateName = roleMenu.CreateName,
                    CreateNo = roleMenu.CreateNo,
                    CreateTime = timeTmp,
                    UpdateName = roleMenu.CreateName,
                    UpdateNo = roleMenu.CreateNo,
                    UpdateTime = timeTmp,
                    IsValid = true
                });

                context.SaveChanges();
                //var tmpRoleMenu = context.RoleMenu.FirstOrDefault(c => c.RoleId == roleMenu.RoleId);
                //if (tmpRoleMenu != null)
                //{
                //    context.RoleMenu.Remove(tmpRoleMenu);
                //    context.SaveChanges();
                //}
            }

            return tmp.Id;
        }

        //批量分配角色菜单
        public void InsertRoleMenu(List<RoleMenu> roleMenus)
        {
            using (var context = new CoreDbContext())
            {
                // 开始事务
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var timeTmp = DateTime.Now;
                        var roleId = roleMenus.First().RoleId;

                        // 批量删除旧数据（避免循环删除）
                        var oldRecords = context.RoleMenu.Where(c => c.RoleId == roleId).ToList();
                        context.RoleMenu.RemoveRange(oldRecords);
                        context.SaveChanges(); // 立即提交删除操作

                        // 批量插入新数据（避免循环插入）
                        var newRecords = roleMenus.Select(r => new RoleMenu
                        {
                            RoleId = r.RoleId,
                            MenuId = r.MenuId,
                            CreateName = r.CreateName,
                            CreateNo = r.CreateNo,
                            CreateTime = timeTmp,
                            UpdateName = r.CreateName,
                            UpdateNo = r.CreateNo,
                            UpdateTime = timeTmp,
                            IsValid = true
                        }).ToList();

                        context.RoleMenu.AddRange(newRecords);
                        context.SaveChanges();

                        // 提交事务
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务
                        transaction.Rollback();
                        throw new ApplicationException("事务执行失败，已回滚所有操作", ex);
                    }
                }
            }
        }

        public void DeleteRoleMenu(int id)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                context.RoleMenu.Remove(context.RoleMenu.First(c => c.RoleId == id));

                context.SaveChanges();
            }
        }
    }
}
