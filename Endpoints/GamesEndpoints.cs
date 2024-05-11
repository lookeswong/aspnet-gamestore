using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    const string getGameEndpointName = "GetGame";

    public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app)  {

        var group = app.MapGroup("games")
                        .WithParameterValidation();

        // GET /games
        group.MapGet("/", async (GameStoreContext dbContext) => 
            await dbContext.Games
                .Include(game => game.Genre)
                .Select(game => game.ToGameSummaryDto())
                .AsNoTracking()
                .ToListAsync()
        );

        // GET /games/1
        group.MapGet("/{id}", async (int id, GameStoreContext dbContext) => {
            Game? game = await dbContext.Games.FindAsync(id);

            return game is null ? 
                Results.NotFound() : 
                Results.Ok(game.ToGameDetailsDto());

        })
        .WithName(getGameEndpointName);

        // POST /games
        group.MapPost("/", async (CreateGameDto newGame, GameStoreContext dbContext) => {

            // GameDto game = new (
            //     games.Count + 1,
            //     newGame.Name,
            //     newGame.Genre,
            //     newGame.Price,
            //     newGame.ReleaseDate
            // );
            // games.Add(game);

            Game game = newGame.ToEntity();
            // game.Genre = dbContext.Genres.Find(newGame.GenreId);

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(
                getGameEndpointName, 
                new { id = game.Id }, 
                game.ToGameDetailsDto()
            );
        });

        // PUT /games/1
        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext dbContext) => {
            

            // var index = games.FindIndex(game => game.Id == id);
            // if (index == -1) {
            //     return Results.NotFound();
            // }

            // games[id] = new GameSummaryDto(
            //     id,
            //     updatedGame.Name,
            //     updatedGame.Genre,
            //     updatedGame.Price,
            //     updatedGame.ReleaseDate
            // );

            var existingGame = await dbContext.Games.FindAsync(id);
            if (existingGame is null) {
                return Results.NotFound();
            }

            dbContext.Entry(existingGame)
                .CurrentValues
                .SetValues(updatedGame.ToEntity(id));

            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        });

        // DELETE /games/1
        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) => {

            // games.RemoveAll(game => game.Id == id);

            await dbContext.Games
                .Where(game => game.Id == id)
                .ExecuteDeleteAsync();

            return Results.NoContent();
        });

        return group;
    }
}