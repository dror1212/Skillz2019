using ElfKingdom;
using System.Collections.Generic; 
using System.Linq;

namespace MyBot
{
    public class Elfiot
    {
        private static Elfiot instance;
        public Game game;
        public Dictionary<int, Location> target;
        public Server server;
        public Location portal_base;
        public Dictionary<int,bool> start_building;
        public int dmg;
        
        /// Enemy Troll Info
        Dictionary<int, Location> enemy_loc; 
        
        /// Enemy Elf Info
        Dictionary<int, Location> enemy_loc_elf;
        
        Dictionary<int, List<Location>> Elf_enemy;

        
        private Elfiot(Game game)
        {
            this.dmg = 0;
            this.game = game;      
            this.target = new Dictionary<int, Location>();
            this.server = new Server();
            int counter = 0;
            this.start_building=new Dictionary<int,bool>();
            foreach(Elf elfit in game.GetAllMyElves())
            {                
                target.Add(counter,null);
                start_building.Add(counter,false);
                counter++;
            }
            
            this.enemy_loc = new Dictionary<int, Location>();
            this.enemy_loc_elf = new Dictionary<int, Location>();
            this.Elf_enemy = new Dictionary<int, List<Location>>();
            
            foreach(Elf elfit in game.GetEnemyLivingElves())
            { 
                List<Location> d = new List<Location>();
                d.Add(elfit.GetLocation());
                this.Elf_enemy.Add(elfit.Id, d); 
            }
            
        }
        
        public static Elfiot ElfiotInstance(Game game)
        {
            if(instance == null)
            { instance = new Elfiot(game); }
            return instance;
        }
        
        public bool check_go(Elf elfit, Location l)
        {
            if(enemy_loc.Count == 0)
            { return true; }
            
            if(l==null)
            { return true; }
            
            Location Next_Turn = elfit.GetLocation().Towards(l, game.ElfMaxSpeed);
            // IceTroll Check
            foreach(Location enemy in this.enemy_loc.Values)
            {
                if(Next_Turn.InRange(enemy, game.IceTrollAttackRange))
                { 
                    return false;
                }
            }
            return true;
        }
        
        public bool check_go_elf(Elf elfit, Location l)
        {
            if(enemy_loc.Count == 0)
            { return true; }
            
            if(enemy_loc_elf.Count == 0)
            { return true; }
            
            if(l==null)
            { return true; }
            
            Location Next_Turn = elfit.GetLocation().Towards(l, game.ElfMaxSpeed);
            // IceTroll Check
            foreach(Location enemy in enemy_loc.Values)
            {
                if(Next_Turn.InRange(enemy, game.IceTrollAttackRange))
                { 
                    return false;
                }
            }
            
            // Elf Check
            foreach(Location enemy in enemy_loc_elf.Values)
            {
                if(Next_Turn.InRange(enemy, game.ElfAttackRange))
                { 
                    return false; 
                }
            }
            return true;
        }
        
        
        public void main() //need to look for problems
        {
            try
            {
                enemy_loc.Clear();
                enemy_loc_elf.Clear();
            }
            catch{}
            
            if(game.GetMyManaFountains().Length>0||game.GetMyMana()>game.ManaFountainCost+game.PortalCost)
            {
                server.done();
                server.help();
            }
            /////////////////////////////////////need to check if works
            target = TargetUpdate.TargetInstance(game).Update();
            Enemy_invs(); 
            ////////////////////////////////////
            if(game.GetMyLivingElves().Length>0)
            {
                List<Elf> sortedElves = (from element in game.GetMyLivingElves() orderby element.Distance(game.GetMyCastle()) select element).ToList();
                
                DestroyEnemyManaFountains(sortedElves[sortedElves.Count-1]);
                
                foreach(Elf elfit2 in game.GetAllMyElves())
                {
                    if(!elfit2.IsAlive())
                    {
                        sortedElves.Add(elfit2);
                    }
                }
                foreach(Elf elfit in sortedElves)
                {
                    try
                    {
                        if(elfit.IsAlive())
                        {
                            if(!elfit.IsBuilding)
                            {
                                Can_Go(elfit, target[elfit.Id]);
                                Can_Go_Elf(elfit, target[elfit.Id]);
                                if(!elfit.AlreadyActed)
                                {
                                    HelpDefenceAgainstPortal(elfit);
                                }
                                if(!elfit.AlreadyActed&NeedToRun(elfit))
                                {
                                    Run(elfit,"onlyTrolls");
                                }
                                if(!elfit.AlreadyActed)
                                {
                                    HelpDefenceAgainstElf(elfit);
                                }
                                if(!elfit.AlreadyActed&&!elfit.GetLocation().Equals(new Location(-50,-50)))
                                {
                                    AttackElf(elfit,true);
                                }
                                if(!elfit.AlreadyActed)
                                {
                                    ProtectMyManaFountains(elfit);
                                }
                                if(!elfit.AlreadyActed)
                                {
                                    MeetUp(elfit);
                                }
                                if(!elfit.AlreadyActed)
                                {
                                    AttackManaFountain(elfit);
                                }
                                if(!elfit.AlreadyActed)
                                {
                                    AttackPortal(elfit);
                                }
                                if(!elfit.AlreadyActed&&game.GetMyPortals().Length>=game.GetMyManaFountains().Length)
                                {
                                    Build_Mana_Fountain(elfit);
                                }
                                if(!elfit.AlreadyActed&&(game.Turn>game.MaxTurns/7||game.GetMyManaFountains().Length>0||game.GetMyPortals().Length==0))
                                {
                                    Build_Portal(elfit);
                                }
                                if(!elfit.AlreadyActed&&!elfit.GetLocation().Equals(new Location(-50,-50)))
                                {
                                    GoToNearEnemyElf(elfit);
                                }
                                if(!elfit.AlreadyActed)
                                {
                                    ChooseClosestManaOrPortal(elfit);
                                }
                                if(!elfit.AlreadyActed)
                                {
                                    AttackCastle(elfit);
                                }
                                if(!elfit.AlreadyActed)
                                {
                                    Build_Portal(elfit);
                                }
                                if(!elfit.AlreadyActed)
                                {
                                    GoToEnemyCastle(elfit);
                                }
                            }
                            else
                                start_building[elfit.Id]=false;
                        }
                        else
                        {
                            target[elfit.Id]=null;
                        }
                    }
                    catch{}
                }
            }
            
            foreach(Elf elfit in game.GetEnemyLivingElves())
            { 
                Elf_enemy[elfit.Id].Add(elfit.GetLocation()); 
            }
        }
        
