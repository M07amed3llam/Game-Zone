namespace GameZone.Services
{
    public interface IGameService
    {
        IEnumerable<Game> GetGames();
        Game? GetGameById(int id);
        Task Create(CreateGameFormViewModel game);
        Task<Game?> Update(EditGameFormViewModel game);
        bool Delete(int id);

    }
}
