using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theatre.Data.SqlQuery
{
    public class SqlQueries
    {
        public const string connectionString = "Host=localhost;Port=5432;Database=Users;Username=postgres;Password=Mailgame5250580;";

        public const string SqlCheckAdminKey = @"SELECT COUNT(*) FROM adminKey WHERE code = @code";

        public const string SqlLoadImageFromDataBase = @"SELECT name_spectacle, photo_spectacle FROM spectacles";

        public const string SqlLoadSpectacles = @"SELECT id_spectacle, name_spectacle, quantity_actors, photo_spectacle FROM spectacles";

        public const string SqlLoadGenre = @"SELECT id_genre, name_genre FROM genre";

        public const string SqlLoadSpectacleGenre = @"SELECT id_genre, id_spectacle FROM spectacle_genre";

        public const string SqlInsertUser = @"INSERT INTO users(login, password, email, id_type_user) 
                            VALUES(@login, @password, @email, @Id_type_user)";

        public const string SqlCheckUser = @"SELECT COUNT(*) FROM users WHERE login = @login AND password = @password";

        public const string SqlCheckUserWithEmail = @"SELECT COUNT(*) FROM users WHERE login = @login OR email = @email";

        public const string SqlGenres = @"SELECT id_genre, id_spectacle FROM spectacle_genre";

        public const string SqlSeats = @"SELECT number_seat, price, isbooked, id_spectacle FROM seat";

        public const string SqlGetUser = @"SELECT id_user, login, password, email, id_type_user FROM users WHERE login = @login";
    }
}
