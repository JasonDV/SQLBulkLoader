namespace ivaldez.Sql.SqlBulkLoader
{
    /// <summary>
    /// Factory class used to create default instances of the BulkLoader
    /// </summary>
    public class BulkLoaderFactory
    {
        public static IBulkLoader Create()
        {
            return new BulkLoader(new SqlBulkLoadUtility());
        }
    }
}