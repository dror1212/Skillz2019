using ElfKingdom;
using System.Collections.Generic;
using System.Linq;

namespace MyBot
{
    public class HandlePort
    {
        Game game;
        Castle EnemyCastle;
        LavaGiant[] EnemyCreatures;
        Portal DefPortal;
        Portal AttPortal;
        List<Portal> Myp;
        bool CanAtt = false;
        Server server;
        public HandlePort(Game game)
        {
            this.game = game;
            this.EnemyCastle = game.GetEnemyCastle();
            this.EnemyCreatures = game.GetEnemyLavaGiants();
            this.server = new Server();
            this.Myp = (from element in game.GetMyPortals() orderby element.Distance(game.GetMyCastle()) select element).ToList();
             
             
            if(Myp.Count == 1)
            { this.DefPortal = Myp[0]; }
            else if(Myp.Count > 1)
            {
                this.DefPortal = Myp[0];
                this.AttPortal = Myp[Myp.Count - 1];
            }
        }
        
        
        //////////////// Main Portasls
        
        public void main()
        {
            if((game.GetMyPortals().Length > 0 && game.GetEnemyPortals().Length == 0 && game.GetEnemyLivingElves().Length == 0)||(game.GetMyPortals().Length > 0&&game.GetEnemyCastle().CurrentHealth<10&&game.GetMyCastle().CurrentHealth>game.GetEnemyCastle().CurrentHealth))
            {
                List<Portal> portals = (from element in game.GetMyPortals() orderby element.Distance(game.GetEnemyCastle()) select element).ToList();
                foreach(Portal p in portals)
                {
                    if(p.CanSummonLavaGiant())
                        p.SummonLavaGiant();
                }
            }
            else if(game.GetMyPortals().Length == 1)
            {
                OnePortal();
            }
            if(game.GetMyPortals().Length == 2)
            {
                TwoPortal();
            }
            else if(game.GetMyPortals().Length > 2)
            {
                ThreePortal();
            }
        }
        
        public void OnePortal()
        {
            List<LavaGiant> lava_sorted = (from element in game.GetEnemyLavaGiants() orderby element.Distance(game.GetMyCastle()) select element).ToList();
            
            protect_the_portal();

            if(EnemyCreatures.Length != 0)
            {
                if(DefTurn(lava_sorted[0], DefPortal) && server.Get_Sum())
                {
                    if(DefPortal.CanSummonIceTroll())
                    { DefPortal.SummonIceTroll(); } 
                }
            }
        }
        
        public void TwoPortal()
        {
            int attack = Attack_CheckLava();

            bool b = protect_portal();
            if(!b||game.GetMyMana()>=game.TornadoCost+game.LavaGiantCost+game.IceTrollCost)
            {
                if(attack == game.LavaGiantMaxHealth / 2 && game.GetEnemyLivingElves().Length <= game.GetMyLivingElves().Length)
                {
                    if(AttPortal.CanSummonLavaGiant() && game.GetMyMana() >= game.LavaGiantCost + game.PortalCost)
                    { AttPortal.SummonLavaGiant(); }
    
                    protect_the_portal();
                }
                else if(attack == game.LavaGiantMaxHealth / 3 && game.GetEnemyLivingElves().Length <= game.GetMyLivingElves().Length)
                {
                    if(game.GetMyMana() > game.LavaGiantCost + game.GetMyself().ManaPerTurn * 2 && !server.Get_Set())
                    {
                        if(AttPortal.CanSummonLavaGiant())
                        { AttPortal.SummonLavaGiant(); } 
                    }
                    
                    List<LavaGiant> lava_sorted = (from element in game.GetEnemyLavaGiants() orderby element.Distance(game.GetMyCastle()) select element).ToList();
                    
                    protect_the_portal();
                    
                    if(EnemyCreatures.Length != 0)
                    {
                        if(DefTurn(lava_sorted[0], DefPortal) && server.Get_Sum())
                        {
                            if(DefPortal.CanSummonIceTroll())
                            { DefPortal.SummonIceTroll(); } 
                        }
                    }
                }
                
                else
                {
                    List<LavaGiant> lava_sorted = (from element in game.GetEnemyLavaGiants() orderby element.Distance(game.GetMyCastle()) select element).ToList();
                    
                    protect_the_portal();
    
                    if(EnemyCreatures.Length != 0)
                    {
                        if(DefTurn(lava_sorted[0], DefPortal) && server.Get_Sum())
                        {
                            if(DefPortal.CanSummonIceTroll())
                            { DefPortal.SummonIceTroll(); } 
                        }
                    }
                    if(game.GetMyMana() + game.GetMyself().ManaPerTurn * 4 > 150 && !server.Get_Set())
                    {
                        if(AttPortal.CanSummonLavaGiant())
                        { AttPortal.SummonLavaGiant(); } 
                    }
                }
            }
            else
                protect_the_portal();
        }
        
