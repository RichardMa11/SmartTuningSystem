using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace DAL
{
    public class SysConfigService
    {

        public int InsertSysConfig(SysConfig sysConfig)
        {
            SysConfig tmp;
            using (CoreDbContext context = new CoreDbContext())
            {
                //加入数据库
                var timeTmp = DateTime.Now;
                tmp = context.SysConfig.Add(new SysConfig
                {
                    Key = sysConfig.Key,
                    Value = sysConfig.Value,
                    CreateName = sysConfig.CreateName,
                    CreateNo = sysConfig.CreateNo,
                    CreateTime = timeTmp,
                    UpdateName = sysConfig.CreateName,
                    UpdateNo = sysConfig.CreateNo,
                    UpdateTime = timeTmp,
                    IsValid = true,
                    Remark = sysConfig.Remark
                });

                context.SaveChanges();
            }

            return tmp.Id;
        }

        public void UpdateSysConfig(SysConfig sysConfig)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.SysConfig.Single(c => c.Id == sysConfig.Id);
                model.Key = sysConfig.Key;
                model.Value = sysConfig.Value;
                model.UpdateName = sysConfig.UpdateName;
                model.UpdateNo = sysConfig.UpdateNo;
                model.UpdateTime = DateTime.Now;

                context.SaveChanges();
            }
        }

        public void DeleteSysConfig(int id)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var sysConfig = context.SysConfig.FirstOrDefault(c => c.Id == id);
                if (sysConfig == null) return;
                context.SysConfig.Remove(sysConfig);
                context.SaveChanges();
            }
        }

        public void LogicDeleteSysConfig(SysConfig sysConfig)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.SysConfig.Single(c => c.Id == sysConfig.Id);
                model.DelName = sysConfig.DelName;
                model.DelNo = sysConfig.DelNo;
                model.DelTime = DateTime.Now;
                model.IsValid = false;

                context.SaveChanges();
            }
        }

        public List<SysConfig> SelectSysConfig(string key)
        {
            List<SysConfig> sysConfigs;
            using (CoreDbContext context = new CoreDbContext())
            {
                sysConfigs = context.SysConfig.Where(e => e.Key == key && e.IsValid).ToList();
            }

            return sysConfigs;
        }
    }
}
