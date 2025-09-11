using System.Collections.Generic;
using System.Data.Entity;
using Model;
using System.Configuration;

namespace DAL
{
    public class CoreDbContext : DbContext
    {
        public CoreDbContext() : base(DbConnections.Get())
        {
            //首次运行时 不存在数据库 进行初始化
            Database.SetInitializer(new CreateDatabaseIfNotExists<CoreDbContext>());
        }

        #region 用户、系统角色、日志

        public DbSet<User> User { get; set; }//系统账户
        public DbSet<Role> Role { get; set; }//系统账户角色
        public DbSet<Log> Logs { get; set; }//日志

        public DbSet<DeviceInfo> DeviceInfo { get; set; }

        public DbSet<DeviceInfoDetail> DeviceInfoDetail { get; set; }

        public DbSet<Menu> Menu { get; set; }

        public DbSet<RoleMenu> RoleMenu { get; set; }

        public DbSet<SysConfig> SysConfig { get; set; }

        public DbSet<UserRole> UserRole { get; set; }

        public DbSet<TuningRecord> TuningRecord { get; set; }

        public DbSet<UserMenu> UserMenu { get; set; }

        public DbSet<InspectionLock> InspectionLock { get; set; }

        #endregion

        #region  权限相关

        //public DbSet<Plugins> Plugins { get; set; }//插件
        //public DbSet<PluginsModule> PluginsModule { get; set; }//插件中的模型
        //public DbSet<ModulePage> ModulePage { get; set; }//模型中的页面
        //public DbSet<RolePlugins> RolePlugins { get; set; }//角色的插件
        //public DbSet<UserPlugins> UserPlugins { get; set; }//用户的插件

        #endregion

        #region 部门 职位

        //public DbSet<Department> Department { get; set; }//部门
        //public DbSet<DepartmentPosition> DepartmentPosition { get; set; }//职位

        #endregion 

        #region 邮件

        //public DbSet<Email> Email { get; set; }//邮件
        //public DbSet<EmailSendTo> EmailSendTo { get; set; }//邮件发送信息

        #endregion 
    }

    public class DbConnections
    {
        public static Dictionary<string, string> ConStr = new Dictionary<string, string>();
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["SqlConnection"].ConnectionString;
        static DbConnections()
        {

        }

        public static void Set(string key, string value)
        {
            if (ConStr.ContainsKey(key)) ConStr[key] = value;
            else ConStr.Add(key, value);
        }

        /// <summary>
        /// 获取连接字符串 默认返回本机
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(string key)
        {
            return ConStr.ContainsKey(key) ? ConStr[key] : @"Data Source=localhost;Initial Catalog=SmartTuningSystemDB;User ID=sa;Password=Ma.20250217;";
        }

        public static string Get()
        {
            return ConnectionString;
        }
    }
}