        public void ThreePortal()
        {
            int attack = Attack_CheckLava();
            bool b = protect_portal();
            if(!b||game.GetMyMana()>=game.TornadoCost+game.LavaGiantCost+game.IceTrollCost)
            {
                if(attack == game.LavaGiantMaxHealth / 2 && game.GetEnemyLivingElves().Length <= game.GetMyLivingElves().Length)
                {
                    if(game.GetMyPortals().Length < 4)
                    {
                        if(AttPortal.CanSummonLavaGiant() && game.GetMyMana() >= game.LavaGiantCost + game.PortalCost)
                        { AttPortal.SummonLavaGiant(); }
                    }
                    else
                    {
                        if(AttPortal.CanSummonLavaGiant())
                        { AttPortal.SummonLavaGiant(); }
                    }
                    
                    
                    protect_the_portal();
                    
                }
                else if(attack == game.LavaGiantMaxHealth / 3 && game.GetEnemyLivingElves().Length <= game.GetMyLivingElves().Length)
                {
                    
                    if(game.GetMyMana() > game.LavaGiantCost + game.GetMyself().ManaPerTurn * 2 && !server.Get_Set())
                    {
                        if(AttPortal.CanSummonLavaGiant())
                        { AttPortal.SummonLavaGiant(); } 
                    }
                    
                    List<LavaGiant> lava_sorted = (from element in game.GetEnemyLavaGiants() orderby element.Distance(game.GetMyCastle()) select element).ToList();
                    
                    protect_the_portal();
                    
                    if(EnemyCreatures.Length != 0)
                    {
                        if(DefTurn(lava_sorted[0], DefPortal) && server.Get_Sum())
                        {
                            if(DefPortal.CanSummonIceTroll())
                            { DefPortal.SummonIceTroll(); } 
                        }
                    }
                }
                else
                {
                    List<LavaGiant> lava_sorted = (from element in game.GetEnemyLavaGiants() orderby element.Distance(game.GetMyCastle()) select element).ToList();
                    
                    protect_the_portal();
                    
                    if(EnemyCreatures.Length != 0)
                    {
                        if(DefTurn(lava_sorted[0], DefPortal) && server.Get_Sum())
                        {
                            if(DefPortal.CanSummonIceTroll())
                            { DefPortal.SummonIceTroll(); } 
                        }
                    }
                    
                    if(game.GetMyMana() + game.GetMyself().ManaPerTurn * 4 > 150 && !server.Get_Set())
                    {
                        if(AttPortal.CanSummonLavaGiant())
                        { AttPortal.SummonLavaGiant(); } 
                    }
                }
            }
            else
                protect_the_portal();
        }
        
        ///////// Cal if need defend
        
        public List<Location> AttPath(LavaGiant ene)
        {
            List<Location> AttackPath = new List<Location>();
            Location start = ene.GetLocation();
            AttackPath.Add(start);
            while(!start.InRange(game.GetMyCastle(), game.LavaGiantAttackRange + game.GetMyCastle().Size))
            {
                start = start.Towards(game.GetMyCastle(), game.LavaGiantMaxSpeed);
                AttackPath.Add(start);
            }
            return AttackPath;
        }
        
