using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using AutoGladiators.Core.Core;                 // GladiatorBot (adjust if your namespace differs)
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Core.Services.Storage;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services
{
    public class DatabaseService
    {
        private readonly IAppStorage _storage;
        private readonly SQLiteAsyncConnection _db;
        private static readonly ILogger Log = (ILogger)AppLog.For("DatabaseService");

        public DatabaseService(IAppStorage storage)
        {
            _storage = storage;
            var dbPath = Path.Combine(_storage.AppDataPath, "AppDatabase.db");
            _db = new SQLiteAsyncConnection(dbPath);
        }

        private bool _initialized;
        private async Task EnsureInitializedAsync()
        {
            if (_initialized) return;
            await _db.CreateTableAsync<GladiatorBot>().ConfigureAwait(false);
            _initialized = true;
        }

        public async Task AddBotAsync(GladiatorBot bot)
        {
            await EnsureInitializedAsync().ConfigureAwait(false);
            await _db.InsertAsync(bot).ConfigureAwait(false);
        }

        public async Task<List<GladiatorBot>> GetBotsAsync()
        {
            await EnsureInitializedAsync().ConfigureAwait(false);
            return await _db.Table<GladiatorBot>().ToListAsync().ConfigureAwait(false);
        }

        public async Task DeleteBotAsync(int id)
        {
            await EnsureInitializedAsync().ConfigureAwait(false);
            await _db.DeleteAsync<GladiatorBot>(id).ConfigureAwait(false);
        }
    }
}
