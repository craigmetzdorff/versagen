using Microsoft.EntityFrameworkCore;

namespace Versagen.Data
{
    public abstract class GameDB<TUserIDMap, TInventory, TPlayer, TEntity, TStory> : DbContext where TUserIDMap: class, IAuthMapper where TInventory: InventoryMap where TPlayer:Player where TEntity:EntityStore where TStory: StoryListing
    {
        public abstract DbSet<TPlayer> Players { get; set; }
        public abstract DbSet<TEntity> Entities { get; set; }
        public abstract DbSet<TUserIDMap> AuthTranslator { get; set; }
        public abstract DbSet<TStory> Stories { get; set; }
        public abstract DbSet<TInventory> Inventories { get; set; }
        GameDbOptions GameOptions { get; }



        public GameDB(GameDbOptions options): base(options.DbBuilder.Options)
        {
            GameOptions = options;
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            
            
        }
    }
}