        public bool DefTurn(LavaGiant giant, Portal defn)
        {
            
            Location ice = defn.Location;
            List<Location> Attp = AttPath(giant);
            int hplava = giant.CurrentHealth;
            
            
            if(game.GetMyIceTrolls().Length == 0)
            {
                for(int i = 0; i < Attp.Count; i++)
                {
                    if(i <= game.IceTrollSummoningDuration - 1)
                    { }
                    else if(!ice.InRange(Attp[i], game.IceTrollAttackRange))
                    {ice = ice.Towards(Attp[i], game.IceTrollMaxSpeed); }
                    else
                    { hplava = hplava - game.IceTrollAttackMultiplier; }
                    
                    hplava = hplava - giant.SuffocationPerTurn;
                }
                if(hplava != 0)
                { return true; } 
                else
                { return false; }
            }
            else
            {
                //////////// Backup
                
                /// Trolls Info
                Dictionary<int, Location> troll_loc = new Dictionary<int, Location>();
                Dictionary<int, int> troll_hp = new Dictionary<int, int>();
                Dictionary<int, bool> troll_act = new Dictionary<int, bool>();
                foreach(IceTroll againt in game.GetMyIceTrolls())
                { 
                    troll_hp.Add(againt.Id, againt.CurrentHealth); 
                    troll_loc.Add(againt.Id, againt.GetLocation());
                    troll_act.Add(againt.Id, false);
                    
                }
                
                /// Lava Info
                Dictionary<int, Location> lava_loc = new Dictionary<int, Location>();
                Dictionary<int, int> lava_hp = new Dictionary<int, int>();
                foreach(LavaGiant againt in game.GetMyLavaGiants())
                {
                    lava_hp.Add(againt.Id, againt.CurrentHealth); 
                    lava_loc.Add(againt.Id, againt.GetLocation());
                }
                
                /// Tornado Info
                Dictionary<int, Location> tornado_loc = new Dictionary<int, Location>();
                Dictionary<int, int> tornado_hp = new Dictionary<int, int>();
                foreach(Tornado againt in game.GetMyTornadoes())
                {
                    tornado_hp.Add(againt.Id, againt.CurrentHealth); 
                    tornado_loc.Add(againt.Id, againt.GetLocation());
                }
                
                int counter = 1;
                /// Buildings Info
                Dictionary<int, Location> buildings_loc = new Dictionary<int, Location>();
                Dictionary<int, int> buildings_hp = new Dictionary<int, int>();
                foreach(Building againt in game.GetMyManaFountains())
                {
                    buildings_hp.Add(counter, againt.CurrentHealth); 
                    buildings_loc.Add(counter, againt.GetLocation());
                    counter++;
                }
                counter = -1;
                foreach(Building againt in game.GetMyPortals())
                {
                    buildings_hp.Add(counter, againt.CurrentHealth); 
                    buildings_loc.Add(counter, againt.GetLocation());
                    counter--;
                }
                
                /// Enemy Lava Info
                Dictionary<int, Location> lava_enemy_loc = new Dictionary<int, Location>();
                Dictionary<int, int> lava_enemy_hp = new Dictionary<int, int>();
                foreach(LavaGiant againt in game.GetEnemyLavaGiants())
                {
                    lava_enemy_hp.Add(againt.Id, againt.CurrentHealth); 
                    lava_enemy_loc.Add(againt.Id, againt.GetLocation());
                }
                
                /// Enemy Troll Info
                Dictionary<int, Location> troll_enemy_loc = new Dictionary<int, Location>();
                Dictionary<int, int> troll_enemy_hp = new Dictionary<int, int>();
                Dictionary<int, bool> troll_enemy_act = new Dictionary<int, bool>();
                foreach(IceTroll againt in game.GetEnemyIceTrolls())
                {
                    troll_enemy_hp.Add(againt.Id, againt.CurrentHealth); 
                    troll_enemy_loc.Add(againt.Id, againt.GetLocation());
                    troll_enemy_act.Add(againt.Id, false);
                }
                
                /// Enemy Tornado Info
                Dictionary<int, Location> tornado_enemy_loc = new Dictionary<int, Location>();
                Dictionary<int, int> tornado_enemy_hp = new Dictionary<int, int>();
                foreach(Tornado againt in game.GetEnemyTornadoes())
                {
                    tornado_enemy_hp.Add(againt.Id, againt.CurrentHealth); 
                    tornado_enemy_loc.Add(againt.Id, againt.GetLocation());
                }
                
                counter = 1;

                /// Enemy Buildings Info
                Dictionary<int, Location> buildings_enemy_loc = new Dictionary<int, Location>();
                Dictionary<int, int> buildings_enemy_hp = new Dictionary<int, int>();
                foreach(Building againt in game.GetEnemyManaFountains())
                {
                    buildings_enemy_hp.Add(counter, againt.CurrentHealth); 
                    buildings_enemy_loc.Add(counter, againt.GetLocation());
                    counter++;
                }
                counter = -1;
                foreach(Building againt in game.GetEnemyPortals())
                {
                    buildings_enemy_hp.Add(counter, againt.CurrentHealth); 
                    buildings_enemy_loc.Add(counter, againt.GetLocation());
                    counter--;
                }
                
                // No BackUp Check
                for(int j = 0; j < Attp.Count; j++)
                {
                    ///// HitCheck
                    HitCheck(troll_loc, troll_hp, troll_enemy_loc, troll_enemy_hp, lava_enemy_loc, lava_enemy_hp, lava_loc, lava_hp, troll_act, troll_enemy_act, tornado_enemy_loc, tornado_enemy_hp, tornado_loc, tornado_hp);
                    
                    Dictionary<int,int> troll_enemy_hp_old = new Dictionary<int, int>(troll_enemy_hp);
                    Dictionary<int,Location> troll_enemy_loc_old = new Dictionary<int, Location>(troll_enemy_loc);
                    Dictionary<int,Location> lava_enemy_loc_old = new Dictionary<int, Location>(lava_enemy_loc);
                    Dictionary<int,int> lava_enemy_hp_old = new Dictionary<int, int>(lava_enemy_hp);
                    Dictionary<int,Location> tornado_enemy_loc_old = new Dictionary<int, Location>(tornado_enemy_loc);
                    Dictionary<int,int> tornado_enemy_hp_old = new Dictionary<int, int>(tornado_enemy_hp);
                    
                    ///// MoveEnemy
                    MoveCheck(troll_enemy_loc, troll_enemy_hp, troll_loc, troll_hp, lava_loc, lava_hp, lava_enemy_loc, lava_enemy_hp, troll_enemy_act, game.GetMyCastle(), tornado_enemy_loc, tornado_enemy_hp, buildings_loc, buildings_hp, tornado_loc, tornado_hp);
                    
                    ///// MoveMy
                    MoveCheck(troll_loc, troll_hp, troll_enemy_loc_old, troll_enemy_hp_old, lava_enemy_loc_old, lava_enemy_hp_old, lava_loc, lava_hp, troll_act, game.GetEnemyCastle(), tornado_loc, tornado_hp, buildings_enemy_loc, buildings_enemy_hp, tornado_enemy_loc_old, tornado_enemy_hp_old);
                    
                    ResetTurn(troll_enemy_act); // Prep For Next Turn
                    ResetTurn(troll_act); // Prep For Next Turn
                    
                }
                
                bool nobackup = false;
                
                foreach(var item in lava_enemy_loc)
                {
                    if(item.Value.InRange(game.GetMyCastle(), game.LavaGiantAttackRange + game.GetMyCastle().Size) && lava_enemy_hp[item.Key] > 3)
                    { nobackup = true; }
                }
                
                if(nobackup)
                {
                    lava_enemy_hp.Clear();
                    lava_enemy_loc.Clear();
                    foreach(LavaGiant againt in game.GetEnemyLavaGiants())
                    {
                        lava_enemy_hp.Add(againt.Id, againt.CurrentHealth); 
                        lava_enemy_loc.Add(againt.Id, againt.GetLocation());
                    }
                    
                    troll_hp.Clear();
                    troll_loc.Clear();
                    foreach(IceTroll againt in game.GetMyIceTrolls())
                    { 
                        troll_hp.Add(againt.Id, againt.CurrentHealth); 
                        troll_loc.Add(againt.Id, againt.GetLocation());
                    }
                    
                    troll_enemy_hp.Clear();
                    troll_enemy_loc.Clear();
                    foreach(IceTroll againt in game.GetEnemyIceTrolls())
                    {
                        troll_enemy_hp.Add(againt.Id, againt.CurrentHealth); 
                        troll_enemy_loc.Add(againt.Id, againt.GetLocation());
                    }
                    
                    lava_hp.Clear();
                    lava_loc.Clear();
                    foreach(LavaGiant againt in game.GetMyLavaGiants())
                    {
                        lava_hp.Add(againt.Id, againt.CurrentHealth); 
                        lava_loc.Add(againt.Id, againt.GetLocation());
                    }

                    tornado_enemy_hp.Clear();
                    tornado_enemy_loc.Clear();
                    foreach(Tornado againt in game.GetEnemyTornadoes())
                    {
                        tornado_enemy_hp.Add(againt.Id, againt.CurrentHealth); 
                        tornado_enemy_loc.Add(againt.Id, againt.GetLocation());
                    }

                    tornado_hp.Clear();
                    tornado_loc.Clear();
                    foreach(Tornado againt in game.GetMyTornadoes())
                    {
                        tornado_hp.Add(againt.Id, againt.CurrentHealth); 
                        tornado_loc.Add(againt.Id, againt.GetLocation());
                    }

                    buildings_enemy_hp.Clear();
                    buildings_enemy_loc.Clear();
                    counter = 1;
                    foreach(Building againt in game.GetEnemyManaFountains())
                    {
                        buildings_enemy_hp.Add(counter, againt.CurrentHealth); 
                        buildings_enemy_loc.Add(counter, againt.GetLocation());
                        counter++;
                    }
                    counter = -1;
                    foreach(Building againt in game.GetEnemyPortals())
                    {
                        buildings_enemy_hp.Add(counter, againt.CurrentHealth); 
                        buildings_enemy_loc.Add(counter, againt.GetLocation());
                        counter--;
                    }

                    buildings_hp.Clear();
                    buildings_loc.Clear();
                    counter = 1;
                    foreach(Building againt in game.GetMyManaFountains())
                    {
                        buildings_hp.Add(counter, againt.CurrentHealth); 
                        buildings_loc.Add(counter, againt.GetLocation());
                        counter++;
                    }
                    counter = -1;
                    foreach(Building againt in game.GetMyPortals())
                    {
                        buildings_hp.Add(counter, againt.CurrentHealth); 
                        buildings_loc.Add(counter, againt.GetLocation());
                        counter--;
                    }

                    
                    
                    ResetTurn(troll_enemy_act);
                    ResetTurn(troll_act);
                    
                    
                    for(int j = 0; j < Attp.Count; j++)
                    {
                        
                        if(j == game.IceTrollSummoningDuration - 1)
                        {
                            troll_hp.Add(555, game.IceTrollMaxHealth);
                            troll_loc.Add(555, DefPortal.GetLocation());
                            troll_act.Add(555, false);
                        }
                        
                        ///// HitCheck
                        HitCheck(troll_loc, troll_hp, troll_enemy_loc, troll_enemy_hp, lava_enemy_loc, lava_enemy_hp, lava_loc, lava_hp, troll_act, troll_enemy_act, tornado_enemy_loc, tornado_enemy_hp, tornado_loc, tornado_hp);
                        
                        Dictionary<int,int> troll_enemy_hp_old = new Dictionary<int, int>(troll_enemy_hp);
                        Dictionary<int,Location> troll_enemy_loc_old = new Dictionary<int, Location>(troll_enemy_loc);
                        Dictionary<int,Location> lava_enemy_loc_old = new Dictionary<int, Location>(lava_enemy_loc);
                        Dictionary<int,int> lava_enemy_hp_old = new Dictionary<int, int>(lava_enemy_hp);
                        Dictionary<int,Location> tornado_enemy_loc_old = new Dictionary<int, Location>(tornado_enemy_loc);
                        Dictionary<int,int> tornado_enemy_hp_old = new Dictionary<int, int>(tornado_enemy_hp);
                        
                        ///// MoveEnemy
                        MoveCheck(troll_enemy_loc, troll_enemy_hp, troll_loc, troll_hp, lava_loc, lava_hp, lava_enemy_loc, lava_enemy_hp, troll_enemy_act, game.GetMyCastle(), tornado_enemy_loc, tornado_enemy_hp, buildings_loc, buildings_hp, tornado_loc, tornado_hp);
                        
                        ///// MoveMy
                        MoveCheck(troll_loc, troll_hp, troll_enemy_loc_old, troll_enemy_hp_old, lava_enemy_loc_old, lava_enemy_hp_old, lava_loc, lava_hp, troll_act, game.GetEnemyCastle(), tornado_loc, tornado_hp, buildings_enemy_loc, buildings_enemy_hp, tornado_enemy_loc_old, tornado_enemy_hp_old);
                        
                        ResetTurn(troll_enemy_act); // Prep For Next Turn
                        ResetTurn(troll_act); // Prep For Next Turn
                    }
                    
                    foreach(var item in lava_enemy_loc)
                    {
                        if(item.Value.InRange(game.GetMyCastle(), game.LavaGiantAttackRange + game.GetMyCastle().Size) && (lava_enemy_hp[item.Key] < 3 || lava_enemy_hp[item.Key] > 5))
                        { return true; }
                    }
                    
                }
                
                
                /////////////////////////////////////////////////////////////////////////
                
                IceTroll defend = Closest_IceTroll_to_Lava(game.GetMyIceTrolls(), giant, game); 
                Location icetr = defend.GetLocation();
                int i = 0;
                int hpice = defend.CurrentHealth;
                
                while(!ice.InRange(icetr,100))
                {
                    if(i <= 2)
                    { }
                    else
                    {ice = ice.Towards(icetr, 100); }
                    i++;
                    hpice = hpice - defend.SuffocationPerTurn;
                    if(hpice < 0 && game.GetMyIceTrolls().Length < 4)
                    { return true; }
                }
                return false;
            }
        }
        
        
        //////////////////// Help Func ///////////////////////////////
        
