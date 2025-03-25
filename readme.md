# SmartTuningSystem 解决方案使用

1.GitHub 下载源码；
2.下载Sqlserver2019 及以上 ，还原数据库文件（SmartTuningSystemDB.bak）;
3.修改数据库配置文件App.config -- "Data Source=localhost;Initial Catalog=SmartTuningSystemDB;User ID=sa;Password=Ma.20250217";
4.生成，运行解决方案;

# WPF开发的CNC智能调机系统（数据库MSSqlserver2022）

项目采用3层架构  
1.表示层（Presentation Layer）或者UI层
2.业务逻辑层（Business Logic Layer）
3.数据访问层（Data Access Layer）
4.实体层（Model或叫Entity）
提高代码的可维护性、可扩展性和可重用性,在开发时可以更好的业务分离,提高多人协作开发效率.  

# 项目中使用到的技术

NETFramework472  
Panuon.UI.Silver  
LiveCharts  
NPOI.Excel  
NLog  
Newtonsoft.Json  
EntityFramework(Code First)  

开发环境：  
VS2019  
Sqlserver2019 及以上


# 正在进行中（持续开发中，尚不完善）

新功能开发  

# 更新日志

250322更新：
登录功能  
主页功能
管理中心（人员管理、角色授权、机台维护、智能调机等）  

250325更新：
首页
调机日志
更新数据库.BAK文件

# 目录介绍
SmartTuningSystem：程序入口，拥有登录、主页、设置、人员管理、角色授权、机台维护、智能调机等窗口，是主要的UI界面展示层；  
Model：基础数据模型（人员、菜单、权限、日志、设置、机台、产品）（EF 数据模型）  
BLL：业务逻辑（人员、菜单、权限、日志、设置、机台、产品）
DAL: 数据交互，对数据库进行增删改查（人员、菜单、权限、日志、设置、机台、产品）

# 项目业务
取消技术员手动调机，实现人&机&测量3端互通，形成闭环。
1.智能调机软件利用设备联网功能，自动抓取机台参数；
2.区分首件，巡检，制程导入进行分析；
3.测量数据能进行智能化数据公差分析，根据报告生成调机指导；
4.尺寸一键补偿至对应机台
5.可批量操作机台补正,不受机台数量限制
6.数据可追溯性，每次补正机台参数均可被记录，包含：补正人员ID，数值，时间；




