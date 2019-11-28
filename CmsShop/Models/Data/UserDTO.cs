using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CmsShop.Models.Data
{
    [Table("tblUsers")]
    public class UserDTO
    {
        [Key]
        public int Id { get; set; }
        public string FirsName { get; set; }
        public string LastName { get; set; }
        public string EmailAddres { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}