        public void Enemy_invs()
        {
            foreach(Elf elfit in game.GetEnemyLivingElves())
            {
                if(elfit.GetLocation().Equals(new Location(-50, -50)))
                {
                    List<Location> locs = new List<Location>(Elf_enemy[elfit.Id]);
                    locs.Reverse();
                    
                    int time_left;
                    for( time_left = 0; locs[time_left].Equals(new Location(-50, -50)); time_left++)
                    { }
                    Location l = locs[time_left];

                    time_left++;
                    
                    
                    
                    time_left = game.InvisibilityExpirationTurns - time_left;
                    
                    List<Location> closest = (from element in game.GetMyManaFountains() orderby element.Distance(elfit) select element.GetLocation()).ToList();
                    
                    if(closest.Count != 0 && closest[0].InRange(l, game.InvisibilityExpirationTurns * elfit.MaxSpeed + elfit.AttackRange + game.ManaFountainSize))
                    {
                        List<Location> route = new List<Location>();
                        while(!l.InRange(closest[0], elfit.AttackRange + game.ManaFountainSize) && time_left != 0)
                        {
                            route.Add(l);
                            l = l.Towards(closest[0], elfit.MaxSpeed);
                            time_left--;
                        }
                        
                        //// l location go Towards
                        
                        List<Elf> efls = (from element in game.GetMyLivingElves() orderby element.Distance(l) select element).ToList();
                        if(efls.Count != 0 && closest[0].InRange(efls[0], efls[0].MaxSpeed * 6 + game.ManaFountainSize + efls[0].AttackRange))
                        { efls[0].MoveTo(l); }
                        
                    }
                }
            }
        }
        
        public bool DestroyEnemyManaFountains(Elf elfit)
        {
            if(elfit.AlreadyActed||elfit.IsBuilding)
                return false;
            foreach(ManaFountain em in game.GetEnemyManaFountains())
            {
                if(elfit.InRange(em,game.ElfMaxSpeed * game.SpeedUpMultiplier * game.SpeedUpExpirationTurns + game.ElfMaxSpeed * 6 + em.Size + elfit.AttackRange))
                {
                    if(elfit.MaxSpeed==game.ElfMaxSpeed)
                    {
                        if(!elfit.InRange(em,game.ElfMaxSpeed * 2 + em.Size + elfit.AttackRange) && elfit.CanCastSpeedUp())
                        {
                            elfit.CastSpeedUp();
                            return true;
                        }
                        else
                        {
                            if(!Run(elfit,"ElvesToo"))
                            {
                                if(!AttackManaFountain(elfit))
                                {
                                    elfit.MoveTo(em);
                                }
                                return true;
                            }
                            else
                                return true;
                        }
                    }
                    else
                    {
                        if(!Run(elfit,"ElvesToo"))
                        {
                            if(!AttackManaFountain(elfit))
                            {
                                elfit.MoveTo(em);
                            }
                            return true;
                        }
                        else
                            return true;
                    }
                }
            }
            return false;
        }
        private Location closestLocationToLocation(Location l, List<Location> locs)
        {
            var p = from element in locs orderby element.Distance(l) select element;
            return p.ToList()[0];
        }
        
        private bool HelpAgainsLavaGiants(Elf elfit)
        {
            List<LavaGiant> sortedByLife = (from element in game.GetEnemyLavaGiants() orderby element.CurrentHealth select element).ToList();
            foreach(LavaGiant l in sortedByLife)
            {
                if(elfit.InAttackRange(l))
                {
                    if(elfit.Distance(game.GetMyCastle())<game.CastleSize+elfit.AttackRange)
                    {
                        return AttackLavaGiant(elfit);
                    }
                }
            }
            return false;
        }
        
