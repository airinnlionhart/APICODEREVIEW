using System.Collections.Generic;

namespace Candidate.Models
{
    public class Candidate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public List<int> Orgs { get; set; } 
        public List<bool> Questions { get; set; } 
    }
}
