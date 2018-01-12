using System.ComponentModel.DataAnnotations;

namespace Com.Gmlan.Core.Model
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public bool Deleted { get; set; }
    }
}