        private bool AttackLavaGiant(Elf elfit)
        {
            List<LavaGiant> sortedByLife = (from element in game.GetEnemyLavaGiants() orderby element.CurrentHealth select element).ToList();
            foreach(LavaGiant l in sortedByLife)
            {
                if(elfit.InAttackRange(l))
                {
                    elfit.Attack(l);
                    return true;
                }
            }
            return false;
        }
        private void MeetUp(Elf elfit)
        {
            if(game.GetMyLivingElves().Length>1)
            {
                if(con(target[elfit.Id],elfit.Id))
                {
                    bool b = false;
                    foreach(Elf ene in game.GetEnemyLivingElves())
                    {
                        if(ene.GetLocation().Equals(target[elfit.Id]))
                            b=true;
                    }
                    if(b)
                    {
                        int counter = 0;
                        int meet = 999;
                        foreach(Location l in target.Values)
                        {
                            if(l.Equals(target[elfit.Id])&&counter!=elfit.Id)
                            {
                                if(target[elfit.Id].Distance(game.GetAllMyElves()[counter])<1500)
                                {
                                    meet = counter;
                                    break;
                                }
                            }
                            counter++;
                        }
                        if(meet!=999)
                        {
                            if(elfit.Distance(game.GetAllMyElves()[meet])>elfit.MaxSpeed*4)
                                elfit.MoveTo(game.GetAllMyElves()[meet]);
                        }
                    }
                }
            }
        }
        private bool AttackTornado(Elf elfit)
        {
            List<Tornado> sortedByLife = (from element in game.GetEnemyTornadoes() orderby element.CurrentHealth select element).ToList();
            foreach(Tornado t in sortedByLife)
            {
                List<ManaFountain> closest = (from element in game.GetMyManaFountains() orderby element.Distance(t) select element).ToList();
                List<Portal> closest2 = (from element in game.GetMyPortals() orderby element.Distance(t) select element).ToList();
                if(closest.Count>0||closest2.Count>0)
                {
                    GameObject x = null;
                    if(closest.Count>0&&closest2.Count==0)
                        x = closest[0];
                    else if(closest.Count==0&&closest2.Count>0)
                        x = closest2[0];
                    else if(closest.Count>0&&closest2.Count>0)
                    {
                        x = closest[0];
                        if(x.Distance(t)>closest2[0].Distance(t))
                            x = closest2[0];
                    }
                    if(t.CurrentHealth<5||(x!=null&&elfit.InAttackRange(t)&&t.Distance(x)<t.AttackRange+((Building)x).Size))
                    {
                        elfit.Attack(t);
                        return true;
                    }
                }
            }
            return false;
        }
        public bool ProtectMyManaFountains(Elf elfit)
        {
            foreach(Elf enemy in game.GetEnemyLivingElves())
            {
                foreach(ManaFountain mana in game.GetMyManaFountains())
                {
                    if(enemy.InRange(mana,enemy.AttackRange*2+mana.Size)&&!con(enemy.GetLocation(),elfit.Id))
                    {
                        elfit.MoveTo(enemy);
                        return true;
                    }
                }
            }
            return false;
        }
        public Location GetRunBorder(Elf elfit, List<IceTroll> trolls)
        {
            int[] counters = new int[4];
            if(target[elfit.Id]==null)
                target[elfit.Id]=game.GetEnemyCastle().GetLocation();
            foreach(IceTroll t in trolls)
            {
                Location l = t.GetLocation();
                if(System.Math.Abs(elfit.GetLocation().Row-l.Row)<System.Math.Abs(elfit.GetLocation().Col-l.Col))
                {
                    if(elfit.GetLocation().Col<l.Col)
                    {
                        counters[0]++;
                    }
                    else
                    {
                        counters[1]++;
                    }
                }
                else
                {
                    if(elfit.GetLocation().Row<l.Row)
                    {
                        counters[2]++;
                    }
                    else
                    {
                        counters[3]++;
                    }
                }
            }
            
            int maxValue = counters.Max();
            int maxIndex = counters.ToList().IndexOf(maxValue);
            
            if(maxIndex==0)
                return new Location(target[elfit.Id].Row-1,0);
            if(maxIndex==1)
                return new Location(target[elfit.Id].Row-1,game.Cols-1);
            if(maxIndex==2)
                return new Location(0,target[elfit.Id].Col-1);                    
            return new Location(game.Rows-1,target[elfit.Id].Col-1);
        }