        public void ResetTurn(Dictionary<int, bool> act)
        {
            foreach(var k in act.Keys.ToList())
            { act[k] = false; }
        }
        
        public int Attack_CheckLava()
        {
            foreach(Portal p in Myp)
            {
                int i = 0;
                Location l = p.GetLocation();
                while(!l.InRange(game.GetEnemyCastle(), game.GetEnemyCastle().Size + game.LavaGiantAttackRange))
                {
                    i += game.LavaGiantSuffocationPerTurn; 
                    l = l.Towards(game.GetEnemyCastle(), game.LavaGiantMaxSpeed);
                }
                if(game.LavaGiantMaxHealth - i >= game.LavaGiantMaxHealth / 2)
                { return game.LavaGiantMaxHealth / 2; }
                else if(game.LavaGiantMaxHealth - i >= game.LavaGiantMaxHealth / 3)
                { return game.LavaGiantMaxHealth / 3; }
            }
            return 0;
        }
        
        
        public int Attack_Check()
        {
            Location portal = AttPortal.GetLocation();
            int i = game.TornadoMaxHealth;
            List<Location> closest = new List<Location>();
            foreach(var b in game.GetEnemyPortals())
            { closest.Add(b.GetLocation()); }
            foreach(var b in game.GetEnemyManaFountains())
            { closest.Add(b.GetLocation()); }
            closest = (from element in closest orderby element.Distance(portal) select element).ToList();
            if(closest.Count ==  0)
            { return game.TornadoMaxHealth / 2; }
            
            while(!portal.InRange(closest[0], game.TornadoAttackRange))
            {
                i -= game.TornadoSuffocationPerTurn;
                portal = portal.Towards(closest[0], game.TornadoMaxSpeed);
            }
            if(i >= game.TornadoMaxHealth / 2)
            { return game.TornadoMaxHealth / 2; }
            else if(i >= game.TornadoMaxHealth / 3)
            { return game.TornadoMaxHealth / 3; }
            else
            { return 0; }
        }
        
        
        public IceTroll Closest_IceTroll_to_Lava(IceTroll[] f, LavaGiant w, Game game)
        {
            IceTroll closest = null;
            if(f.Length != 0)
            { 
                closest = f[0];
                foreach(var p in f)
                {
                    if(w.Distance(p) < closest.Distance(p))
                    { closest = p; }
                }
            }
            return closest;
        }
        
