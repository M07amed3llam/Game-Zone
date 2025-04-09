namespace GameZone.Controllers
{
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICategoriesService _categoriesService;
        private readonly IDevicesService _devicesService;
        private readonly IGameService _gameService;

        public GamesController(ApplicationDbContext context, ICategoriesService categoriesService,
            IDevicesService devicesService, IGameService gameService)
        {
            _context = context;
            _categoriesService = categoriesService;
            _devicesService = devicesService;
            _gameService = gameService;
        }

        public IActionResult Index()
        {
            var games = _gameService.GetGames();
            return View(games);
        }

        public IActionResult Details(int id)
        {
            var game = _gameService.GetGameById(id);
            if (game is null)
            {
                return NotFound();
            }
            return View(game);
        }

        [HttpGet]
        public IActionResult Create()
        {
            CreateGameFormViewModel viewModel = new()
            {
                Categories = _categoriesService.GetCategories(),
                Devices = _devicesService.GetDevices(),
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGameFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = _categoriesService.GetCategories();
                model.Devices = _devicesService.GetDevices();

                return View(model);
            }

            await _gameService.Create(model);
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var game = _gameService.GetGameById(id);
            if (game is null)
            {
                return NotFound();
            }

            EditGameFormViewModel viewModel = new()
            {
                Id = id,
                Name = game.Name,
                Description = game.Description,
                CategoryId = game.CategoryId,
                SelectedDevices = game.Devices.Select(d => d.DeviceId).ToList(),
                Categories = _categoriesService.GetCategories(),
                Devices = _devicesService.GetDevices(),
                CurrentCover = game.Cover,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditGameFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = _categoriesService.GetCategories();
                model.Devices = _devicesService.GetDevices();

                return View(model);
            }

            var game = await _gameService.Update(model);
            if (game is null)
            {
                return BadRequest(); 
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var isDeleted = _gameService.Delete(id);
            return isDeleted ? Ok() : BadRequest();         
        }
    }
}
