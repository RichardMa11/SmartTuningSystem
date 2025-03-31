using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    public class MenuManager
    {
        public readonly MenuService MenuService = new MenuService();

        public int AddMenu(Menu menu)
        {
            return MenuService.InsertMenu(menu);
        }

        public void ModifyMenu(Menu menu)
        {
            MenuService.UpdateMenu(menu);
        }

        public void RemoveMenu(Menu menu)
        {
            MenuService.LogicDeleteMenu(menu);
        }

        public void DeleteMenu(int id)
        {
            MenuService.DeleteMenu(id);
        }

        public List<Menu> GetAllMenu()
        {
            return MenuService.SelectAllMenu();
        }

        public int AddUserMenu(UserMenu userMenu)
        {
            return MenuService.InsertUserMenu(userMenu);
        }

        public List<UserMenu> GetAllUserMenu()
        {
            return MenuService.SelectAllUserMenu();
        }
    }
}