        public Elf Closest_Elf_to_Portal(Elf[] f, Portal w, Game game)
        {
            Elf closest = null;
            if(f.Length != 0)
            { 
                closest = f[0];
                foreach(var p in f)
                {
                    if(w.Distance(p) < closest.Distance(p))
                    { closest = p; }
                }
            }
            return closest;
        }
        
        public bool protect_portal()
        {
            bool b=false;
            foreach(Portal closest in Myp)
            {
                foreach(Portal p in game.GetEnemyPortals())
                {
                    int counter2=0;
                    foreach(Tornado i in game.GetMyTornadoes())
                    {
                        if(i.Distance(closest)<p.Distance(closest))
                        {
                            counter2++;
                        }
                    }
                    if(closest.InRange(p, (p.Size+closest.Size)*2) && counter2 < 1)
                    {   
                        b=true;
                        if(closest.CanSummonTornado())
                        { 
                            closest.SummonTornado();
                        }
                    }
                }
                
                foreach(ManaFountain p in game.GetEnemyManaFountains())
                {
                    int counter3=0;
                    foreach(Tornado i in game.GetMyTornadoes())
                    {
                        if(i.Distance(closest)<p.Distance(closest))
                        {
                            counter3++;
                        }
                    }
                    if(closest.InRange(p, (p.Size+closest.Size)*2) && counter3 < 1)
                    {   
                        b=true;
                        if(closest.CanSummonTornado())
                        { 
                            closest.SummonTornado();
                        }
                    }
                }
            }
            return b;
        }

