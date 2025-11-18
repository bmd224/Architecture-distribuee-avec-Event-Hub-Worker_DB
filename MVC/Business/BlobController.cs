using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using MVC.Models;

namespace MVC.Business
{
    public class BlobController
    {
        // Configuration pour recevoir les ApplicationConfiguration du AppConfig
        private ApplicationConfiguration _applicationConfiguration { get; }

        public BlobController(IOptionsSnapshot<ApplicationConfiguration> options)
        {
            _applicationConfiguration = options.Value;
        }

        public async Task<string> PushImageToBlob(IFormFile formFile, Guid imageGuid)
        {
            // Conversion du fichier recu en IFormFile a Byte[]. 
            // Ensuite le Byte[] sera envoyer au BlobStorage en utilisant un Guid comme identifiant.
            // Nous allons garder le Guid et créer un URL.

            using (MemoryStream ms = new MemoryStream())
            {
                if (ms.Length < 40971520)
                {
                    await formFile.CopyToAsync(ms);

                    //Création du service connection au Blob
                    BlobServiceClient serviceClient = new BlobServiceClient(_applicationConfiguration.BlobConnectionString);

                    //Création du client pour le Blob
                    BlobContainerClient blobClient = serviceClient.GetBlobContainerClient(_applicationConfiguration.UnvalidatedBlob);

                    //Reinitialize le Stream
                    ms.Position = 0;

                    //Envoie de l'image sur le blob
                    await blobClient.UploadBlobAsync(imageGuid.ToString(), ms);
                    return blobClient.Uri.AbsoluteUri + "/" + imageGuid.ToString();
                }
                else
                {
                    throw new ExceptionFilesize();
                }

            }
        }
    }

    // Exception la BusinessLayer pour expliquer que le fichier est trop gros.
    public class ExceptionFilesize : Exception
    { 
        public ExceptionFilesize() { }
    }
}
