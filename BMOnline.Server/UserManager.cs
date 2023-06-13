using System.Diagnostics.CodeAnalysis;
using BMOnline.Common;

namespace BMOnline.Server
{
    internal class UserManager
    {
        private readonly Dictionary<ushort, User> idToUser = new Dictionary<ushort, User>();
        private readonly Dictionary<uint, User> secretToUser = new Dictionary<uint, User>();
        private readonly SecretsDict<ushort> stageToSecrets = new SecretsDict<ushort>();
        private readonly SecretsDict<byte> courseToSecrets = new SecretsDict<byte>();

        public int TotalCount => secretToUser.Count;
        public IEnumerable<User> Users => secretToUser.Values;

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

        public void RemoveExpired(TimeSpan currentTime)
        {
            User[] expiredUsers = secretToUser.Values.Where(u => u.IsExpired(currentTime)).ToArray();
            foreach (User user in expiredUsers)
            {
                Log.Info($"{user.Name} (ID {user.Id}) disconnected");
                idToUser.Remove(user.Id);
                secretToUser.Remove(user.Secret);
                courseToSecrets.RemoveSecretFromCollection(user.Course, user.Secret);
                stageToSecrets.RemoveSecretFromCollection(user.Stage, user.Secret);
            }
        }

        public void MoveUserToMenu(User user)
        {
            if (!UserSecretExists(user.Secret)) throw new ArgumentException("User doesn't exist", nameof(user));

            if (user.Location != UserLocation.Menu)
            {
                courseToSecrets.RemoveSecretFromCollection(user.Course, user.Secret);
                stageToSecrets.RemoveSecretFromCollection(user.Stage, user.Secret);
                user.Location = UserLocation.Menu;
            }
        }

        public void MoveUserToGame(User user, byte course, ushort stage)
        {
            if (!UserSecretExists(user.Secret)) throw new ArgumentException("User doesn't exist", nameof(user));

            if (user.Location != UserLocation.Game)
            {
                courseToSecrets.RemoveSecretFromCollection(user.Course, user.Secret);
                if (Definitions.CourseIds.Contains(course))
                    courseToSecrets.AddSecretToCollection(course, user.Secret);
                user.Course = course;
                stageToSecrets.RemoveSecretFromCollection(user.Stage, user.Secret);
                if (Definitions.StageIds.Contains(stage))
                    stageToSecrets.AddSecretToCollection(stage, user.Secret);
                user.Stage = stage;
                user.Location = UserLocation.Game;
            }
        }

        public int GetCoursePlayerCount(byte course) => courseToSecrets.GetCollectionLength(course);
        public int GetStagePlayerCount(ushort stage) => stageToSecrets.GetCollectionLength(stage);
    }
}
