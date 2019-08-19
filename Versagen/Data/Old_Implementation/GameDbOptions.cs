using Microsoft.EntityFrameworkCore;

namespace Versagen.Data
{
    public class GameDbOptions
    {
        public class Builder : DbContextOptionsBuilder
        {
            internal string DbSchema { get; set; }

            public string DbConnectionString { get; set; }

            public GameDbOptions Build() => new GameDbOptions(this);

        }

        public string DbSchema { get; }

        public string DbConnectionString { get; }

        internal DbContextOptionsBuilder DbBuilder { get; }

        protected GameDbOptions(Builder b)
        {
            DbSchema = b.DbSchema;
            DbBuilder = b;
        }
    }
}
