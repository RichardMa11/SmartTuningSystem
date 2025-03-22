using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    public class SysConfigManager
    {
        public readonly SysConfigService SysConfigService = new SysConfigService();

        public int AddSysConfig(SysConfig sysConfig)
        {
            return SysConfigService.InsertSysConfig(sysConfig);
        }

        public void ModifySysConfig(SysConfig sysConfig)
        {
            SysConfigService.UpdateSysConfig(sysConfig);
        }

        public void RemoveSysConfig(SysConfig sysConfig)
        {
            SysConfigService.LogicDeleteSysConfig(sysConfig);
        }

        public void DeleteUser(int id)
        {
            SysConfigService.DeleteSysConfig(id);
        }

        public List<SysConfig> GetSysConfigByKey(string key)
        {
            return SysConfigService.SelectSysConfig(key);
        }
    }
}
