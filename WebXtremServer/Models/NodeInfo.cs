using System.ComponentModel.DataAnnotations;

namespace FmCndoeServer.Models
{
  
    public class NodeInfo
    {
        [Required]
        public int Id { get; set; }
        [Range(1, 100)]
        public int ConcurrentCount { get; set; }
        [Required]
        public String Name { get; set; }
        [Required]
        public int Memory { get; set; }
        [Required]
        public int Cpu { get; set; }
        [Required]
        public string IP { get; set; }

      
    }
}
