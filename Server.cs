using ElfKingdom;
using System.Collections.Generic; 
using System.Linq;

namespace MyBot
{
    public class Server
    {
        public static bool set_up = true;
        static bool Need_Sum = false;
        
        public Server()
        { }
        
        public void done()
        { set_up = false; }
        
        public void setting()
        { set_up = true; }
        
        public bool Get_Set()
        { return set_up; }
        
        ///////////////////////////////////////
        
        public void help()
        { Need_Sum = true; }
        
        public void deal()
        { Need_Sum = false; }
        
        public bool Get_Sum()
        { return Need_Sum; }
    }
}