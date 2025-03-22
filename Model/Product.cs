using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 产品
    /// </summary>
    [Table("Product")]
    public class Product : CommEntity
    {
        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public int DeviceId { get; set; }
    }
}