        public bool NeedToRun(Elf elfit)
        {
            GameObject p = this.Closest_Object_to_Object(game.GetEnemyPortals(),elfit);
            if(p!=null)
            {
                if(elfit.InAttackRange(p))
                {
                    int x = p.CurrentHealth;
                    if(x <= game.PortalMaxHealth/2)
                    {
                        return false;
                    }
                }
            }
            p = this.Closest_Object_to_Object(game.GetEnemyManaFountains(),elfit);
            if(p!=null)
            {
                if(elfit.InAttackRange(p))
                {
                    int x = p.CurrentHealth;
                    if(x < game.ManaFountainMaxHealth)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        
        public bool check_for_portals(Location dot)
        {
            foreach(Portal p in game.GetMyPortals())
            {
                if(dot.Equals(p.GetLocation()))
                {
                    return false;
                }
            }
            return true;
        }
        
        public int getClosestBorderToLocation(Location l)
        {
            Location b = new Location(game.Rows-1,l.Col);
            int dis = l.Distance(b);
            int number=1;
            if(l.Distance(new Location(0,l.Col))<dis)
            {
                b = new Location(0,l.Col);
                dis = l.Distance(b);
                number=3;
            }
            if(l.Distance(new Location(l.Row,game.Cols-1))<dis)
            {
                b = new Location(l.Row,game.Cols-1);
                dis = l.Distance(b);
                number=0;
                
            }
            if(l.Distance(new Location(l.Row,game.Cols-1))<dis)
            {
                b = new Location(l.Row,game.Cols-1);
                dis = l.Distance(b);
                number=2;
            }
            return number*90;
        }
        
        public bool con(Location dot,int i)
        {
            foreach(int j in target.Keys.ToList())
            {
                Location l = target[j];
                if(j!=i&&dot.Equals(l))
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool BuildSpecificPortal(Elf elfit,Location dot)
        {
            if(!con(dot,elfit.Id))
            {
                if(check_for_portals(dot))
                {
                    if(CanBuildSafely(elfit))
                    {
                        target[elfit.Id]=dot;
                        if(elfit.GetLocation().Equals(dot)&&elfit.CanBuildPortal())
                        {
                            elfit.BuildPortal();
                            return true;
                        }
                        else
                        {
                            elfit.MoveTo(dot);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        public bool HelpDefenceAgainstPortal(Elf elfit)
        {
            MapObject p = Closest_Object_to_Object(game.GetEnemyPortals(),game.GetMyCastle());
            if(p!=null)
            {
                if(!Build_Portal(elfit))
                {
                    if(p.Distance(game.GetMyCastle())<game.GetEnemyCastle().Distance(game.GetMyCastle())/2)
                    {
                        if(!elfit.InAttackRange(p))
                        {
                            if(!AttackElf(elfit,true))
                            {
                                elfit.MoveTo(p);
                            }
                            return true;
                        }
                        else
                        {
                            return AttackPortal(elfit);
                        }
                    }
                }
            }
            return false;
        }
        
        public bool HelpDefenceAgainstElf(Elf elfit)
        {
            List<Elf> sortedEnemyElves = (from element in game.GetEnemyLivingElves() orderby element.Distance(game.GetMyCastle()) select element).ToList();
            foreach(Elf elfitEnemy in sortedEnemyElves)
            {
                if(elfitEnemy.GetLocation().InMap())
                {
                    if(!con(elfitEnemy.GetLocation(),elfit.Id))
                    {
                        if((elfit.Distance(game.GetMyCastle())>=elfitEnemy.Distance(game.GetMyCastle())||(elfitEnemy.Distance(game.GetMyCastle())<game.CastleSize*4)))
                        {
                            if(!elfit.InAttackRange(elfitEnemy))
                            {
                                if(elfit.InRange(elfitEnemy,game.ElfMaxSpeed*4))
                                {
                                    elfit.MoveTo(elfitEnemy);
                                    return true;
                                }
                                else
                                {
                                   if(elfit.CanCastSpeedUp()&&elfit.MaxSpeed==game.ElfMaxSpeed)
                                    {
                                        elfit.CastSpeedUp();
                                        return true;
                                    }
                                    else
                                    {
                                        elfit.MoveTo(elfitEnemy);
                                        return true;
                                    }
                                }
                            }
                            else
                            {  
                                return AttackElf(elfit,true);
                            } 
                        }
                    }
                }
            }
            return false;
        }
        
        private bool AttackPortal(Elf elfit)
        {
            foreach(Portal p in game.GetEnemyPortals())
            {
                if(elfit.InAttackRange(p))
                {
                    elfit.Attack(p);
                    return true;
                }
            }
            return false;
        }
        
        private bool AttackManaFountain(Elf elfit)
        {
            foreach(ManaFountain p in game.GetEnemyManaFountains())
            {
                if(elfit.InAttackRange(p))
                {
                    elfit.Attack(p);
                    return true;
                }
            }
            return false;
        }
        
        private bool ChooseClosestManaOrPortal(Elf elfit)
        {
            MapObject p = GetNearPortal(elfit);
            MapObject m = GetNearManaFountains(elfit);
            if(p==null&&m==null)
            {
                return false;
            }
            else if(p==null)
            {
                elfit.MoveTo(m);
                return true;
            }
            else if(m==null)
            {
                elfit.MoveTo(p);
                return true;
            }
            else if(!check_go(elfit,p.GetLocation()))
            {
                elfit.MoveTo(m);
                return true;
            }
            else if(!check_go(elfit,m.GetLocation()))
            {
                elfit.MoveTo(p);
                return true;
            }
            else if(m.Distance(elfit) < p.Distance(elfit))
            {
                elfit.MoveTo(m);
                return true;
            }
            elfit.MoveTo(p);
            return true;
        }
        
        private MapObject GetNearManaFountains(Elf elfit)
        {
            MapObject p = this.Closest_Object_to_Object(game.GetEnemyManaFountains(),elfit);
            return p;
        }
        
        private MapObject GetNearPortal(Elf elfit)
        {
            MapObject p = this.Closest_Object_to_Object(game.GetEnemyPortals(),elfit);
            return p;
        }
        
        private bool AttackCastle(Elf elfit)
        {
            if(elfit.InAttackRange(game.GetEnemyCastle()))
            {
                elfit.Attack(game.GetEnemyCastle());
                return true;
            }
            return false;
        }
        
        private bool GoToEnemyCastle(Elf elfit)
        {
            elfit.MoveTo(game.GetEnemyCastle());
            return true;
        }
        
        private bool GoToNearPortal(Elf elfit)
        {
            MapObject p = this.Closest_Object_to_Object(game.GetEnemyPortals(),elfit);
            if(p!=null)
            {
                elfit.MoveTo(p);
                return true;
            }
            return false;
        }
        
        private bool GoToNearManaFountains(Elf elfit)
        {
            MapObject p = this.Closest_Object_to_Object(game.GetEnemyManaFountains(),elfit);
            if(p!=null)
            {
                elfit.MoveTo(p);
                return true;
            }
            return false;
        }
        
        private bool AttackElf(Elf elfit,bool loose)
        {
            List<Elf> enemies = new List<Elf>();
            foreach(Elf e in game.GetEnemyLivingElves())
            {
                if(elfit.InAttackRange(e))
                {
                    if(e.CurrentHealth<=elfit.CurrentHealth||loose)
                        enemies.Add(e);
                }
            }
            if(enemies.Count==0)
                return false;
            else if(enemies.Count==1)
            {
                elfit.Attack(enemies[0]);
                return true;
            }
            else
            {
                bool a = true;
                foreach(Elf enemy in enemies)
                {
                    if(con(enemy.GetLocation(),elfit.Id)&&a)
                    {
                        elfit.Attack(enemy);
                        a=false;
                    }
                }
                if(a)
                {
                    foreach(Elf e in game.GetEnemyLivingElves())
                    {
                        if(elfit.InAttackRange(e))
                        {
                            elfit.Attack(e);
                            return true;
                        }
                    }
                }
                else
                    return true;
                    
            }
            return false;
        }
        
        private bool GoToNearEnemyElf(Elf elfit)
        {
            GameObject p = this.Closest_Object_to_Object(game.GetEnemyLivingElves(),elfit);
            if(p!=null &&p.GetLocation().InMap()&& elfit.CurrentHealth >= p.CurrentHealth)
            {
                elfit.MoveTo(p);
                return true;
            }
            return false;
        }
        
        private bool Build_Portal(Elf elfit)
        {
            if(!server.Get_Set())
            {
                bool k = false;
                if(start_building.ContainsValue(true))
                {
                    foreach(Elf elfit2 in game.GetMyLivingElves())
                    {
                        if(start_building[elfit2.Id]==true&&elfit.Distance(elfit2)<game.PortalSize+1)
                        {
                            k=true;
                        }
                    }
                }
                if(!k&&(game.GetEnemyCastle().GetLocation().Distance(elfit.GetLocation())<4600&&elfit.CanBuildPortal())||elfit.Distance(game.GetEnemyCastle())<game.CastleSize*2)
                {
                    if(elfit.CanBuildPortal()&&CanBuildSafely(elfit))
                    {
                        start_building[elfit.Id]=true;
                        elfit.BuildPortal();
                        return true;
                    }
                }
            }
            return false;
        }
        
        private bool Build_Mana_Fountain(Elf elfit)
        { 
            if(game.GetMyManaFountains().Length<5)
            {
                if(elfit.CanBuildManaFountain())
                {
                    elfit.BuildManaFountain();
                    return true;
                }
            }
            return false;
        }
        
        public bool CanBuildSafely(Elf elfit)
        {
            Location tar = target[elfit.Id];
            if(target[elfit.Id] == null || game.GetEnemyIceTrolls().Length == 0)
            { return true; }
            
            //*************************************************************************
            //*************************************************************************
             
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
            
            /// Enemy Elf Info
            Dictionary<int, Location> elf_enemy_loc = new Dictionary<int, Location>();
            Dictionary<int, bool> elf_enemy_act = new Dictionary<int, bool>();
            foreach(Elf againt in game.GetEnemyLivingElves())
            {
                elf_enemy_loc.Add(againt.Id, againt.GetLocation());
                elf_enemy_act.Add(againt.Id, false);
            }
            
            
            dmg = 0;
            
            Location Next_Turn = elfit.GetLocation();
            
            Location elfit_loc = elfit.GetLocation();

            for(int i = 0 ; i < game.PortalBuildingDuration+1; i++)
            {
                ///// HitCheck
                HitCheckElf(troll_loc, troll_hp, troll_enemy_loc, troll_enemy_hp, lava_enemy_loc, lava_enemy_hp, lava_loc, lava_hp, troll_act, troll_enemy_act, elfit_loc, elf_enemy_loc, elf_enemy_act, tornado_enemy_loc, tornado_enemy_hp, tornado_loc, tornado_hp);
                        
                Dictionary<int,int> troll_enemy_hp_old = new Dictionary<int, int>(troll_enemy_hp);
                Dictionary<int,Location> troll_enemy_loc_old = new Dictionary<int, Location>(troll_enemy_loc);
                Dictionary<int,Location> lava_enemy_loc_old = new Dictionary<int, Location>(lava_enemy_loc);
                Dictionary<int,int> lava_enemy_hp_old = new Dictionary<int, int>(lava_enemy_hp);
                Dictionary<int,Location> tornado_enemy_loc_old = new Dictionary<int, Location>(tornado_enemy_loc);
                Dictionary<int,int> tornado_enemy_hp_old = new Dictionary<int, int>(tornado_enemy_hp);
                        
                ///// MoveEnemy
                MoveCheckElf(troll_enemy_loc, troll_enemy_hp, troll_loc, troll_hp, lava_loc, lava_hp, lava_enemy_loc, lava_enemy_hp, troll_enemy_act, game.GetMyCastle(), elfit, elf_enemy_loc, elf_enemy_act, tornado_enemy_loc, tornado_enemy_hp, buildings_loc, buildings_hp, tornado_loc, tornado_hp);
                        
                ///// MoveMy
                MoveCheck(troll_loc, troll_hp, troll_enemy_loc_old, troll_enemy_hp_old, lava_enemy_loc_old, lava_enemy_hp_old, lava_loc, lava_hp, troll_act, game.GetEnemyCastle(), elfit, tornado_loc, tornado_hp, buildings_enemy_loc, buildings_enemy_hp, tornado_enemy_loc_old, tornado_enemy_hp_old);
                    
                ResetTurn(troll_enemy_act);
                ResetTurn(troll_act);
                ResetTurn(elf_enemy_act);
            }
            if(this.dmg >= elfit.CurrentHealth)
            { return false; }
            else
            { return true; }
        }
    
        ///////////////////////////////////////////// Gaby Run
        
        public GameObject Closest_Object_to_Object(GameObject[] f, GameObject w)
        {
            var Sorted = from element in f orderby element.Distance(w) select element;
            
            if(Sorted.ToList().Count != 0)
            { return Sorted.ToList()[0]; }
            else
            { return null; }
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
        
        public List<Location> Closest_MapObject_to_location(Location t, Dictionary<int, Location> troll, Dictionary<int, Location> lava, Dictionary<int, Location> tornado, Location elfit)
        {
            List<Location> enemyloc = new List<Location>();
            enemyloc.AddRange(lava.Values.ToList());
            enemyloc.AddRange(troll.Values.ToList());
            enemyloc.AddRange(tornado.Values.ToList());
            enemyloc.Add(elfit);
            var enemyloc_sorted = from element in enemyloc orderby element.Distance(t) select element;
            return enemyloc_sorted.ToList();
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
        
        public bool Run(Elf elfit,string mode)
        {
            var c = from element in game.GetEnemyLivingElves() orderby element.Distance(elfit) select element;
            
            if(mode.Equals("onlyTrolls")||c.ToList().Count==0)
            {
                if(!check_go(elfit, target[elfit.Id]))
                {
                    List<Location> Run_pos = new List<Location>();
                    
                    for(double i = 0.0; i < 360.0; i=i+0.1)
                    {
                        double angle = (System.Math.PI*i)/180.0;
                        Location circle = new Location((int)(elfit.MaxSpeed*System.Math.Sin(angle)),(int)(elfit.MaxSpeed*System.Math.Cos(angle)));
                        Location l = elfit.GetLocation().Add(circle);
                        if(l.InMap()&&check_go(elfit, l))
                        { 
                            Run_pos.Add(l);
                        }
                    }
                    
                    if(Run_pos.Count > 0)
                    {
                        var closest = from element in Run_pos orderby element.Distance(target[elfit.Id]) select element;
                        elfit.MoveTo(closest.ToList()[0]);
                        return true;
                    }
                    else
                    {
                        if(!elfit.GetLocation().Equals(new Location(-50,-50))&&elfit.CanCastInvisibility())
                        {
                            elfit.CastInvisibility();
                        }
                        else
                        {
                            return false;
                        }
                        return true;
                    }
                }
                else
                { return false; }
            }
            else
            {
                if(!check_go_elf(elfit, target[elfit.Id]))
                {
                    List<Location> Run_pos = new List<Location>();
                    
                    for(double i = 0.0; i < 360.0; i=i+0.1)
                    {
                        double angle = (System.Math.PI*i)/180.0;
                        Location circle = new Location((int)(elfit.MaxSpeed*System.Math.Sin(angle)),(int)(elfit.MaxSpeed*System.Math.Cos(angle)));
                        Location l = elfit.GetLocation().Add(circle);
                        if(l.InMap()&&check_go_elf(elfit, l))
                        { 
                            Run_pos.Add(l);
                        }
                    }
                    
                    if(Run_pos.Count > 0)
                    {
                        var closest = from element in Run_pos orderby element.Distance(target[elfit.Id]) select element;
                        elfit.MoveTo(closest.ToList()[0]);
                        return true;
                    }
                    else
                    {
                        if(!elfit.GetLocation().Equals(new Location(-50,-50))&&elfit.CanCastInvisibility())
                        {
                            elfit.CastInvisibility();
                        }
                        else
                        {
                            return false;
                        }
                        return true;
                    }
                }
                else
                { return false; }
            }
        }
        
        public bool Can_Go_Elf(Elf elfit, Location tar)
        {
            if(tar == null || game.GetEnemyIceTrolls().Length == 0)
            { return true; }
            
            //*************************************************************************
            //*************************************************************************
             
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
            
            /// Enemy Elf Info
            Dictionary<int, Location> elf_enemy_loc = new Dictionary<int, Location>();
            Dictionary<int, bool> elf_enemy_act = new Dictionary<int, bool>();
            foreach(Elf againt in game.GetEnemyLivingElves())
            {
                elf_enemy_loc.Add(againt.Id, againt.GetLocation());
                elf_enemy_act.Add(againt.Id, false);
            }
            
            Location elfit_loc = elfit.GetLocation();

            ///// HitCheck
            HitCheckElf(troll_loc, troll_hp, troll_enemy_loc, troll_enemy_hp, lava_enemy_loc, lava_enemy_hp, lava_loc, lava_hp, troll_act, troll_enemy_act, elfit_loc, elf_enemy_loc, elf_enemy_act, tornado_enemy_loc, tornado_enemy_hp, tornado_loc, tornado_hp);
                    
            Dictionary<int,int> troll_enemy_hp_old = new Dictionary<int, int>(troll_enemy_hp);
            Dictionary<int,Location> troll_enemy_loc_old = new Dictionary<int, Location>(troll_enemy_loc);
            Dictionary<int,Location> lava_enemy_loc_old = new Dictionary<int, Location>(lava_enemy_loc);
            Dictionary<int,int> lava_enemy_hp_old = new Dictionary<int, int>(lava_enemy_hp);
            Dictionary<int,Location> tornado_enemy_loc_old = new Dictionary<int, Location>(tornado_enemy_loc);
            Dictionary<int,int> tornado_enemy_hp_old = new Dictionary<int, int>(tornado_enemy_hp);
            
            ///// MoveEnemy
            MoveCheckElf(troll_enemy_loc, troll_enemy_hp, troll_loc, troll_hp, lava_loc, lava_hp, lava_enemy_loc, lava_enemy_hp, troll_enemy_act, game.GetMyCastle(), elfit, elf_enemy_loc, elf_enemy_act, tornado_enemy_loc, tornado_enemy_hp, buildings_loc, buildings_hp, tornado_loc, tornado_hp);
            
            ///// MoveMy
            MoveCheck(troll_loc, troll_hp, troll_enemy_loc_old, troll_enemy_hp_old, lava_enemy_loc_old, lava_enemy_hp_old, lava_loc, lava_hp, troll_act, game.GetEnemyCastle(), elfit, tornado_loc, tornado_hp, buildings_enemy_loc, buildings_enemy_hp, tornado_enemy_loc_old, tornado_enemy_hp_old);
            
            Location Next_Turn = elfit.GetLocation().Towards(tar, elfit.MaxSpeed);
            
            this.enemy_loc_elf = elf_enemy_loc;
            
            // IceTroll Check
            foreach(Location enemy in troll_enemy_loc.Values)
            {
                if(Next_Turn.InRange(enemy, game.IceTrollAttackRange))
                { 
                    return false; 
                }
            }
            
            // Elf Check
            foreach(Location enemy in elf_enemy_loc.Values)
            {
                if(Next_Turn.InRange(enemy, game.ElfAttackRange))
                { 
                    return false; 
                }
            }
            return true;
        }
        

        public Location GetClosestVertexToTarget(Elf elfit)
        {
            Location[] locs = {new Location(0,0),new Location(0,game.Cols-1),new Location(game.Rows-1,0),new Location(game.Rows-1,game.Cols-1)};
            var sortedLocs = from element in locs orderby element.Distance(target[elfit.Id]) select element;
            return sortedLocs.ToList()[0];
        }
        
        public void ResetTurn(Dictionary<int, bool> act)
        {
            foreach(var k in act.Keys.ToList())
            { act[k] = false; }
        }
        
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
        
        public bool Can_Go(Elf elfit, Location tar)
        {
            if(tar == null || game.GetEnemyIceTrolls().Length == 0)
            { return true; }

                
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
            
            /// Enemy Elf Info
            Dictionary<int, Location> elf_enemy_loc = new Dictionary<int, Location>();
            Dictionary<int, bool> elf_enemy_act = new Dictionary<int, bool>();
            foreach(Elf againt in game.GetEnemyLivingElves())
            {
                elf_enemy_loc.Add(againt.Id, againt.GetLocation());
                elf_enemy_act.Add(againt.Id, false);
            }
            
            Location elfit_loc = elfit.GetLocation();

            ///// HitCheck
            HitCheck(troll_loc, troll_hp, troll_enemy_loc, troll_enemy_hp, lava_enemy_loc, lava_enemy_hp, lava_loc, lava_hp, troll_act, troll_enemy_act, elfit_loc, tornado_enemy_loc, tornado_enemy_hp, tornado_loc, tornado_hp);
                    
            Dictionary<int,int> troll_enemy_hp_old = new Dictionary<int, int>(troll_enemy_hp);
            Dictionary<int,Location> troll_enemy_loc_old = new Dictionary<int, Location>(troll_enemy_loc);
            Dictionary<int,Location> lava_enemy_loc_old = new Dictionary<int, Location>(lava_enemy_loc);
            Dictionary<int,int> lava_enemy_hp_old = new Dictionary<int, int>(lava_enemy_hp);
            Dictionary<int,Location> tornado_enemy_loc_old = new Dictionary<int, Location>(tornado_enemy_loc);
            Dictionary<int,int> tornado_enemy_hp_old = new Dictionary<int, int>(tornado_enemy_hp);
                    
            ///// MoveEnemy
            MoveCheck(troll_enemy_loc, troll_enemy_hp, troll_loc, troll_hp, lava_loc, lava_hp, lava_enemy_loc, lava_enemy_hp, troll_enemy_act, game.GetMyCastle(), elfit, tornado_enemy_loc, tornado_enemy_hp, buildings_loc, buildings_hp, tornado_loc, tornado_hp);
                    
            ///// MoveMy
            MoveCheck(troll_loc, troll_hp, troll_enemy_loc_old, troll_enemy_hp_old, lava_enemy_loc_old, lava_enemy_hp_old, lava_loc, lava_hp, troll_act, game.GetEnemyCastle(), elfit, tornado_loc, tornado_hp, buildings_enemy_loc, buildings_enemy_hp, tornado_enemy_loc_old, tornado_enemy_hp_old);
            
            Location Next_Turn = elfit.GetLocation().Towards(tar, game.ElfMaxSpeed);
            
            
            this.enemy_loc = new Dictionary<int, Location>(troll_enemy_loc);
            
            foreach(Location enemy in troll_enemy_loc.Values)
            {
                if(Next_Turn.InRange(enemy, game.IceTrollAttackRange))
                { 
                    return false; 
                }
            }
            return true;
        }
        
        
        public void HitCheck(Dictionary<int, Location> My_IceTrolls_loc, Dictionary<int, int> My_IceTrolls_hp, Dictionary<int, Location> Enemy_IceTrolls_loc, Dictionary<int, int> Enemy_IceTrolls_hp,
        Dictionary<int,Location> Enemy_Lava_loc, Dictionary<int, int> Enemy_Lava_hp, Dictionary<int, Location> My_Lava_loc, Dictionary<int, int> My_Lava_hp, Dictionary<int, bool> My_Ice_acted, Dictionary<int, bool> Enemy_Ice_acted, Location elfit,
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
                var closest = Closest_MapObject_to_location(Enemy_IceTrolls_loc[def], My_IceTrolls_loc, My_Lava_loc, My_Tornado_loc, elfit.GetLocation());
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
                    // Elf Hit
                    else if(closest[i].Equals(elfit.GetLocation()) && Enemy_IceTrolls_loc[def].InRange(closest[i], game.IceTrollAttackRange))
                    {
                        if(Enemy_IceTrolls_hp[def] != 0)
                        {
                            Enemy_IceTrolls_hp[def] -= game.IceTrollSuffocationPerTurn; // end turn for this icetroll
                            Enemy_Ice_acted[def] = true; // end turn for this icetroll
                            break;
                        }
                    }
                }
            }
        }
        
        public void MoveCheck(Dictionary<int, Location> My_IceTrolls_loc, Dictionary<int, int> My_IceTrolls_hp, Dictionary<int, Location> Enemy_IceTrolls_loc, Dictionary<int, int> Enemy_IceTrolls_hp,
        Dictionary<int,Location> Enemy_Lava_loc, Dictionary<int, int> Enemy_Lava_hp, Dictionary<int, Location> My_Lava_loc, Dictionary<int, int> My_Lava_hp, Dictionary<int, bool> My_Ice_acted, Castle c, Elf elfit,
        Dictionary<int,Location> My_Tornado_loc, Dictionary<int, int> My_Tornado_hp, Dictionary<int,Location> Enemy_Building_loc, Dictionary<int, int> Enemy_Building_hp, Dictionary<int,Location> Enemy_Tornado_loc, Dictionary<int, int> Enemy_Tornado_hp)
        {
            List<int> troll_keys = new List<int>(My_IceTrolls_loc.Keys);
            List<int> lava_keys = new List<int>(My_Lava_loc.Keys);
            List<int> tornado_keys = new List<int>(My_Tornado_loc.Keys);

            //////////////// Move Check
            foreach(var def in troll_keys)
            {
                var closest = Closest_MapObject_to_location(My_IceTrolls_loc[def], Enemy_IceTrolls_loc, Enemy_Lava_loc, Enemy_Tornado_loc, elfit.GetLocation());
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
                    
                    // Elf Move
                    else if(closest[i].Equals(elfit.GetLocation()) && !My_Ice_acted[def])
                    {
                        if(My_IceTrolls_hp[def] != 0 && c.Equals(game.GetMyCastle()))
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
                            /// Attack Building
                            if(My_Tornado_hp[att] != 0 && !My_Tornado_loc[att].InRange(Enemy_Building_loc[key], game.ManaFountainSize + game.TornadoAttackRange))
                            {
                                My_Tornado_loc[att] = My_Tornado_loc[att].Towards(closest[i], game.TornadoMaxSpeed); // Move
                                My_Tornado_hp[att] -= game.TornadoSuffocationPerTurn; // End Turn
                                break;
                            }
                            /// Move To Building
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
                            /// Attack Building
                            if(My_Tornado_hp[att] != 0 && !My_Tornado_loc[att].InRange(Enemy_Building_loc[key], game.PortalSize + game.TornadoAttackRange))
                            {
                                My_Tornado_loc[att] = My_Tornado_loc[att].Towards(closest[i], game.TornadoMaxSpeed); // Move
                                My_Tornado_hp[att] -= game.TornadoSuffocationPerTurn; // End Turn
                                break;
                            }
                            /// Move To Building
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


        public void HitCheckElf(Dictionary<int, Location> My_IceTrolls_loc, Dictionary<int, int> My_IceTrolls_hp, Dictionary<int, Location> Enemy_IceTrolls_loc, Dictionary<int, int> Enemy_IceTrolls_hp,
        Dictionary<int,Location> Enemy_Lava_loc, Dictionary<int, int> Enemy_Lava_hp, Dictionary<int, Location> My_Lava_loc, Dictionary<int, int> My_Lava_hp, Dictionary<int, bool> My_Ice_acted, Dictionary<int, bool> Enemy_Ice_acted, Location elfit, Dictionary<int, Location> Enemy_Elf_Loc, Dictionary<int, bool> Enemy_Elf_Acted,
        Dictionary<int,Location> Enemy_Tornado_loc, Dictionary<int, int> Enemy_Tornado_hp, Dictionary<int,Location> My_Tornado_loc, Dictionary<int, int> My_Tornado_hp)
        {
            List<int> troll_keys = new List<int>(My_IceTrolls_loc.Keys);
            List<int> troll_enemy_keys = new List<int>(Enemy_IceTrolls_loc.Keys);
            List<int> elf_enemy_keys = new List<int>(Enemy_Elf_Loc.Keys);
            
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
            
            foreach(var def in elf_enemy_keys)
            {
                if(Enemy_Elf_Loc[def].InRange(elfit, game.ElfAttackRange))
                { 
                    this.dmg = this.dmg + game.ElfAttackMultiplier;
                    Enemy_Elf_Acted[def] = true; //end turn for this elf 
                }
            }
            
            ////// Enemy icetroll
            foreach(var def in troll_enemy_keys)
            {
                var closest = Closest_MapObject_to_location(Enemy_IceTrolls_loc[def], My_IceTrolls_loc, My_Lava_loc, My_Tornado_loc, elfit.GetLocation());
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
                    // Elf Hit
                    else if(closest[i].Equals(elfit.GetLocation()) && Enemy_IceTrolls_loc[def].InRange(closest[i], game.IceTrollAttackRange))
                    {
                        if(Enemy_IceTrolls_hp[def] != 0)
                        {
                            this.dmg = this.dmg + game.IceTrollAttackMultiplier;
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

        public void MoveCheckElf(Dictionary<int, Location> My_IceTrolls_loc, Dictionary<int, int> My_IceTrolls_hp, Dictionary<int, Location> Enemy_IceTrolls_loc, Dictionary<int, int> Enemy_IceTrolls_hp,
        Dictionary<int,Location> Enemy_Lava_loc, Dictionary<int, int> Enemy_Lava_hp, Dictionary<int, Location> My_Lava_loc, Dictionary<int, int> My_Lava_hp, Dictionary<int, bool> My_Ice_acted, Castle c, Elf elfit, Dictionary<int, Location> Enemy_Elf_Loc, Dictionary<int, bool> Enemy_Elf_Acted,
        Dictionary<int,Location> My_Tornado_loc, Dictionary<int, int> My_Tornado_hp, Dictionary<int,Location> Enemy_Building_loc, Dictionary<int, int> Enemy_Building_hp, Dictionary<int,Location> Enemy_Tornado_loc, Dictionary<int, int> Enemy_Tornado_hp)
        {
            List<int> troll_keys = new List<int>(My_IceTrolls_loc.Keys);
            List<int> lava_keys = new List<int>(My_Lava_loc.Keys);
            List<int> elf_keys = new List<int>(Enemy_Elf_Loc.Keys);
            List<int> tornado_keys = new List<int>(My_Tornado_loc.Keys);
            
            //////////////// Move Check
            foreach(var def in troll_keys)
            {
                var closest = Closest_MapObject_to_location(My_IceTrolls_loc[def], Enemy_IceTrolls_loc, Enemy_Lava_loc, Enemy_Tornado_loc, elfit.GetLocation());
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
                    
                    // Elf Move
                    else if(closest[i].Equals(elfit.GetLocation()) && !My_Ice_acted[def])
                    {
                        if(My_IceTrolls_hp[def] != 0 && c.Equals(game.GetMyCastle()))
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
            
            foreach(var def in elf_keys)
            {
                if(!Enemy_Elf_Acted[def])
                { Enemy_Elf_Loc[def] = Enemy_Elf_Loc[def].Towards(elfit, game.ElfMaxSpeed); }
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
                            /// Attack Building
                            if(My_Tornado_hp[att] != 0 && !My_Tornado_loc[att].InRange(Enemy_Building_loc[key], game.ManaFountainSize + game.TornadoAttackRange))
                            {
                                My_Tornado_loc[att] = My_Tornado_loc[att].Towards(closest[i], game.TornadoMaxSpeed); // Move
                                My_Tornado_hp[att] -= game.TornadoSuffocationPerTurn; // End Turn
                                break;
                            }
                            /// Move To Building
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
                            /// Attack Building
                            if(My_Tornado_hp[att] != 0 && !My_Tornado_loc[att].InRange(Enemy_Building_loc[key], game.PortalSize + game.TornadoAttackRange))
                            {
                                My_Tornado_loc[att] = My_Tornado_loc[att].Towards(closest[i], game.TornadoMaxSpeed); // Move
                                My_Tornado_hp[att] -= game.TornadoSuffocationPerTurn; // End Turn
                                break;
                            }
                            /// Move To Building
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