        public void protect_the_portal()
        {
            Elf enemy = null;
            foreach(Portal closest in Myp)
            {
                foreach(Portal p in game.GetEnemyPortals())
                {
                    int counter2=0;
                    foreach(Tornado i in game.GetMyTornadoes())
                    {
                        if(i.Distance(closest)<p.Distance(closest))
                        {
                            counter2++;
                        }
                    }
                    if(closest.InRange(p, (p.Size+closest.Size)*2) && closest.CanSummonTornado() && counter2 < 1)
                    { closest.SummonTornado(); }
                }
                
                foreach(ManaFountain p in game.GetEnemyManaFountains())
                {
                    int counter3=0;
                    foreach(Tornado i in game.GetMyTornadoes())
                    {
                        if(i.Distance(closest)<p.Distance(closest))
                        {
                            counter3++;
                        }
                    }
                    if(closest.InRange(p, (p.Size+closest.Size)*2) && closest.CanSummonTornado() && counter3 < 1)
                    { closest.SummonTornado(); }
                }
                
                foreach(Tornado att in game.GetEnemyTornadoes())
                {
                    int counter=0;
                    foreach(IceTroll i in game.GetMyIceTrolls())
                    {
                        if(i.Distance(closest)<att.Distance(closest))
                        {
                            counter++;
                        }
                    }
                    if(closest.InRange(att, 1200) && closest.CanSummonIceTroll() && counter < 3)
                    { closest.SummonIceTroll(); }    
                }
                
                foreach(Elf att in game.GetEnemyLivingElves())
                {
                    int counter=0;
                    foreach(IceTroll i in game.GetMyIceTrolls())
                    {
                        if(i.Distance(closest)<att.Distance(closest))
                        {
                            counter++;
                        }
                    }
                    if(closest.InRange(att, 1000) && closest.CanSummonIceTroll() && counter < 2)
                    { closest.SummonIceTroll(); }    
                }
            }
            
        }
        
        public int KeyByValue(Dictionary<int, Location> dict, Location val)
        {
            int key = 555;
            foreach (KeyValuePair<int, Location> pair in dict)
            {
                if (pair.Value == val)
                { 
                    key = pair.Key; 
                    break; 
                }
            }
            return key;
        }
        
        
        public List<Location> Closest_MapObject_to_location(Location t, Dictionary<int, Location> troll, Dictionary<int, Location> lava, Dictionary<int, Location> tornado)
        {
            List<Location> enemyloc = new List<Location>();
            enemyloc.AddRange(lava.Values.ToList());
            enemyloc.AddRange(troll.Values.ToList());
            enemyloc.AddRange(tornado.Values.ToList());
            var enemyloc_sorted = from element in enemyloc orderby element.Distance(t) select element;
            return enemyloc_sorted.ToList();
        }
        
