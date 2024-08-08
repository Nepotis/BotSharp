using BotSharp.Abstraction.Knowledges.Models;

namespace BotSharp.Abstraction.VectorStorage;

public interface IVectorDb
{
    Task<IEnumerable<string>> GetCollections();
    Task<StringIdPagedItems<KnowledgeCollectionData>> GetCollectionData(string collectionName, KnowledgeFilter filter);
    Task CreateCollection(string collectionName, int dim);
    Task<bool> Upsert(string collectionName, string id, float[] vector, string text, Dictionary<string, string>? payload = null);
    Task<IEnumerable<string>> Search(string collectionName, float[] vector, string returnFieldName, int limit = 5, float confidence = 0.5f);
    Task<bool> DeleteCollectionData(string collectionName, string id);
}
