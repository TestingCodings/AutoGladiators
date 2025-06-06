using SQLite;
using AutoGladiators.Client.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoGladiators.Client.Models;
using System.IO;

namespace AutoGladiators.Client.Services
{
    public static class DatabaseService
    {
        static SQLiteAsyncConnection db;

        public static async Task InitAsync()
        {
            if (db != null) return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "AppDatabase.db");
            db = new SQLiteAsyncConnection(dbPath);
            await db.CreateTableAsync<GladiatorBot>();
        }

        public static async Task AddBotAsync(GladiatorBot bot)
        {
            await InitAsync();
            await db.InsertAsync(bot);
        }

        public static async Task<List<GladiatorBot>> GetBotsAsync()
        {
            await InitAsync();
            return await db.Table<GladiatorBot>().ToListAsync();
        }

        public static async Task DeleteBotAsync(int id)
        {
            await InitAsync();
            await db.DeleteAsync<GladiatorBot>(id);
        }
    }
}
