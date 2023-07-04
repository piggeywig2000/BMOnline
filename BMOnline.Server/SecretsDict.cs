namespace BMOnline.Server
{
    internal class SecretsDict<T> where T : notnull
    {
        private readonly Dictionary<T, HashSet<uint>> backingDict = new Dictionary<T, HashSet<uint>>();

        public SecretsDict()
        {

        }

        public int GetCollectionLength(T collectionId)
        {
            return backingDict.ContainsKey(collectionId) ? backingDict[collectionId].Count : 0;
        }

        public bool IsSecretInCollection(T collectionId, uint secret)
        {
            return backingDict.ContainsKey(collectionId) && backingDict[collectionId].Contains(secret);
        }

        public void AddSecretToCollection(T collectionId, uint secret)
        {
            if (!backingDict.ContainsKey(collectionId))
                backingDict.Add(collectionId, new HashSet<uint>());
            backingDict[collectionId].Add(secret);
        }

        public void RemoveSecretFromCollection(T collectionId, uint secret)
        {
            if (!backingDict.ContainsKey(collectionId)) return;
            backingDict[collectionId].Remove(secret);
            if (backingDict[collectionId].Count == 0) backingDict.Remove(collectionId);
        }

        public ICollection<uint> GetCollectionSecrets(T collectionId)
        {
            if (backingDict.TryGetValue(collectionId, out HashSet<uint>? secrets))
                return secrets;
            else
                return Array.Empty<uint>();
        }
    }
}