        public bool ContainsValue(Location l, Dictionary<int, Location> dic)
        {
            
            List<Location> loc = dic.Values.ToList();
            if(loc.Count == 0)
            { return false; }
            foreach(Location ll in loc)
            {
                if(l.Equals(ll))
                { return true; }
            }
            return false;
        }
        

        
         
        
        public void HitCheck(Dictionary<int, Location> My_IceTrolls_loc, Dictionary<int, int> My_IceTrolls_hp, Dictionary<int, Location> Enemy_IceTrolls_loc, Dictionary<int, int> Enemy_IceTrolls_hp,
        Dictionary<int,Location> Enemy_Lava_loc, Dictionary<int, int> Enemy_Lava_hp, Dictionary<int, Location> My_Lava_loc, Dictionary<int, int> My_Lava_hp, Dictionary<int, bool> My_Ice_acted, Dictionary<int, bool> Enemy_Ice_acted,
        Dictionary<int,Location> Enemy_Tornado_loc, Dictionary<int, int> Enemy_Tornado_hp, Dictionary<int,Location> My_Tornado_loc, Dictionary<int, int> My_Tornado_hp)
        {
            List<int> troll_keys = new List<int>(My_IceTrolls_loc.Keys);
            List<int> troll_enemy_keys = new List<int>(Enemy_IceTrolls_loc.Keys);
            
            //////// My IceTroll
            foreach(var def in troll_keys)
            {
                var closest = Closest_MapObject_to_location(My_IceTrolls_loc[def], Enemy_IceTrolls_loc, Enemy_Lava_loc, Enemy_Tornado_loc);
                for(int i = 0; closest.Count > i; i++)
                {
                    // IceTroll Hit
                    if(ContainsValue(closest[i], Enemy_IceTrolls_loc) && My_IceTrolls_loc[def].InRange(closest[i], game.IceTrollAttackRange))
                    {
                        int key = KeyByValue(Enemy_IceTrolls_loc, closest[i]);
                        if(Enemy_IceTrolls_hp[key] != 0 && My_IceTrolls_hp[def] != 0)
                        {
                            Enemy_IceTrolls_hp[key] -= game.IceTrollAttackMultiplier; // deal attack
                            My_IceTrolls_hp[def] -= game.IceTrollSuffocationPerTurn; // end turn for this icetroll
                            My_Ice_acted[def] = true; // end turn for this icetroll
                            break;
                        }
                    }
                    // LavaGiant hit 
                    else if(ContainsValue(closest[i], Enemy_Lava_loc) && My_IceTrolls_loc[def].InRange(closest[i], game.IceTrollAttackRange))
                    {
                        int key = KeyByValue(Enemy_Lava_loc, closest[i]);
                        if(Enemy_Lava_hp[key] != 0 && My_IceTrolls_hp[def] != 0)
                        {
                            Enemy_Lava_hp[key] -= game.IceTrollAttackMultiplier; // deal attack
                            My_IceTrolls_hp[def] -= game.IceTrollSuffocationPerTurn; // end turn for this icetroll
                            My_Ice_acted[def] = true; // end turn for this icetroll
                            break;
                        }
                    }
                    // Tornado Hit
                    else if(ContainsValue(closest[i], Enemy_Tornado_loc) && My_IceTrolls_loc[def].InRange(closest[i], game.IceTrollAttackRange))
                    {
                        int key = KeyByValue(Enemy_Tornado_loc, closest[i]);
                        if(Enemy_Tornado_hp[key] != 0 && My_IceTrolls_hp[def] != 0)
                        {
                            Enemy_Tornado_hp[key] -= game.IceTrollAttackMultiplier; // deal attack
                            My_IceTrolls_hp[def] -= game.IceTrollSuffocationPerTurn; // end turn for this icetroll
                            My_Ice_acted[def] = true; // end turn for this icetroll
                            break;
                        }
                    }
                }
            }

            
            ////// Enemy icetroll
            foreach(var def in troll_enemy_keys)
            {
                var closest = Closest_MapObject_to_location(Enemy_IceTrolls_loc[def], My_IceTrolls_loc, My_Lava_loc, My_Tornado_loc);
                for(int i = 0; closest.Count > i; i++)
                {
                    // IceTroll Hit
                    if(ContainsValue(closest[i], My_IceTrolls_loc) && Enemy_IceTrolls_loc[def].InRange(closest[i], game.IceTrollAttackRange))
                    {
                        int key = KeyByValue(My_IceTrolls_loc, closest[i]);
                        if(My_IceTrolls_hp[key] != 0 && Enemy_IceTrolls_hp[def] != 0)
                        {
                            My_IceTrolls_hp[key] -= game.IceTrollAttackMultiplier; // deal attack
                            Enemy_IceTrolls_hp[def] -= game.IceTrollSuffocationPerTurn; // end turn for this icetroll
                            Enemy_Ice_acted[def] = true; // end turn for this icetroll
                            break;
                        }
                    }
                    // LavaGiant Hit
                    else if(ContainsValue(closest[i], My_Lava_loc) && Enemy_IceTrolls_loc[def].InRange(closest[i], game.IceTrollAttackRange))
                    {
                        int key = KeyByValue(My_Lava_loc, closest[i]);
                        if(My_Lava_hp[key] != 0 && Enemy_IceTrolls_hp[def] != 0)
                        {
                            My_Lava_hp[key] -= game.IceTrollAttackMultiplier; // deal attack
                            Enemy_IceTrolls_hp[def] -= game.IceTrollSuffocationPerTurn; // end turn for this icetroll
                            Enemy_Ice_acted[def] = true; // end turn for this icetroll
                            break;
                        }
                    }
                    // Tornado Hit
                    else if(ContainsValue(closest[i], My_Tornado_loc) && Enemy_IceTrolls_loc[def].InRange(closest[i], game.IceTrollAttackRange))
                    {
                        int key = KeyByValue(My_Tornado_loc, closest[i]);
                        if(My_Tornado_hp[key] != 0 && Enemy_IceTrolls_hp[def] != 0)
                        {
                            My_Tornado_hp[key] -= game.IceTrollAttackMultiplier; // deal attack
                            Enemy_IceTrolls_hp[def] -= game.IceTrollSuffocationPerTurn; // end turn for this icetroll
                            Enemy_Ice_acted[def] = true; // end turn for this icetroll
                            break;
                        }
                    }
                }
            }
        }
        
