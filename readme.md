# SmartTuningSystem 解决方案使用

1.GitHub 下载源码；  
2.下载Sqlserver2019 及以上 ，还原数据库文件（SmartTuningSystemDB.bak）;  
3.修改数据库配置文件App.config -- "Data Source=localhost;Initial Catalog=SmartTuningSystemDB;User ID=sa;Password=Ma.20250217"；  
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

# 功能截图  
架构  
![image](https://github.com/RichardMa11/SmartTuningSystem/blob/master/%E6%95%88%E6%9E%9C%E5%9B%BE/%E6%9E%B6%E6%9E%84.png)  
登录  
![image](https://github.com/RichardMa11/SmartTuningSystem/blob/master/%E6%95%88%E6%9E%9C%E5%9B%BE/%E7%99%BB%E5%BD%95.png)  
欢迎页  
![image](https://github.com/RichardMa11/SmartTuningSystem/blob/master/%E6%95%88%E6%9E%9C%E5%9B%BE/%E6%AC%A2%E8%BF%8E%E9%A1%B5.png)  
首页  
![image](https://github.com/RichardMa11/SmartTuningSystem/blob/master/%E6%95%88%E6%9E%9C%E5%9B%BE/%E9%A6%96%E9%A1%B5.png)  
测试页  
![image](https://github.com/RichardMa11/SmartTuningSystem/blob/master/%E6%95%88%E6%9E%9C%E5%9B%BE/%E6%B5%8B%E8%AF%95%E9%A1%B5.png)  
调机记录  
![image](https://github.com/RichardMa11/SmartTuningSystem/blob/master/%E6%95%88%E6%9E%9C%E5%9B%BE/%E8%B0%83%E6%9C%BA%E8%AE%B0%E5%BD%95.png)  
人员管理  
![image](https://github.com/RichardMa11/SmartTuningSystem/blob/master/%E6%95%88%E6%9E%9C%E5%9B%BE/%E4%BA%BA%E5%91%98%E7%AE%A1%E7%90%86.png)  
菜单管理  
![image](https://github.com/RichardMa11/SmartTuningSystem/blob/master/%E6%95%88%E6%9E%9C%E5%9B%BE/%E8%8F%9C%E5%8D%95%E7%AE%A1%E7%90%86.png)  
角色授权  
![image](https://github.com/RichardMa11/SmartTuningSystem/blob/master/%E6%95%88%E6%9E%9C%E5%9B%BE/%E8%A7%92%E8%89%B2%E6%8E%88%E6%9D%83.png)  
菜单新增  
![image](https://github.com/RichardMa11/SmartTuningSystem/blob/master/%E6%95%88%E6%9E%9C%E5%9B%BE/%E8%8F%9C%E5%8D%95%E6%96%B0%E5%A2%9E.png)  
人员新增  
![image](https://github.com/RichardMa11/SmartTuningSystem/blob/master/%E6%95%88%E6%9E%9C%E5%9B%BE/%E4%BA%BA%E5%91%98%E6%96%B0%E5%A2%9E.png)  
密码修改  
![image](https://github.com/RichardMa11/SmartTuningSystem/blob/master/%E6%95%88%E6%9E%9C%E5%9B%BE/%E5%AF%86%E7%A0%81%E4%BF%AE%E6%94%B9.png)  


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
  
250327更新：  
修改密码  
新增账户  
新增角色  
  
250331更新：  
新增菜单  
  
250401更新：  
新增角色授权  
更新.BAK文件  

251231更新：  
完善业务逻辑  
更新.BAK文件 


# 目录介绍
SmartTuningSystem：程序入口，拥有登录、主页、设置、人员管理、角色授权、机台维护、智能调机等窗口，是主要的UI界面展示层；   
Model：基础数据模型（人员、菜单、权限、日志、设置、机台、产品）（EF 数据模型）；  
BLL：业务逻辑（人员、菜单、权限、日志、设置、机台、产品）；  
DAL: 数据交互，对数据库进行增删改查（人员、菜单、权限、日志、设置、机台、产品）；             

# 项目业务
取消技术员手动调机，实现人&机&测量3端互通，形成闭环。  
1.智能调机软件利用设备联网功能，自动抓取机台参数；  
2.区分首件，巡检，制程导入进行分析；  
3.测量数据能进行智能化数据公差分析，根据报告生成调机指导；  
4.尺寸一键补偿至对应机台；  
5.可批量操作机台补正,不受机台数量限制；  
6.数据可追溯性，每次补正机台参数均可被记录，包含：补正人员ID，数值，时间；              




