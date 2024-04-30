
using AppVentas.Enums;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace AppVentas.Entities
{
    [Table("purchases")]
    public class Purchase
    {
        [PrimaryKey, AutoIncrement]
        [Column("rowid")]
        public int Rowid { get; set; }
        [Required]
        [Column("creation_date")]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        [Required]
        [Column("rowid_producto")]
        public int RowidProducto { get; set; }
        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }
        [Required]
        [Column("producto")]
        public string Product { get; set; } = null!;
        [Column("state")]
        public EnumStatePurchase State { get; set; }
    }
}
