using System.Text;
using System.Text.Json;

namespace ElearningAPI.Services
{
    public class KeycloakAdminService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public KeycloakAdminService(
            HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        // Get admin token
        private async Task<string> GetAdminTokenAsync()
        {
            var url = $"{_config["Keycloak:AdminUrl"]}/realms/master/protocol/openid-connect/token";

            var body = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = _config["Keycloak:AdminUsername"]!,
                ["password"] = _config["Keycloak:AdminPassword"]!
            });

            var response = await _http.PostAsync(url, body);
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("access_token").GetString()!;
        }

        // Delete user from Keycloak by username
        public async Task<bool> DeleteUserAsync(string username)
        {
            try
            {
                var token = await GetAdminTokenAsync();
                var realm = _config["Keycloak:Realm"];

                // Find user by username
                var searchUrl = $"{_config["Keycloak:AdminUrl"]}/admin/realms/{realm}/users?username={username}&exact=true";
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", token);

                var searchResponse = await _http.GetAsync(searchUrl);
                var searchJson = await searchResponse.Content
                    .ReadAsStringAsync();
                var users = JsonDocument.Parse(searchJson);
                var userArray = users.RootElement.EnumerateArray()
                    .ToList();

                if (!userArray.Any()) return false;

                var keycloakUserId = userArray[0]
                    .GetProperty("id").GetString();

                // Delete user
                var deleteUrl = $"{_config["Keycloak:AdminUrl"]}/admin/realms/{realm}/users/{keycloakUserId}";
                var deleteResponse = await _http.DeleteAsync(deleteUrl);
                return deleteResponse.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Keycloak delete error: {ex.Message}");
                return false;
            }
        }

        // Create user in Keycloak
        public async Task<bool> CreateUserAsync(
            string username, string password, string role)
        {
            try
            {
                var token = await GetAdminTokenAsync();
                var realm = _config["Keycloak:Realm"];

                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", token);

                // Create user
                var createUrl = $"{_config["Keycloak:AdminUrl"]}/admin/realms/{realm}/users";
                var userBody = JsonSerializer.Serialize(new
                {
                    username = username,
                    enabled = true,
                    emailVerified = true,
                    credentials = new[]
                    {
                        new {
                            type = "password",
                            value = password,
                            temporary = false
                        }
                    }
                });

                var createResponse = await _http.PostAsync(
                    createUrl,
                    new StringContent(
                        userBody,
                        Encoding.UTF8,
                        "application/json"));

                if (!createResponse.IsSuccessStatusCode)
                    return false;

                // Get new user's Keycloak ID
                var searchUrl = $"{_config["Keycloak:AdminUrl"]}/admin/realms/{realm}/users?username={username}&exact=true";
                var searchResponse = await _http.GetAsync(searchUrl);
                var searchJson = await searchResponse.Content
                    .ReadAsStringAsync();
                var users = JsonDocument.Parse(searchJson);
                var keycloakUserId = users.RootElement
                    .EnumerateArray().First()
                    .GetProperty("id").GetString();

                // Get role ID
                var rolesUrl = $"{_config["Keycloak:AdminUrl"]}/admin/realms/{realm}/roles/{role}";
                var rolesResponse = await _http.GetAsync(rolesUrl);
                var rolesJson = await rolesResponse.Content
                    .ReadAsStringAsync();
                var roleDoc = JsonDocument.Parse(rolesJson);
                var roleId = roleDoc.RootElement
                    .GetProperty("id").GetString();
                var roleName = roleDoc.RootElement
                    .GetProperty("name").GetString();

                // Assign role
                var assignUrl = $"{_config["Keycloak:AdminUrl"]}/admin/realms/{realm}/users/{keycloakUserId}/role-mappings/realm";
                var roleBody = JsonSerializer.Serialize(new[]
                {
                    new { id = roleId, name = roleName }
                });

                await _http.PostAsync(
                    assignUrl,
                    new StringContent(
                        roleBody,
                        Encoding.UTF8,
                        "application/json"));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Keycloak create error: {ex.Message}");
                return false;
            }
        }
    }
}