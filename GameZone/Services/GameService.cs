
namespace GameZone.Services
{
    public class GameService : IGameService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _ImagesPath;

        public GameService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _ImagesPath = $"{_webHostEnvironment.WebRootPath}{FileSettings.ImagesPath}";
        }

        public IEnumerable<Game> GetGames()
        {
            return _context.Games
                .Include(g => g.Category)
                .Include(g => g.Devices)
                .ThenInclude(d => d.Device)
                .AsNoTracking()
                .ToList();
        }

        public async Task Create(CreateGameFormViewModel model)
        {
            try
            {
                var coverName = await SaveCover(model.Cover);
                Game game = new()
                {
                    Name = model.Name,
                    Description = model.Description,
                    CategoryId = model.CategoryId,
                    Cover = coverName,
                    Devices = model.SelectedDevices.Select(d => new GameDevice { DeviceId = d }).ToList()
                };

                _context.Add(game);
                await _context.SaveChangesAsync(); // Asynchronously save to database
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while creating the game.", ex);
            }
        }

        public Game? GetGameById(int id)
        {
            return _context.Games
                .Include(g => g.Category)
                .Include(g => g.Devices)
                .ThenInclude(d => d.Device)
                .AsNoTracking()
                .SingleOrDefault(g => g.Id == id);
        }

        public async Task<Game?> Update(EditGameFormViewModel model)
        {
            var game = _context.Games
                .Include(g => g.Devices)
                .SingleOrDefault(g => g.Id == model.Id);

            if (game is null)
            {
                return null;
            }

            var hasNewCover = model.Cover is not null;
            var currentCover = game.Cover;

            game.Name = model.Name;
            game.Description = model.Description;
            game.CategoryId = model.CategoryId;
            game.Devices = model.SelectedDevices.Select(d => new GameDevice { DeviceId = d }).ToList();

            if (hasNewCover)
            {
                game.Cover = await SaveCover(model.Cover!);
            }

            var effectedRows = _context.SaveChanges();

            //Delete old cover from server
            if (effectedRows > 0)
            {
                if (hasNewCover)
                {
                    var cover = Path.Combine(_ImagesPath, currentCover);
                    File.Delete(cover);
                }

                return game;
            }
            else
            {
                var cover = Path.Combine(_ImagesPath, game.Cover);
                File.Delete(cover);

                return null;
            }
        }

        public bool Delete(int id)
        {
            var isDeleted = false;

            var game = _context.Games.Find(id);
            if (game is null)
            {
                return isDeleted;
            }

            _context.Remove(game);

            var effecedRows = _context.SaveChanges();
            if (effecedRows > 0)
            {
                isDeleted = true;
                var cover = Path.Combine(_ImagesPath, game.Cover);
                File.Delete(cover);
            }

            return isDeleted;
        }

        private async Task<string> SaveCover(IFormFile cover)
        {
            var coverName = $"{Guid.NewGuid()}{Path.GetExtension(cover.FileName)}";
            var path = Path.Combine(_ImagesPath, coverName);

            // Asynchronously save the file
            using (var stream = File.Create(path))
            {
                await cover.CopyToAsync(stream);
            }

            return coverName;
        }
    }
}
