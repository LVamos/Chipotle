using System.IO;

namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFileService
    {
        Stream GetReadStream(FileToken ft); // returns null if the stream was not found.
    } // end IFileService interface

    /// <summary>
    /// for a more generic serialization service
    /// </summary>
    public interface ISerializationService
    {
        // Serialize would traverse the entire object graph, perhaps assigning IDs to each complex object as it goes, so we can handle circular references. It would know about the specific classes used in our project.
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        T Deserialize<T>(Stream stream);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        void Serialize(Stream stream, object obj);
    } // end ISerializationService interface
}