using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace DAL
{
    public class MenuService
    {

        public int InsertMenu(Menu menu)
        {
            Menu tmp;
            using (CoreDbContext context = new CoreDbContext())
            {
                //加入数据库
                var timeTmp = DateTime.Now;
                tmp = context.Menu.Add(new Menu
                {
                    PageName = menu.PageName,
                    PagePath = menu.PagePath,
                    Icon = menu.Icon,
                    Order = menu.Order,
                    CreateName = menu.CreateName,
                    CreateNo = menu.CreateNo,
                    CreateTime = timeTmp,
                    UpdateName = menu.CreateName,
                    UpdateNo = menu.CreateNo,
                    UpdateTime = timeTmp,
                    IsValid = true
                });

                context.SaveChanges();
            }

            return tmp.Id;
        }

        public void UpdateMenu(Menu menu)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.Menu.Single(c => c.Id == menu.Id);
                model.PageName = menu.PageName;
                model.PagePath = menu.PagePath;
                model.Icon = menu.Icon;
                model.Order = menu.Order;
                model.UpdateName = menu.UpdateName;
                model.UpdateNo = menu.UpdateNo;
                model.UpdateTime = DateTime.Now;

                context.SaveChanges();
            }
        }

        public void DeleteMenu(int id)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var menu = context.Menu.FirstOrDefault(c => c.Id == id);
                if (menu == null) return;
                context.Menu.Remove(menu);
                context.SaveChanges();
            }
        }

        public void LogicDeleteMenu(Menu menu)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.Menu.Single(c => c.Id == menu.Id);
                model.DelName = menu.DelName;
                model.DelNo = menu.DelNo;
                model.DelTime = DateTime.Now;
                model.IsValid = false;

                context.SaveChanges();
            }
        }

        public List<Menu> SelectAllMenu()
        {
            List<Menu> menus;
            using (CoreDbContext context = new CoreDbContext())
            {
                menus = context.Menu.Where(e => e.IsValid).ToList();
            }

            return menus;
        }

        public List<Menu> SelectMenuByNameAndPath(string name, string path)
        {
            List<Menu> menus;
            using (CoreDbContext context = new CoreDbContext())
            {
                menus = context.Menu.Where(e => e.PageName == name && e.PagePath == path && e.IsValid).ToList();
            }

            return menus;
        }

        //分配人员菜单(前提人员账号已有)
        public int InsertUserMenu(UserMenu userMenu)
        {
            UserMenu tmp;
            using (CoreDbContext context = new CoreDbContext())
            {
                var timeTmp = DateTime.Now;
                tmp = context.UserMenu.Add(new UserMenu
                {
                    UserId = userMenu.UserId,
                    IncreaseMenus = userMenu.IncreaseMenus,
                    DecrementMenus = userMenu.DecrementMenus,
                    CreateName = userMenu.CreateName,
                    CreateNo = userMenu.CreateNo,
                    CreateTime = timeTmp,
                    UpdateName = userMenu.CreateName,
                    UpdateNo = userMenu.CreateNo,
                    UpdateTime = timeTmp,
                    IsValid = true
                });

                context.SaveChanges();
            }

            return tmp.Id;
        }

        public void UpdateUserMenu(UserMenu userMenu)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.UserMenu.Single(c => c.UserId == userMenu.UserId);
                model.IncreaseMenus = userMenu.IncreaseMenus;
                model.DecrementMenus = userMenu.DecrementMenus;
                model.UpdateName = userMenu.UpdateName;
                model.UpdateNo = userMenu.UpdateNo;
                model.UpdateTime = DateTime.Now;

                context.SaveChanges();
            }
        }

        public List<UserMenu> SelectAllUserMenu()
        {
            List<UserMenu> userMenus;
            using (CoreDbContext context = new CoreDbContext())
            {
                userMenus = context.UserMenu.Where(e => e.IsValid).ToList();
            }

            return userMenus;
        }

        public List<RoleMenu> SelectAllRoleMenu()
        {
            List<RoleMenu> roleMenus;
            using (CoreDbContext context = new CoreDbContext())
            {
                roleMenus = context.RoleMenu.Where(e => e.IsValid).ToList();
            }

            return roleMenus;
        }
    }
}
