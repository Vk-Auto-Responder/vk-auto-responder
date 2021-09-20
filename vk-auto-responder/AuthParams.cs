namespace VkAutoResponder
{
    public class AuthParams
    {
        // USE: https://vkhost.github.io/
        // Allow only messages
        // App ID: 6121396
        // After just substitute the auth_token
        
        public string Login { get; set; }

        public string Password { get; set; }

        public ulong AppId { get; set; }

        public override string ToString()
        {
            return $"{{ Login: {Login}, Password: {Password}, AppId: {AppId}}}";
        }
    }
}