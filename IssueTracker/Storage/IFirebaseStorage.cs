using System;
using System.Threading.Tasks;

namespace IssueTracker.Storage
{
    public interface IFirebaseStorage
    {
        public Task<string> Upload(byte[] fileBytes, string fileName);
        public void Delete(string fileName);
    }
}
