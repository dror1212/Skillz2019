using ElfKingdom;
using System.Collections.Generic; 
using System.Linq;

namespace MyBot
{
    public class TargetUpdate
    {
        private static TargetUpdate instance;
        public Game game;
        public Server server;
        public Dictionary<int,bool> start_building;
        public Dictionary<int,Location> targets;
        public int dmg;
        
        
        
        public TargetUpdate(Game game)
        { 
            this.dmg=0;
            this.game = game;
            this.server = new Server();
            this.start_building=new Dictionary<int,bool>();
            this.targets = new Dictionary<int,Location>();
            int counter=0;
            foreach(Elf elfit in game.GetAllMyElves())
            {                
                start_building.Add(counter,false);
                targets.Add(counter,null);
                counter++;
            }
        }
        
        public static TargetUpdate TargetInstance(Game game)
        {
            if(instance == null)
            { instance = new TargetUpdate(game); }
            return instance;
        }
        
        public Dictionary<int,Location> Update()
        {
            List<Elf> sortedElves = new List<Elf>();
            
            int check =999;
            
            if(game.GetMyLivingElves().Length>0)
            {
                //////////////////////////
                sortedElves = (from element in game.GetMyLivingElves() orderby element.Distance(game.GetMyCastle()) select element).ToList();
                Location p3 = DestroyEnemyManaFountains(sortedElves[sortedElves.Count-1]);
                if(p3!=null)
                {
                    targets[sortedElves[sortedElves.Count-1].Id] = p3;
                    check = sortedElves[sortedElves.Count-1].Id;
                }
                /////////////////////////////
                foreach(Elf elfit2 in game.GetAllMyElves())
                {
                    if(!elfit2.IsAlive())
                    {
                        sortedElves.Add(elfit2);
                    }
                }
            }
            foreach(Elf elfit in sortedElves)
            {
                try
                {
                    Location up = null;
                    if(elfit.IsAlive()&&elfit.Id!=check)
                    {
                        if(!elfit.IsBuilding)
                        {
                            if(up==null)
                            {
                                up = HelpDefenceAgainstPortal(elfit);
                            }
                            if(up==null)
                            {
                                up = HelpDefenceAgainstElf(elfit);
                            }
                            if(up==null)
                            {
                                up = ProtectMyManaFountains(elfit);
                            }
                            if(up==null)
                            {
                                up = GoToNearEnemyElf(elfit);
                            }
                            if(up==null)
                            {
                                up = ChooseClosestManaOrPortal(elfit);
                            }
                            if(up==null)
                            {
                                up = GoToEnemyCastle(elfit);
                            }
                        }
                        else
                            start_building[elfit.Id]=false;
                    }
                    targets[elfit.Id]=up;
                }
                catch{}
            }
            return targets;
        }
        
        public Location ProtectMyManaFountains(Elf elfit)
        {
            foreach(Elf enemy in game.GetEnemyLivingElves())
            {
                foreach(ManaFountain mana in game.GetMyManaFountains())
                {
                    if(enemy.InRange(mana,enemy.AttackRange*2+mana.Size)&&!con(enemy.GetLocation(),elfit.Id))
                    {
                        return enemy.GetLocation();
                    }
                }
            }
            return null;
        }
        
        public Location DestroyEnemyManaFountains(Elf elfit)
        {
            foreach(ManaFountain em in game.GetEnemyManaFountains())
            {
                if(elfit.InRange(em,game.ElfMaxSpeed * game.SpeedUpMultiplier * game.SpeedUpExpirationTurns + game.ElfMaxSpeed * 6 + em.Size + elfit.AttackRange))
                {
                    if(elfit.MaxSpeed==game.ElfMaxSpeed)
                    {
                        if(!elfit.InRange(em,game.ElfMaxSpeed * 2 + em.Size + elfit.AttackRange) && elfit.CanCastSpeedUp())
                        {
                            return em.GetLocation();
                        }
                        else
                        {
                            return em.GetLocation();
                        }
                    }
                    else
                    {
                        return em.GetLocation();
                    }
                }
            }
            return null;
        }
        
        public Location HelpDefenceAgainstPortal(Elf elfit)
        {
            MapObject p = Closest_Object_to_Object(game.GetEnemyPortals(),game.GetMyCastle());
            if(p!=null)
            {
                Location x = Build_Portal(elfit);
                if(x==null)
                {
                    if(p.Distance(game.GetMyCastle())<game.GetEnemyCastle().Distance(game.GetMyCastle())/2)
                    {
                        return p.GetLocation();
                    }
                }
                else
                    return x;
            }
            return null;
        }
        
        public Location HelpDefenceAgainstElf(Elf elfit)
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
                            return elfitEnemy.GetLocation();
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            return null;
        }
        

        private Location AttackPortal(Elf elfit)
        {
            foreach(Portal p in game.GetEnemyPortals())
            {
                if(elfit.InAttackRange(p))
                {
                    return p.GetLocation();
                }
            }
            return null;
        }

        private Location Build_Portal(Elf elfit)
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
                        return elfit.GetLocation();
                    }
                }
            }
            return null;
        }
        
        private Location ChooseClosestManaOrPortal(Elf elfit)
        {
            MapObject p = GetNearPortal(elfit);
            MapObject m = GetNearManaFountains(elfit);
            if(p==null&&m==null)
            {
                return null;
            }
            else if(p==null)
            {
                return m.GetLocation();
            }
            else if(m==null)
            {
                return p.GetLocation();
            }
            else if(!Can_Go(elfit,p.GetLocation()))
            {
                return m.GetLocation();
            }
            else if(!Can_Go(elfit,m.GetLocation()))
            {
                return p.GetLocation();
            }
            else if(m.Distance(elfit) < p.Distance(elfit))
            {
                return m.GetLocation();
            }
            return p.GetLocation();
        }
        private Location GoToNearEnemyElf(Elf elfit)
        {
            GameObject p = this.Closest_Object_to_Object(game.GetEnemyLivingElves(),elfit);
            if(p!=null &&p.GetLocation().InMap()&& elfit.CurrentHealth >= p.CurrentHealth)
            {
                return p.GetLocation();
            }
            return null;
        }
        
        private Location GoToEnemyCastle(Elf elfit)
        {
            return game.GetEnemyCastle().GetLocation();;
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
        
        public bool con(Location dot,int i)
        {
            foreach(int j in targets.Keys.ToList())
            {
                Location l = targets[j];
                if(j!=i&&dot.Equals(l))
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool CanBuildSafely(Elf elfit)
        {
            Location tar = targets[elfit.Id];
            if(targets[elfit.Id] == null || game.GetEnemyIceTrolls().Length == 0)
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
        
        
        public void ResetTurn(Dictionary<int, bool> act)
        {
            foreach(var k in act.Keys.ToList())
            { act[k] = false; }
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
        
        public GameObject Closest_Object_to_Object(GameObject[] f, GameObject w)
        {
            var Sorted = from element in f orderby element.Distance(w) select element;
            
            if(Sorted.ToList().Count != 0)
            { return Sorted.ToList()[0]; }
            else
            { return null; }
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
        
        public bool Can_Go(Elf elfit, Location tar)
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