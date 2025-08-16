using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoilAPITests.Models;

namespace StorySpoilAPITests
{
    public class StorySpoilAPITests : IDisposable
    {
        private RestClient client;
        private const string baseUrl = Environment.GetEnvironmentVariables(API_BASE_URL);
        private const string user = Environment.GetEnvironmentVariables(USER_NAME);
        private const string password = Environment.GetEnvironmentVariables(USER_PASSWORD);
        private static string lastCreatedStoryId;

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetAccessToken(user, password);
            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetAccessToken(string user, string password)
        {
            var authClient = new RestClient(baseUrl);
            var authRequest = new RestRequest("/api/User/Authentication", Method.Post);
            var loginBody = new LoginBodyDto
            {
                User = user,
                Password = password
            };
            authRequest.AddJsonBody(loginBody);
            var authResponse = authClient.Post(authRequest);

            if (authResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Assert.That(authResponse.Content, Is.Not.Null);

                var authentication = JsonSerializer.Deserialize<AuthenticationResponseDto>(authResponse.Content);
                var accessToken = authentication.AccessToken.ToString();

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new InvalidOperationException("Content is null");
                }
                return accessToken;
            }
            else
            {
                throw new InvalidOperationException($"Authentication failed with {authResponse.StatusCode}");
            }
        }


        [Test, Order(1)]
        public void TestStirySpoil_CreateNewStory_WithAllRequiredFields_ShouldSucceed()
        {
            // Arrange
            string storyName = "New test story";
            string storyDescription = "Some random text";
            string expectedMessage = "Successfully created!";
            var newStory = new StoryDTO
            {
                Title = storyName,
                Description = storyDescription
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newStory);

            // Act
            var response = client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), $"Status code is not as expected - {response.StatusCode}");
            Assert.That(response.Content, Is.Not.Null, "Response content is null");

            var story = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(story.Msg, Is.EqualTo(expectedMessage), "Response message is not as expected");
            Assert.That(story.StoryId, Is.Not.Null);

            lastCreatedStoryId = story.StoryId.ToString();
        }

        [Test, Order(2)]
        public void TestStirySpoil_EditCreatedStory_WithValidData_ShouldSucceed()
        {
            // Arrange
            string newStoryName = "Edited Title";
            string newStoryDescription = "Edited Description";
            string expectedMessage = "Successfully edited";
            var editStory = new StoryDTO
            {
                Title = newStoryName,
                Description = newStoryDescription
            };
            var request = new RestRequest($"/api/Story/Edit/{lastCreatedStoryId}", Method.Put);
            request.AddJsonBody(editStory);

            // Act
            var response = client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Status code is not as expected - {response.StatusCode}");
            Assert.That(response.Content, Is.Not.Null, "Response content is null");

            var story = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(story.Msg, Is.EqualTo(expectedMessage), "Response message is not as expected");
        }

        [Test, Order(3)]
        public void TestStirySpoil_GetAllStories_ShouldReturnAnArray()
        {
            // Arrange           
            var request = new RestRequest($"/api/Story/All", Method.Get);

            // Act
            var response = client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Status code is not as expected - {response.StatusCode}");
            Assert.That(response.Content, Is.Not.Null, "Response content is null");

            var story = JsonSerializer.Deserialize<Get_ResponseDTO[]>(response.Content);

            Assert.That(story.Length, Is.GreaterThan(0), "Returned items are less than one");
        }

        [Test, Order(4)]
        public void TestStirySpoil_RemoveLastCreatedStory_ShouldSucceed()
        {
            // Arrange
            string expectedMessage = "Deleted successfully!";

            var request = new RestRequest($"/api/Story/Delete/{lastCreatedStoryId}", Method.Delete);

            // Act
            var response = client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Status code is not as expected - {response.StatusCode}");
            Assert.That(response.Content, Is.Not.Null, "Response content is null");

            var story = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(story.Msg, Is.EqualTo(expectedMessage), "Response message is not as expected");
        }

        [Test, Order(5)]
        public void TestStirySpoil_CreateNewStory_WithMissingTitleField_ShouldFail()
        {
            // Arrange
            string storyDescription = "Some random text";
            var newStory = new StoryDTO
            {                 
                Description = storyDescription
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newStory);

            // Act
            var response = client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), $"Status code is not as expected - {response.StatusCode}");                                  
        }

        [Test, Order(6)]
        public void TestStirySpoil_EditNonExistingStory_ShouldFail()
        {
            // Arrange
            string wrongStoryId = "1234567";
            string newStoryName = "Edited Title";
            string newStoryDescription = "Edited Description";
            string expectedMessage = "No spoilers...";
            var editStory = new StoryDTO
            {
                Title = newStoryName,
                Description = newStoryDescription
            };
            var request = new RestRequest($"/api/Story/Edit/{wrongStoryId}", Method.Put);
            request.AddJsonBody(editStory);

            // Act
            var response = client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), $"Status code is not as expected - {response.StatusCode}");
            Assert.That(response.Content, Is.Not.Null, "Response content is null");

            var story = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(story.Msg, Is.EqualTo(expectedMessage), "Response message is not as expected");
        }

        [Test, Order(7)]
        public void TestStirySpoil_RemoveNonExistingStory_ShouldFail()
        {
            // Arrange
            string wrongStoryId = "1234567";
            string expectedMessage = "Unable to delete this story spoiler!";

            var request = new RestRequest($"/api/Story/Delete/{wrongStoryId}", Method.Delete);

            // Act
            var response = client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), $"Status code is not as expected - {response.StatusCode}");

            Assert.That(response.Content, Is.Not.Null, "Response content is null");

            var story = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(story.Msg, Is.EqualTo(expectedMessage), "Response message is not as expected");
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}