        public void MoveCheck(Dictionary<int, Location> My_IceTrolls_loc, Dictionary<int, int> My_IceTrolls_hp, Dictionary<int, Location> Enemy_IceTrolls_loc, Dictionary<int, int> Enemy_IceTrolls_hp,
        Dictionary<int,Location> Enemy_Lava_loc, Dictionary<int, int> Enemy_Lava_hp, Dictionary<int, Location> My_Lava_loc, Dictionary<int, int> My_Lava_hp, Dictionary<int, bool> My_Ice_acted, Castle c,
        Dictionary<int,Location> My_Tornado_loc, Dictionary<int, int> My_Tornado_hp, Dictionary<int,Location> Enemy_Building_loc, Dictionary<int, int> Enemy_Building_hp, Dictionary<int,Location> Enemy_Tornado_loc, Dictionary<int, int> Enemy_Tornado_hp)
        {
            List<int> troll_keys = new List<int>(My_IceTrolls_loc.Keys);
            List<int> lava_keys = new List<int>(My_Lava_loc.Keys);
            List<int> tornado_keys = new List<int>(My_Tornado_loc.Keys);
            
            //////////////// Move Check
            foreach(var def in troll_keys)
            {
                var closest = Closest_MapObject_to_location(My_IceTrolls_loc[def], Enemy_IceTrolls_loc, Enemy_Lava_loc, Enemy_Tornado_loc);
                for(int i = 0; closest.Count > i ; i++)
                {
                    // LavaGiant Move
                    if(ContainsValue(closest[i], Enemy_Lava_loc) && !My_Ice_acted[def])
                    {
                        int key = KeyByValue(Enemy_Lava_loc, closest[i]);
                        if(Enemy_Lava_hp[key] != 0 && My_IceTrolls_hp[def] != 0)
                        {
                            My_IceTrolls_loc[def] = My_IceTrolls_loc[def].Towards(closest[i], game.IceTrollMaxSpeed); // Move To Object
                            My_IceTrolls_hp[def] -= game.IceTrollSuffocationPerTurn; // End Trun
                            break; // End Trun
                        }
                    }
                    
                    // IceTroll Move
                    else if(ContainsValue(closest[i], Enemy_IceTrolls_loc) && !My_Ice_acted[def])
                    {
                        int key = KeyByValue(Enemy_IceTrolls_loc, closest[i]);
                        if(Enemy_IceTrolls_hp[key] != 0 && My_IceTrolls_hp[def] != 0)
                        {
                            My_IceTrolls_loc[def] = My_IceTrolls_loc[def].Towards(closest[i], game.IceTrollMaxSpeed); // Move To Object
                            My_IceTrolls_hp[def] -= game.IceTrollSuffocationPerTurn; // End Turn
                            break; // End Trun
                        }
                    }

                    // Tornado Move
                    else if(ContainsValue(closest[i], Enemy_Tornado_loc) && !My_Ice_acted[def])
                    {
                        int key = KeyByValue(Enemy_Tornado_loc, closest[i]);
                        if(Enemy_Tornado_hp[key] != 0 && My_IceTrolls_hp[def] != 0)
                        {
                            My_IceTrolls_loc[def] = My_IceTrolls_loc[def].Towards(closest[i], game.IceTrollMaxSpeed); // Move To Object
                            My_IceTrolls_hp[def] -= game.IceTrollSuffocationPerTurn; // End Turn
                            break; // End Trun
                        }
                    }
                }
            }
            
            foreach(var att in lava_keys)
            {
                if(My_Lava_hp[att] != 0 && !My_Lava_loc[att].InRange(c, c.Size + game.LavaGiantAttackRange))
                {
                    My_Lava_loc[att] = My_Lava_loc[att].Towards(c.GetLocation(), game.LavaGiantMaxSpeed); // Move To Castle
                    My_Lava_hp[att] -= game.LavaGiantSuffocationPerTurn; // End Turn
                }
            }

            foreach(var att in tornado_keys)
            {
                List<Location> closest = (from element in Enemy_Building_loc.Values orderby element.Distance(My_Tornado_loc[att]) select element).ToList();
                for(int i = 0; closest.Count > i ; i++)
                {
                    int key = KeyByValue(Enemy_Building_loc, closest[i]);
                    if(Enemy_Building_hp[key] != 0)
                    {
                        /// Mana
                        if(key > 0)
                        {
                            /// Move To Building
                            if(My_Tornado_hp[att] != 0 && !My_Tornado_loc[att].InRange(Enemy_Building_loc[key], game.ManaFountainSize + game.TornadoAttackRange))
                            {
                                My_Tornado_loc[att] = My_Tornado_loc[att].Towards(closest[i], game.TornadoMaxSpeed); // Move
                                My_Tornado_hp[att] -= game.TornadoSuffocationPerTurn; // End Turn
                                break;
                            }
                             /// Attack Building
                            else if(My_Tornado_hp[att] != 0 && My_Tornado_loc[att].InRange(Enemy_Building_loc[key], game.ManaFountainSize + game.TornadoAttackRange))
                            {
                                Enemy_Building_hp[key] -= game.TornadoAttackMultiplier; // Deal Damge
                                My_Tornado_hp[att] -= game.TornadoSuffocationPerTurn; // End Turn
                                break;
                            }
                        }
                        /// Portal
                        else
                        {
                            /// Move To Building
                            if(My_Tornado_hp[att] != 0 && !My_Tornado_loc[att].InRange(Enemy_Building_loc[key], game.PortalSize + game.TornadoAttackRange))
                            {
                                My_Tornado_loc[att] = My_Tornado_loc[att].Towards(closest[i], game.TornadoMaxSpeed); // Move
                                My_Tornado_hp[att] -= game.TornadoSuffocationPerTurn; // End Turn
                                break;
                            }
                            /// Attack Building
                            else if(My_Tornado_hp[att] != 0 && My_Tornado_loc[att].InRange(Enemy_Building_loc[key], game.PortalSize + game.TornadoAttackRange))
                            {
                                Enemy_Building_hp[key] -= game.TornadoAttackMultiplier; // Deal Damge
                                My_Tornado_hp[att] -= game.TornadoSuffocationPerTurn; // End Turn
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}