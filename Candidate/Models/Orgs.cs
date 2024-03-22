using System.Collections.Generic;

namespace Candidate.Models
{
    public class Org
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MinAge { get; set; }
        public List<bool> Questions { get; set; } 
    }
}
