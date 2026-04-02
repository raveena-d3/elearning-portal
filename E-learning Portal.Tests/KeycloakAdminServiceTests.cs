using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ElearningAPI.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace E_learning_Portal.Tests
{
    public class KeycloakAdminServiceTests
    {
        private static IConfiguration GetConfig()
        {
            var settings = new Dictionary<string, string?>
            {
                ["Keycloak:AdminUrl"] = "http://fake-keycloak",
                ["Keycloak:AdminUsername"] = "admin",
                ["Keycloak:AdminPassword"] = "admin123",
                ["Keycloak:Realm"] = "elearning-realm"
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Return_True_When_User_Deleted()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(request =>
            {
                var url = request.RequestUri!.ToString();

                if (url.Contains("/realms/master/protocol/openid-connect/token"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"access_token\":\"fake-token\"}", Encoding.UTF8, "application/json")
                    };
                }

                if (url.Contains("/users?username=testuser&exact=true"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("[{\"id\":\"user-123\"}]", Encoding.UTF8, "application/json")
                    };
                }

                if (url.Contains("/users/user-123") && request.Method == HttpMethod.Delete)
                {
                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var httpClient = new HttpClient(handler);
            var config = GetConfig();
            var service = new KeycloakAdminService(httpClient, config);

            // Act
            var result = await service.DeleteUserAsync("testuser");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Return_False_When_User_Not_Found()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(request =>
            {
                var url = request.RequestUri!.ToString();

                if (url.Contains("/realms/master/protocol/openid-connect/token"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"access_token\":\"fake-token\"}", Encoding.UTF8, "application/json")
                    };
                }

                if (url.Contains("/users?username=missinguser&exact=true"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("[]", Encoding.UTF8, "application/json")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var httpClient = new HttpClient(handler);
            var config = GetConfig();
            var service = new KeycloakAdminService(httpClient, config);

            // Act
            var result = await service.DeleteUserAsync("missinguser");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateUserAsync_Should_Return_True_When_Create_And_Assign_Role_Succeed()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(request =>
            {
                var url = request.RequestUri!.ToString();

                if (url.Contains("/realms/master/protocol/openid-connect/token"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"access_token\":\"fake-token\"}", Encoding.UTF8, "application/json")
                    };
                }

                if (url.EndsWith("/admin/realms/elearning-realm/users") && request.Method == HttpMethod.Post)
                {
                    return new HttpResponseMessage(HttpStatusCode.Created);
                }

                if (url.Contains("/users?username=newuser&exact=true"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("[{\"id\":\"user-123\"}]", Encoding.UTF8, "application/json")
                    };
                }

                if (url.Contains("/roles/Student"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"id\":\"role-456\",\"name\":\"Student\"}", Encoding.UTF8, "application/json")
                    };
                }

                if (url.Contains("/role-mappings/realm") && request.Method == HttpMethod.Post)
                {
                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var httpClient = new HttpClient(handler);
            var config = GetConfig();
            var service = new KeycloakAdminService(httpClient, config);

            // Act
            var result = await service.CreateUserAsync("newuser", "pass123", "Student");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateUserAsync_Should_Return_False_When_Create_User_Fails()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(request =>
            {
                var url = request.RequestUri!.ToString();

                if (url.Contains("/realms/master/protocol/openid-connect/token"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"access_token\":\"fake-token\"}", Encoding.UTF8, "application/json")
                    };
                }

                if (url.EndsWith("/admin/realms/elearning-realm/users") && request.Method == HttpMethod.Post)
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var httpClient = new HttpClient(handler);
            var config = GetConfig();
            var service = new KeycloakAdminService(httpClient, config);

            // Act
            var result = await service.CreateUserAsync("newuser", "pass123", "Student");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateUserAsync_Should_Return_False_When_Exception_Happens()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(request =>
            {
                throw new HttpRequestException("Network error");
            });

            var httpClient = new HttpClient(handler);
            var config = GetConfig();
            var service = new KeycloakAdminService(httpClient, config);

            // Act
            var result = await service.CreateUserAsync("newuser", "pass123", "Student");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Return_False_When_Exception_Happens()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(request =>
            {
                throw new HttpRequestException("Network error");
            });

            var httpClient = new HttpClient(handler);
            var config = GetConfig();
            var service = new KeycloakAdminService(httpClient, config);

            // Act
            var result = await service.DeleteUserAsync("testuser");

            // Assert
            Assert.False(result);
        }

        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _handlerFunc;

            public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc)
            {
                _handlerFunc = handlerFunc;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(_handlerFunc(request));
            }
        }
    }
}