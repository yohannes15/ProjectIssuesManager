using System;
using System.IO;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.Extensions.Options;

namespace IssueTracker.Storage
{
    public class FirebaseFileStorage : IFirebaseStorage
    {
        private readonly FirebaseSettings _settings;

        public FirebaseFileStorage(IOptions<FirebaseSettings> settings)
        {
            _settings = settings.Value;
        }
        
        public async Task<string> Upload(byte[] fileBytes, string fileName)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(_settings.APIKEY));
            var link = await auth.SignInWithEmailAndPasswordAsync(_settings.Email, _settings.Password);

            var task = new FirebaseStorage(
                _settings.Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(link.FirebaseToken),
                    ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
                })
                .Child("Screenshots")
                .Child(fileName)
                .PutAsync(new MemoryStream(fileBytes));

            var percent = "";

            task.Progress.ProgressChanged += (s, e) =>
            {
                percent = $"Progress: {e.Percentage}%";
            };

            return await task;


        }

        public async void Delete(string fileName)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(_settings.APIKEY));
            var link = await auth.SignInWithEmailAndPasswordAsync(_settings.Email, _settings.Password);

            var task = new FirebaseStorage(
                _settings.Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(link.FirebaseToken),
                    ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
                })
                .Child("Screenshots")
                .Child(fileName)
                .DeleteAsync();


        }
    }
}
