using System.Data.Entity;
using Model;

namespace DAL
{
    public class LogDbContext : DbContext
    {
        public DbSet<TuningRecord> TuningRecord { get; set; }
        public LogDbContext(string dbConnectionString) : base(dbConnectionString)
        {
            //首次运行时 不存在数据库 进行初始化
            Database.SetInitializer(new CreateDatabaseIfNotExists<LogDbContext>());
        }
    }
}
