using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    public class CommEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string CreateNo { get; set; }//创建人工号
        public string CreateName { get; set; }//创建姓名
        public DateTime CreateTime { get; set; }//创建时间

        public string UpdateNo { get; set; }//更新人工号
        public string UpdateName { get; set; }//更新姓名
        public DateTime UpdateTime { get; set; }//更新时间

        public string DelNo { get; set; }
        public string DelName { get; set; }
        public DateTime? DelTime { get; set; }

        public bool IsValid { get; set; }
        public string Remark { get; set; }//备注
    }
}
