using System.ComponentModel.DataAnnotations;

namespace WEBSAIGONGLISTEN.Models
{
    public class Comment
    {
        public int Id { get; set; } // Khóa chính

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        [Range(1, 5)] // Giới hạn đánh giá từ 1 đến 5 sao
        public int Rating { get; set; }

        public int ProductId { get; set; } // Khóa ngoại

        public Product Product { get; set; }
    }
}
