namespace Com.Gmlan.Middlewares.MongoDB.Model
{
    public class MongoDbConfig
    {
        /// <summary>
        ///     Default parameterless constructor
        /// </summary>
        public MongoDbConfig()
        {
        }


        /// <summary>
        ///     Constructor for MongoDbConfig
        /// </summary>
        /// <param name="constr">MogoDB connection string</param>
        /// <param name="database">database name to connect</param>
        public MongoDbConfig(string constr, string database)
        {
            ConnectionString = constr;
            Database = database;
        }

        public string ConnectionString { get; set; }

        public string Database { get; set; }
    }
}
