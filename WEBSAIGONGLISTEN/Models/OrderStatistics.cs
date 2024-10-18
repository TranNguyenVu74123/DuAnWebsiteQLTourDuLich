using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace WEBSAIGONGLISTEN.Models
{
    public class OrderStatistics
    {
        [Key]
        public int Id { get; set; }
        public int TotalOrders { get; set; } // Tổng số đơn hàng
        public string CategoryNamee { get; set; }

    }
}
