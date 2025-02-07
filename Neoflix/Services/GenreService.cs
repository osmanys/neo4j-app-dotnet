﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neoflix.Example;
using Neoflix.Exceptions;

namespace Neoflix.Services
{
    public class GenreService
    {
        private readonly IDriver _driver;

        /// <summary>
        /// Initializes a new instance of <see cref="GenreService"/> that handles genre database calls.
        /// </summary>
        /// <param name="driver">Instance of Neo4j Driver, which will be used to interact with Neo4j</param>
        public GenreService(IDriver driver)
        {
            _driver = driver;
        }

        /// <summary>
        /// Get a list of genres from the database with "name" property.<br/>
        /// "movies" property which is the count of incoming "IN_GENRE" relationships.<br/>
        /// and "poster" property.<br/>
        /// <code>
        /// [
        ///   {
        ///     name: 'Action',
        ///     movies: 1545,
        ///     poster: 'https://image.tmdb.org/t/p/w440_and_h660_face/qJ2tW6WMUDux911r6m7haRef0WH.jpg'
        ///   }, ...
        /// ]
        /// </code>
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.<br/>
        /// The task result contains a list of records.
        /// </returns>
        // tag::all[]
        public async Task<Dictionary<string, object>[]> AllAsync()
        {
            await using var session = _driver.AsyncSession();
            return await session.ExecuteReadAsync(async tx =>
            {
                var query = @"
            MATCH (g:Genre)
            WHERE g.name <> '(no genres listed)'
            CALL {
                WITH g
                MATCH (g)<-[:IN_GENRE]-(m:Movie)
                WHERE m.imdbRating IS NOT NULL
                AND m.poster IS NOT NULL
                RETURN m.poster AS poster
                ORDER BY m.imdbRating DESC LIMIT 1
            }
            RETURN g {
                .*,
                movies: count { (g)<-[:IN_GENRE]-(:Movie) },
                poster: poster
            } as genre
            ORDER BY g.name ASC";
                var cursor = await tx.RunAsync(query);
                var records = await cursor.ToListAsync();

                return records
                    .Select(x => x["genre"].As<Dictionary<string, object>>())
                    .ToArray();
            });
        }
        // end::all[]

        /// <summary>
        /// Get a Genre node by its name and return a set of properties along with a "poster" image and "movies" count.<br/><br/>
        /// If the genre is not found, a NotFoundException should be thrown.
        /// </summary>
        /// <param name="name">The name of the genre.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.<br/>
        /// The task result contains a record.
        /// </returns>
        // tag::find[]
        public Task<Dictionary<string, object>> FindGenreAsync(string name)
        {
            // TODO: Open a new session
            // TODO: Get Genre information from the database
            // TODO: return null if the genre is not found
            // TODO: Close the session

            return Task.FromResult(
                Fixtures
                    .Genres
                    .OfType<Dictionary<string, object>>()
                    .First(x => x["name"] == name));
        }
        // end::find[]
    }
}