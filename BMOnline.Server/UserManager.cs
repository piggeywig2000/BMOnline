using System.Diagnostics.CodeAnalysis;
using BMOnline.Common.Gamemodes;
using BMOnline.Server.Gamemodes;

namespace BMOnline.Server
{
    internal class UserManager
    {
        private readonly Dictionary<ushort, User> idToUser = new Dictionary<ushort, User>();
        private readonly Dictionary<uint, User> secretToUser = new Dictionary<uint, User>();
        private readonly Dictionary<OnlineGamemode, IGamemodeBase> gamemodes = new Dictionary<OnlineGamemode, IGamemodeBase>();

        public int TotalCount => secretToUser.Count;
        public IReadOnlyCollection<User> Users => secretToUser.Values;

        public bool UserIdExists(ushort id) => idToUser.ContainsKey(id);
        public bool UserSecretExists(uint secret) => secretToUser.ContainsKey(secret);
        public bool TryGetUserFromId(ushort id, [NotNullWhen(returnValue: true)] out User? user) => idToUser.TryGetValue(id, out user);
        public bool TryGetUserFromSecret(uint secret, [NotNullWhen(returnValue: true)] out User? user) => secretToUser.TryGetValue(secret, out user);
        public User GetUserFromId(ushort id) => idToUser[id];
        public User GetUserFromSecret(uint secret) => secretToUser[secret];

        public void RegisterGamemode(IGamemodeBase gamemode)
        {
            gamemodes.Add(gamemode.GamemodeType, gamemode);
        }

        public void AddUser(User user)
        {
            idToUser.Add(user.Id, user);
            secretToUser.Add(user.Secret, user);
        }

        public User[] RemoveExpired(TimeSpan currentTime)
        {
            User[] expiredUsers = secretToUser.Values.Where(u => u.IsExpired(currentTime)).ToArray();
            foreach (User user in expiredUsers)
            {
                idToUser.Remove(user.Id);
                secretToUser.Remove(user.Secret);
                if (gamemodes.TryGetValue((OnlineGamemode)user.Mode, out IGamemodeBase? gamemode))
                    gamemode.RemoveUser(user.Id);
            }
            return expiredUsers;
        }

        public void ChangeUserMode(uint userSecret, byte newMode)
        {
            if (!secretToUser.TryGetValue(userSecret, out User? user))
                throw new ArgumentException("User with secret doesn't exist", nameof(userSecret));

            if (gamemodes.TryGetValue((OnlineGamemode)user.Mode, out IGamemodeBase? gamemode))
                gamemode.RemoveUser(user.Id);

            user.Mode = newMode;

            if (gamemodes.TryGetValue((OnlineGamemode)newMode, out gamemode))
                gamemode.AddUser(user);
        }
    }
}
