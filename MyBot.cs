using ElfKingdom;
namespace MyBot
{
    public class TutorialBot : ISkillzBot
    {
        int i = 999;
        
        public void DoTurn(Game game)
        {
            try
            {
                // Give orders to my elves.
                this.HandleElves(game);
            }
            catch{}
                
            try
            {
                // Give orders to my portals.
                this.HandlePortals(game);
            }
            catch{}
            try
            {
                if(game.GetEnemyCastle().CurrentHealth==1)
                { game.Debug("game.Win()"); }

                if( i > game.GetTimeRemaining())
                {
                    i = game.GetTimeRemaining();
                }
                game.Debug(i);
            }
            catch{}
        }

        private void HandleElves(Game game)
        {
            //ElfiotBeta act = ElfiotBeta.ElfiotBetaInstance(game);
            //act.main();

            Elfiot act = Elfiot.ElfiotInstance(game);
            act.main();
        }

        private void HandlePortals(Game game)
        {
            HandlePort act = new HandlePort(game);
            act.main();
        }
 
    }
}


//// Euclidean Geometry?