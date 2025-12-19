using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace DAL
{
    public class PermissionService
    {

        public int InsertPermission(Permission permission)
        {
            Permission tmp;
            using (CoreDbContext context = new CoreDbContext())
            {
                //加入数据库
                var timeTmp = DateTime.Now;
                tmp = context.Permission.Add(new Permission
                {
                    PermissionCode = permission.PermissionCode,
                    PermissionName = permission.PermissionName,
                    CreateName = permission.CreateName,
                    CreateNo = permission.CreateNo,
                    CreateTime = timeTmp,
                    UpdateName = permission.CreateName,
                    UpdateNo = permission.CreateNo,
                    UpdateTime = timeTmp,
                    IsValid = true
                });

                context.SaveChanges();
            }

            return tmp.Id;
        }

        public void UpdatePermission(Permission permission)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.Permission.Single(c => c.Id == permission.Id);
                model.PermissionName = permission.PermissionName;
                model.UpdateName = permission.UpdateName;
                model.UpdateNo = permission.UpdateNo;
                model.UpdateTime = DateTime.Now;

                context.SaveChanges();
            }
        }

        public void DeletePermission(int id)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var permission = context.Permission.FirstOrDefault(c => c.Id == id);
                if (permission == null) return;
                context.Permission.Remove(permission);
                context.SaveChanges();
            }
        }

        public void LogicDeletePermission(Permission permission)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.Permission.Single(c => c.Id == permission.Id);
                model.DelName = permission.DelName;
                model.DelNo = permission.DelNo;
                model.DelTime = DateTime.Now;
                model.IsValid = false;

                context.SaveChanges();
            }
        }

        public List<Permission> SelectAllPermission()
        {
            List<Permission> permissions;
            using (CoreDbContext context = new CoreDbContext())
            {
                permissions = context.Permission.Where(e => e.IsValid).ToList();
            }

            return permissions;
        }

        public void InsertRolePermission(List<RolePermission> rolePermissions)
        {
            using (var context = new CoreDbContext())
            {
                // 开始事务
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var timeTmp = DateTime.Now;
                        var roleId = rolePermissions.First().RoleId;

                        // 批量删除旧数据（避免循环删除）
                        var oldRecords = context.RolePermission.Where(c => c.RoleId == roleId).ToList();
                        context.RolePermission.RemoveRange(oldRecords);
                        context.SaveChanges(); // 立即提交删除操作

                        // 批量插入新数据（避免循环插入）
                        var newRecords = rolePermissions.Select(r => new RolePermission
                        {
                            RoleId = r.RoleId,
                            PermissionId = r.PermissionId,
                            CreateName = r.CreateName,
                            CreateNo = r.CreateNo,
                            CreateTime = timeTmp,
                            UpdateName = r.CreateName,
                            UpdateNo = r.CreateNo,
                            UpdateTime = timeTmp,
                            IsValid = true
                        }).ToList();

                        context.RolePermission.AddRange(newRecords);
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

        public List<RolePermission> SelectAllRolePermission()
        {
            List<RolePermission> rolePermissions;
            using (CoreDbContext context = new CoreDbContext())
            {
                rolePermissions = context.RolePermission.Where(e => e.IsValid).ToList();
            }

            return rolePermissions;
        }

        public void DeleteRolePermissionByRoleId(int roleId)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                // 批量删除旧数据（避免循环删除）
                var oldRecords = context.RolePermission.Where(c => c.RoleId == roleId).ToList();
                context.RolePermission.RemoveRange(oldRecords);
                context.SaveChanges(); // 立即提交删除操作
            }
        }
    }
}
