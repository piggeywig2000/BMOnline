using System.Diagnostics.CodeAnalysis;
using BMOnline.Common;

namespace BMOnline.Server
{
    internal class UserManager
    {
        private readonly Dictionary<ushort, User> idToUser = new Dictionary<ushort, User>();
        private readonly Dictionary<uint, User> secretToUser = new Dictionary<uint, User>();
        private readonly SecretsDict<ushort> stageToSecrets = new SecretsDict<ushort>();

        public int TotalCount => secretToUser.Count;
        public IReadOnlyCollection<User> Users => secretToUser.Values;

        public bool UserIdExists(ushort id) => idToUser.ContainsKey(id);
        public bool UserSecretExists(uint secret) => secretToUser.ContainsKey(secret);
        public bool TryGetUserFromId(ushort id, [NotNullWhen(returnValue: true)] out User? user) => idToUser.TryGetValue(id, out user);
        public bool TryGetUserFromSecret(uint secret, [NotNullWhen(returnValue: true)] out User? user) => secretToUser.TryGetValue(secret, out user);
        public User GetUserFromId(ushort id) => idToUser[id];
        public User GetUserFromSecret(uint secret) => secretToUser[secret];

        public IEnumerable<User> GetUsersInStage(ushort stage) => stageToSecrets.GetCollectionSecrets(stage).Select(GetUserFromSecret);

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
                stageToSecrets.RemoveSecretFromCollection(user.Stage, user.Secret);
            }
            return expiredUsers;
        }

        public void ChangeUserStage(uint userSecret, ushort newStage)
        {
            if (!secretToUser.TryGetValue(userSecret, out User? user))
                throw new ArgumentException("User with secret doesn't exist", nameof(userSecret));

            stageToSecrets.RemoveSecretFromCollection(user.Stage, user.Secret);

            if (Definitions.StageIds.Contains(newStage))
                stageToSecrets.AddSecretToCollection(user.Stage, user.Secret);

            user.Stage = newStage;
        }
    }
}
