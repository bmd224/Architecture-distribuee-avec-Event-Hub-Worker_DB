using System.Net.Http.Headers;
using System.Text;
using MVC.Models;
using Newtonsoft.Json;

namespace IntegrationTesting
{
    public class IntegrationTest
    {
        private const string HostURL = "http://localhost:50002";

        // créer une fixture pour garder les PostId et CommentId pour ensuite looper des Like

        [Theory]
        [InlineData("Test Title", "User", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]
        [InlineData("Test Title 1", "User 1", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg")]
        [InlineData("Test Title 2", "User 2", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg")]
        [InlineData("Test Title 3", "User 3", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg")]
        [InlineData("Test Title 4", "User 4", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg")]
        [InlineData("Test Title 5", "User 5", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg")]
        [InlineData("Test Title 6", "User 6", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg")]
        [InlineData("Test Title 7", "User 7", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg")]
        [InlineData("Test Title 8", "User 8", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg")]
        [InlineData("Test Title 9", "User 9", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg")]
        [InlineData("Test Title 10", "User 10", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]
        [InlineData("Test Title 11", "User 11", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg")]
        [InlineData("Test Title 12", "User 12", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg")]
        [InlineData("Test Title 13", "User 13", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg")]
        [InlineData("Test Title 14", "User 14", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg")]
        [InlineData("Test Title 15", "User 15", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg")]
        [InlineData("Test Title 16", "User 16", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg")]
        [InlineData("Test Title 17", "User 17", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg")]
        [InlineData("Test Title 18", "User 18", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg")]
        [InlineData("Test Title 19", "User 19", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg")]
        [InlineData("Test Title 20", "User 20", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]
        [InlineData("Test Title 21", "User 21", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg")]
        [InlineData("Test Title 22", "User 22", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg")]
        [InlineData("Test Title 23", "User 23", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg")]
        [InlineData("Test Title 24", "User 24", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg")]
        [InlineData("Test Title 25", "User 25", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg")]
        [InlineData("Test Title 26", "User 26", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg")]
        [InlineData("Test Title 27", "User 27", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg")]
        [InlineData("Test Title 28", "User 28", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg")]
        [InlineData("Test Title 29", "User 29", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg")]
        [InlineData("Test Title 30", "User 30", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]
        [InlineData("Test Title 31", "User 31", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg")]
        [InlineData("Test Title 32", "User 32", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg")]
        [InlineData("Test Title 33", "User 33", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg")]
        [InlineData("Test Title 34", "User 34", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg")]
        [InlineData("Test Title 35", "User 35", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg")]
        [InlineData("Test Title 36", "User 36", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg")]
        [InlineData("Test Title 37", "User 37", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg")]
        [InlineData("Test Title 38", "User 38", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg")]
        [InlineData("Test Title 39", "User 39", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg")]
        [InlineData("Test Title 40", "User 40", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]
        [InlineData("Test Title 41", "User 41", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg")]
        [InlineData("Test Title 42", "User 42", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg")]
        [InlineData("Test Title 43", "User 43", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg")]
        [InlineData("Test Title 44", "User 44", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg")]
        [InlineData("Test Title 45", "User 45", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg")]
        [InlineData("Test Title 46", "User 46", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg")]
        [InlineData("Test Title 47", "User 47", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg")]
        [InlineData("Test Title 48", "User 48", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg")]
        [InlineData("Test Title 49", "User 49", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg")]
        [InlineData("Test Title 50", "User 50", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]
        [InlineData("Test Title 51", "User 51", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg")]
        [InlineData("Test Title 52", "User 52", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg")]
        [InlineData("Test Title 53", "User 53", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg")]
        [InlineData("Test Title 54", "User 54", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg")]
        [InlineData("Test Title 55", "User 55", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg")]
        [InlineData("Test Title 56", "User 56", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg")]
        [InlineData("Test Title 57", "User 57", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg")]
        [InlineData("Test Title 58", "User 58", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg")]
        [InlineData("Test Title 59", "User 59", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg")]
        [InlineData("Test Title 60", "User 60", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]
        [InlineData("Test Title 61", "User 61", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg")]
        [InlineData("Test Title 62", "User 62", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg")]
        [InlineData("Test Title 63", "User 63", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg")]
        [InlineData("Test Title 64", "User 64", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg")]
        [InlineData("Test Title 65", "User 65", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg")]
        [InlineData("Test Title 66", "User 66", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg")]
        [InlineData("Test Title 67", "User 67", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg")]
        [InlineData("Test Title 68", "User 68", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg")]
        [InlineData("Test Title 69", "User 69", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg")]
        [InlineData("Test Title 70", "User 70", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]
        [InlineData("Test Title 71", "User 71", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg")]
        [InlineData("Test Title 72", "User 72", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg")]
        [InlineData("Test Title 73", "User 73", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg")]
        [InlineData("Test Title 74", "User 74", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg")]
        [InlineData("Test Title 75", "User 75", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg")]
        [InlineData("Test Title 76", "User 76", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg")]
        [InlineData("Test Title 77", "User 77", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg")]
        [InlineData("Test Title 78", "User 78", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg")]
        [InlineData("Test Title 79", "User 79", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg")]
        [InlineData("Test Title 80", "User 80", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]
        [InlineData("Test Title 81", "User 81", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg")]
        [InlineData("Test Title 82", "User 82", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg")]
        [InlineData("Test Title 83", "User 83", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg")]
        [InlineData("Test Title 84", "User 84", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg")]
        [InlineData("Test Title 85", "User 85", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg")]
        [InlineData("Test Title 86", "User 86", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg")]
        [InlineData("Test Title 87", "User 87", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg")]
        [InlineData("Test Title 88", "User 88", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg")]
        [InlineData("Test Title 89", "User 89", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg")]
        [InlineData("Test Title 90", "User 90", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]
        [InlineData("Test Title 91", "User 91", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg")]
        [InlineData("Test Title 92", "User 92", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg")]
        [InlineData("Test Title 93", "User 93", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg")]
        [InlineData("Test Title 94", "User 94", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg")]
        [InlineData("Test Title 95", "User 95", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg")]
        [InlineData("Test Title 96", "User 96", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg")]
        [InlineData("Test Title 97", "User 97", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg")]
        [InlineData("Test Title 98", "User 98", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg")]
        [InlineData("Test Title 99", "User 99", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg")]
        [InlineData("Test Title 100", "User 100", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]

        public async Task AddPost_Should_Return_Created_Result(string Title, string User, string Image)
        {
            // Arrange
            HttpClient _client = new HttpClient
            {
                BaseAddress = new Uri(HostURL)
            };

            MemoryStream _stream = new MemoryStream();
            using (var stream = File.OpenRead(Image))
            {
                stream.CopyTo(_stream);
            }

            _stream.Position = 0;

            var imageContent = new ByteArrayContent(_stream.ToArray());
            _stream.Dispose();

            imageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "Image",
                FileName = "test-image.jpg"
            };
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            var content = new MultipartFormDataContent();
            content.Add(imageContent, "Image", "test-image.jpg");
            content.Add(new StringContent(Title), "Title");
            content.Add(new StringContent(Category.Nouvelle.ToString()), "Category");
            content.Add(new StringContent(User), "User");

            // Act
            var response = await _client.PostAsync("/Posts/Add", content);
            response.EnsureSuccessStatusCode();
            var createdPost = await response.Content.ReadAsAsync<PostReadDTO>();

            // Assert
            Assert.Equal(Title, createdPost.Title);
            Assert.Equal(User, createdPost.User);
            Assert.True(createdPost.Id != Guid.Empty);
        }

        [Theory]
        [InlineData("Test GetPost", "User", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]
        [InlineData("Test GetPost 1", "User 1", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg")]
        [InlineData("Test GetPost 2", "User 2", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg")]
        [InlineData("Test GetPost 3", "User 3", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg")]
        [InlineData("Test GetPost 4", "User 4", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg")]
        [InlineData("Test GetPost 5", "User 5", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg")]
        [InlineData("Test GetPost 6", "User 6", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg")]
        [InlineData("Test GetPost 7", "User 7", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg")]
        [InlineData("Test GetPost 8", "User 8", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg")]
        [InlineData("Test GetPost 9", "User 9", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg")]
        public async Task GetPost_ShouldConfirmId(string Title, string User, string Image)
        {
            // Arrange
            HttpClient _client = new HttpClient
            {
                BaseAddress = new Uri(HostURL)
            };

            MemoryStream _stream = new MemoryStream();
            using (var stream = File.OpenRead(Image))
            {
                stream.CopyTo(_stream);
            }

            _stream.Position = 0;

            var imageContent = new ByteArrayContent(_stream.ToArray());
            _stream.Dispose();

            imageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "Image",
                FileName = "test-image.jpg"
            };
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            var content = new MultipartFormDataContent();
            content.Add(imageContent, "Image", "test-image.jpg");
            content.Add(new StringContent(Title), "Title");
            content.Add(new StringContent(Category.Nouvelle.ToString()), "Category");
            content.Add(new StringContent(User), "User");

            var response = await _client.PostAsync("/Posts/Add", content);
            response.EnsureSuccessStatusCode();
            var createdPost = await response.Content.ReadAsAsync<PostReadDTO>();

            // Act
            var response2 = await _client.GetAsync($"/Posts/{createdPost.Id}");
            response2.EnsureSuccessStatusCode();
            var postReadDTO = await response2.Content.ReadAsAsync<PostReadDTO>();

            // Assert
            Assert.Equal(postReadDTO.Id, createdPost.Id);
        }

        [Fact]
        public async Task GetPost()
        {
            // Arrange
            HttpClient _client = new HttpClient
            {
                BaseAddress = new Uri(HostURL)
            };

            // Act
            var response = await _client.GetAsync("/Posts/");
            response.EnsureSuccessStatusCode();
            var createdPost = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(createdPost.Length > 0);

        }

        [Theory]
        [InlineData("Test Comment", "User", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg", "Super commentaire!")]
        [InlineData("Test Comment 1", "User 1", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg", "Autre commentaire")]
        [InlineData("Test Comment 2", "User 2", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg", "Très intéressant!")]
        [InlineData("Test Comment 3", "User 3", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg", "J'adore ça!")]
        [InlineData("Test Comment 4", "User 4", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg", "Commentaire fantastique!")]
        [InlineData("Test Comment 5", "User 5", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg", "Tellement cool!")]
        [InlineData("Test Comment 6", "User 6", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg", "C'est génial!")]
        [InlineData("Test Comment 7", "User 7", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg", "Très bien fait!")]
        [InlineData("Test Comment 8", "User 8", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg", "Impressionnant!")]
        [InlineData("Test Comment 9", "User 9", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg", "Excellente idée!")]
        [InlineData("Test Comment 10", "User 10", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg", "Vraiment intéressant!")]
        [InlineData("Test Comment 11", "User 11", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg", "Très bien!")]
        [InlineData("Test Comment 12", "User 12", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg", "J'aime beaucoup!")]
        [InlineData("Test Comment 13", "User 13", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg", "Excellent travail!")]
        [InlineData("Test Comment 14", "User 14", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg", "Très captivant!")]
        [InlineData("Test Comment 15", "User 15", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg", "Beau travail!")]
        [InlineData("Test Comment 16", "User 16", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg", "Formidable!")]
        [InlineData("Test Comment 17", "User 17", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg", "Incroyable!")]
        [InlineData("Test Comment 18", "User 18", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg", "Ça m'a plu!")]
        [InlineData("Test Comment 19", "User 19", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg", "Super boulot!")]
        [InlineData("Test Comment 20", "User 20", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg", "Bien joué!")]
        [InlineData("Test Comment 21", "User 21", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg", "Remarquable!")]
        [InlineData("Test Comment 22", "User 22", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg", "Bravo!")]
        [InlineData("Test Comment 23", "User 23", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg", "Magnifique!")]
        [InlineData("Test Comment 24", "User 24", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg", "Génial!")]
        [InlineData("Test Comment 25", "User 25", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg", "Très cool!")]
        [InlineData("Test Comment 26", "User 26", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg", "Bien fait!")]
        [InlineData("Test Comment 27", "User 27", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg", "Chouette!")]
        [InlineData("Test Comment 28", "User 28", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg", "Fabuleux!")]
        [InlineData("Test Comment 29", "User 29", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg", "Admirable!")]
        [InlineData("Test Comment 30", "User 30", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg", "Bien pensé!")]
        [InlineData("Test Comment 31", "User 31", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg", "Fascinant!")]
        [InlineData("Test Comment 32", "User 32", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg", "Bravo!")]
        [InlineData("Test Comment 33", "User 33", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg", "Impressionnant!")]
        [InlineData("Test Comment 34", "User 34", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg", "Tellement cool!")]
        [InlineData("Test Comment 35", "User 35", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg", "C'est génial!")]
        [InlineData("Test Comment 36", "User 36", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg", "Très bien fait!")]
        [InlineData("Test Comment 37", "User 37", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg", "Commentaire fantastique!")]
        [InlineData("Test Comment 38", "User 38", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg", "Super boulot!")]
        [InlineData("Test Comment 39", "User 39", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg", "Très intéressant!")]
        [InlineData("Test Comment 40", "User 40", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg", "Vraiment intéressant!")]
        [InlineData("Test Comment 41", "User 41", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg", "C'est génial!")]
        [InlineData("Test Comment 42", "User 42", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg", "Très captivant!")]
        [InlineData("Test Comment 43", "User 43", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg", "Beau travail!")]
        [InlineData("Test Comment 44", "User 44", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg", "Formidable!")]
        [InlineData("Test Comment 45", "User 45", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg", "Incroyable!")]
        [InlineData("Test Comment 46", "User 46", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg", "Bien fait!")]

        public async Task AddComment_Should_Return_Created_Result(string Title, string User, string Image, string Commentaire)
        {
            // Arrange
            HttpClient _client = new HttpClient
            {
                BaseAddress = new Uri(HostURL)
            };

            MemoryStream _stream = new MemoryStream();
            using (var stream = File.OpenRead(Image))
            {
                stream.CopyTo(_stream);
            }

            _stream.Position = 0;

            var imageContent = new ByteArrayContent(_stream.ToArray());
            _stream.Dispose();

            imageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "Image",
                FileName = "test-image.jpg"
            };
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            var content = new MultipartFormDataContent();
            content.Add(imageContent, "Image", "test-image.jpg");
            content.Add(new StringContent(Title), "Title");
            content.Add(new StringContent(Category.Nouvelle.ToString()), "Category");
            content.Add(new StringContent(User), "User");

            // Créez un post pour obtenir un PostId valide
            var response = await _client.PostAsync("/Posts/Add", content);
            response.EnsureSuccessStatusCode();
            var createdPost = await response.Content.ReadAsAsync<PostReadDTO>();

            // Préparer le contenu du commentaire
            var comment = new CommentCreateDTO
            {
                Commentaire = Commentaire,
                User = User,
                PostId = createdPost.Id
            };

            var commentContent = new StringContent(JsonConvert.SerializeObject(comment), Encoding.UTF8, "application/json");

            // Act
            var commentResponse = await _client.PostAsync("/Comments/Add", commentContent);
            commentResponse.EnsureSuccessStatusCode();
            var createdComment = await commentResponse.Content.ReadAsAsync<CommentReadDTO>();

            // Assert
            Assert.Equal(Commentaire, createdComment.Commentaire);
            Assert.Equal(User, createdComment.User);
            Assert.Equal(createdPost.Id, createdComment.PostId);
            Assert.True(createdComment.Id != Guid.Empty);
        }
    }
}
